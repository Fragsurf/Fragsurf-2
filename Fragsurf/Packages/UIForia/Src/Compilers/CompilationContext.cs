using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Compilers {

    public class CompilationContext {

        public bool outputComments;
        public ProcessedType rootType;

        public CompiledTemplate compiledTemplate;
        public CompiledTemplate innerTemplate; // used when expanding a template to get a reference to the inner template that was expanded

        private readonly LightList<ParameterExpression> variables;
        private readonly LightStack<LightList<Expression>> statementStacks;

        public Expression rootParam;
        public Expression templateScope;
        public Expression applicationExpr;

        private int currentDepth;
        private int maxDepth;
        private int bindingNodeCount;
        public StructList<StyleSheetReference> styleSheets;

        private readonly LightStack<ParameterExpression> hierarchyStack;

        private static readonly MethodInfo s_Comment = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.Comment), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_InlineComment = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.InlineComment), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_CommentNewLineBefore = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.CommentNewLineBefore), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly MethodInfo s_CommentNewLineAfter = typeof(ExpressionUtil).GetMethod(nameof(ExpressionUtil.CommentNewLineAfter), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static readonly char[] s_SplitChar = {'.'};
        public TemplateRootNode templateRootNode;
        public LightList<string> namespaces;

        public CompilationContext(TemplateRootNode templateRootNode) {
            this.outputComments = true;
            this.variables = new LightList<ParameterExpression>();
            this.statementStacks = new LightStack<LightList<Expression>>();
            this.hierarchyStack = new LightStack<ParameterExpression>();
            this.templateRootNode = templateRootNode;
        }

        public ParameterExpression ParentExpr => hierarchyStack.PeekAtUnchecked(hierarchyStack.Count - 2);
        public ParameterExpression ElementExpr => hierarchyStack.PeekUnchecked();

        public Expression ContextExpr; // => Expression.Default(typeof(UIElement));

        public void Initialize(ParameterExpression parent) {
            hierarchyStack.Push(parent);
            PushBlock();
        }

        public void AddStyleSheet(string alias, StyleSheet styleSheet) {
            styleSheets = styleSheets ?? new StructList<StyleSheetReference>();

            if (!string.IsNullOrEmpty(alias)) {
                for (int i = 0; i < styleSheets.size; i++) {
                    if (styleSheets.array[i].alias == alias) {
                        throw new CompileException("Duplicate style sheet alias: " + alias);
                    }
                }
            }

            styleSheets.Add(new StyleSheetReference() {
                alias = alias,
                styleSheet = styleSheet
            });
        }

        public int ResolveStyleNameWithFile(string name, out string file) {
            if (styleSheets == null) {
                file = null;
                return -1;
            }

            string alias = string.Empty;

            if (name.Contains(".")) {
                string[] split = name.Split(s_SplitChar, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2) {
                    alias = split[0];
                    name = split[1];
                }
            }

            if (alias != string.Empty) {
                for (int i = 0; i < styleSheets.size; i++) {
                    if (styleSheets.array[i].alias == alias) {
                        StyleSheet sheet = styleSheets.array[i].styleSheet;
                        for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                            UIStyleGroupContainer styleGroupContainer = sheet.styleGroupContainers[j];
                            if (styleGroupContainer.name == name) {
                                file = sheet.path + ":" + name;
                                return styleGroupContainer.id;
                            }
                        }
                    }
                }
            }
            else {
                for (int i = 0; i < styleSheets.size; i++) {
                    StyleSheet sheet = styleSheets.array[i].styleSheet;
                    if (sheet.TryResolveStyleName(name, out UIStyleGroupContainer retn)) {
                        file = sheet.path + ":" + name;
                        return retn.id;
                    }
                }
            }

            file = null;
            return -1;
        }

        public int ResolveStyleName(string name) {
            if (styleSheets == null) return -1;

            string alias = string.Empty;

            if (name.Contains(".")) {
                string[] split = name.Split(s_SplitChar, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2) {
                    alias = split[0];
                    name = split[1];
                }

            }

            if (alias != string.Empty) {
                for (int i = 0; i < styleSheets.size; i++) {
                    if (styleSheets.array[i].alias == alias) {
                        StyleSheet sheet = styleSheets.array[i].styleSheet;
                        for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                            UIStyleGroupContainer styleGroupContainer = sheet.styleGroupContainers[j];
                            if (styleGroupContainer.name == name) {
                                return styleGroupContainer.id;
                            }
                        }
                    }
                }
            }
            else {
                for (int i = 0; i < styleSheets.size; i++) {
                    StyleSheet sheet = styleSheets.array[i].styleSheet;
                    if (sheet.TryResolveStyleName(name, out UIStyleGroupContainer retn)) {
                        return retn.id;
                    }
                }
            }

            return -1;
        }

        public void PushScope() {
            currentDepth++;

            if (currentDepth > maxDepth) {
                maxDepth = currentDepth;
                ParameterExpression variable = Expression.Parameter(typeof(UIElement), "targetElement_" + currentDepth);
                variables.Add(variable);
                hierarchyStack.Push(variable);
            }
            else {
                string targetName = "targetElement_" + currentDepth;
                for (int i = 0; i < variables.size; i++) {
                    if (variables[i].Type == typeof(UIElement) && variables[i].Name == targetName) {
                        hierarchyStack.Push(variables[i]);
                        return;
                    }
                }

                throw new ArgumentOutOfRangeException();
            }
        }

        public ParameterExpression GetVariable(Type type, string name) {
            for (int i = 0; i < variables.size; i++) {
                if (variables[i].Name == name) {
                    if (variables[i].Type != type) {
                        throw new CompileException("Variable already taken: " + name);
                    }

                    return variables[i];
                }
            }

            ParameterExpression param = Expression.Parameter(type, name);
            variables.Add(param);
            return param;
        }

        public ParameterExpression GetVariable<T>(string name) {
            for (int i = 0; i < variables.size; i++) {
                if (variables[i].Name == name) {
                    if (variables[i].Type != typeof(T)) {
                        throw new CompileException("Variable already taken: " + name);
                    }

                    return variables[i];
                }
            }

            ParameterExpression param = Expression.Parameter(typeof(T), name);
            variables.Add(param);
            return param;
        }

        public void PopScope() {
            currentDepth--;
            hierarchyStack.Pop();
        }

        public void PushBlock() {
            statementStacks.Push(LightList<Expression>.Get());
        }

        public BlockExpression PopBlock() {
            LightList<Expression> statements = statementStacks.Pop();
            Expression[] array = statements.ToArray();
            LightList<Expression>.Release(ref statements);
            return Expression.Block(typeof(void), array);
        }

        public BlockExpression Finalize(Type type) {
            LightList<Expression> statements = statementStacks.Pop();
            Expression[] array = statements.ToArray();
            LightList<Expression>.Release(ref statements);
            return Expression.Block(type, variables, array);
        }

        public void AddStatement(Expression expression) {
            this.statementStacks.PeekUnchecked().Add(expression);
        }

        public void Assign(Expression target, Expression value) {
            AddStatement(Expression.Assign(target, value));
        }

        public void Return(Expression arg) {
            AddStatement(arg);
        }

        public void IfEqualsNull(Expression target, BlockExpression block) {
            AddStatement(Expression.IfThen(Expression.Equal(target, Expression.Constant(null)), block));
        }

        public void Comment(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_Comment, Expression.Constant(comment)));
            }
        }

        public void CommentNewLineBefore(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_CommentNewLineBefore, Expression.Constant(comment)));
            }
        }

        public void CommentNewLineAfter(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_CommentNewLineAfter, Expression.Constant(comment)));
            }
        }

        public void PushContextVariable(string aliasName) {
            throw new NotImplementedException("PushContextVariable");
        }

        public void InlineComment(string comment) {
            if (outputComments) {
                AddStatement(Expression.Call(s_InlineComment, Expression.Constant(comment)));
            }
        }

    }

}