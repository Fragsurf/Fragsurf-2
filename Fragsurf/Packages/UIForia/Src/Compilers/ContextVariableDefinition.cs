using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Compilers {

    public class ContextVariableDefinition {

        public int id;
        public Type type;
        public string name;
        public LightList<string> nameList;
        public Type contextVarType;
        public AliasResolverType variableType;

        public void PushAlias(string alias) {
            if (nameList == null) {
                nameList = new LightList<string>(4);
            }

            nameList.Add(alias);
        }

        public string GetName() {
            if (nameList != null && nameList.size > 0) {
                return nameList.Last;
            }

            return name;
        }

        public Expression Resolve(LinqCompiler _compiler) {
            UIForiaLinqCompiler compiler = (UIForiaLinqCompiler) _compiler;
            switch (variableType) {
              
                case AliasResolverType.ContextVariable: {
                    ParameterExpression el = compiler.GetElement();
                    Expression access = Expression.MakeMemberAccess(el, TemplateCompiler.s_UIElement_BindingNode);
                    Expression call = ExpressionFactory.CallInstanceUnchecked(access, TemplateCompiler.s_LinqBindingNode_GetContextVariable, Expression.Constant(id));
                    Type varType = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), type);

                    UnaryExpression convert = Expression.Convert(call, varType);
                    ParameterExpression variable = compiler.AddVariable(type, "ctxVar_resolve_" + GetName());

                    compiler.Assign(variable, Expression.MakeMemberAccess(convert, varType.GetField("value")));
                    return variable;
                }
                case AliasResolverType.RepeatItem: {
                    compiler.Comment(name);
                    //var repeat_item_name = element.bindingNode.GetRepeatItem<T>(id).value;
                    ParameterExpression el = compiler.GetElement();
                    Expression access = Expression.MakeMemberAccess(el, TemplateCompiler.s_UIElement_BindingNode);

                    ReflectionUtil.TypeArray1[0] = type;
                    MethodInfo getItem = TemplateCompiler.s_LinqBindingNode_GetRepeatItem.MakeGenericMethod(ReflectionUtil.TypeArray1);
                    Expression call = ExpressionFactory.CallInstanceUnchecked(access, getItem, Expression.Constant(id));
                    Type varType = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), type);

                    ParameterExpression variable = compiler.AddVariable(type, "repeat_item_" + GetName());

                    compiler.Assign(variable, Expression.MakeMemberAccess(call, varType.GetField(nameof(ContextVariable<int>.value))));
                    return variable;
                }
                case AliasResolverType.RepeatIndex: {
                    ParameterExpression el = compiler.GetElement();
                    Expression access = Expression.MakeMemberAccess(el, TemplateCompiler.s_UIElement_BindingNode);
                    Expression call = ExpressionFactory.CallInstanceUnchecked(access, TemplateCompiler.s_LinqBindingNode_GetContextVariable, Expression.Constant(id));

                    UnaryExpression convert = Expression.Convert(call, typeof(ContextVariable<int>));
                    ParameterExpression variable = compiler.AddVariable(type, "repeat_index_" + GetName());

                    compiler.Assign(variable, Expression.MakeMemberAccess(convert, typeof(ContextVariable<int>).GetField(nameof(ContextVariable<int>.value))));
                    return variable;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Expression ResolveType(LinqCompiler _compiler) {
            UIForiaLinqCompiler compiler = (UIForiaLinqCompiler) _compiler;
            switch (variableType) {
                case AliasResolverType.ContextVariable: {
                    return compiler.AddVariable(type, "ctxvar_" + GetName());;
                }
                case AliasResolverType.RepeatItem: {
                    return compiler.AddVariable(type, "repeat_item_" + GetName());
                }
                case AliasResolverType.RepeatIndex: {
                    return compiler.AddVariable(typeof(int), "ctxvar_" + GetName());
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}