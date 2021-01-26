using System;
using System.Linq.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class BlockDefinition2 {

        public BlockDefinition2 parent;
        private StructList<Parameter> variables;
        private LightList<Expression> statements;
        public LinqCompiler compiler;
        public int blockId;

        public BlockDefinition2() {
            this.variables = StructList<Parameter>.Get();
            this.statements = LightList<Expression>.Get();
        }

        public bool HasStatements => statements.Count > 0;
        public int StatementCount => statements.size;

        public ParameterExpression AddInternalVariable(Type type, string name = null, ParameterFlags flags = 0) {

            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) {
                name = "_var$";
            }

            variables = variables ?? new StructList<Parameter>();

            ParameterExpression retn = Expression.Parameter(type, compiler.GetUniqueVariableName(name));

            variables.Add(new Parameter() {
                name = retn.Name,
                type = retn.Type,
                expression = retn,
                flags = flags
            });
            return retn;
        }

        public LightList<Expression> GetStatements() {
            return statements;
        }

        public ParameterExpression[] GetVariables() {
            ParameterExpression[] retn = new ParameterExpression[variables.size];
            for (int i = 0; i < retn.Length; i++) {
                retn[i] = variables[i].expression;
            }

            return retn;
        }

        public Expression AddStatement(Expression statement) {
            if (compiler.addingStatements) {
                statements.Add(statement);
            }

            return statement;
        }

        public Parameter? ResolveVariable(string variableName) {
            if (variables == null) return default;

            for (int i = 0; i < variables.Count; i++) {
                if (variables[i].name == variableName) {
                    return variables[i];
                }
            }

            // does this hide closures?
            return parent?.ResolveVariable(variableName);
        }

        public void Spawn() {
            this.variables = StructList<Parameter>.Get();
            this.statements = LightList<Expression>.Get();
        }

        public void Release() {
            parent = null;
            StructList<Parameter>.Release(ref variables);
            LightList<Expression>.Release(ref statements);
        }

        public BlockExpression ToBlockExpression() {
            if (variables != null && variables.Count > 0) {
                return Expression.Block(typeof(void), GetVariables(), statements);
            }
            else {
                return Expression.Block(typeof(void), statements);
            }
        }

        public ParameterExpression AddUserVariable(Parameter parameter) {
            variables.Add(parameter);
            return variables[variables.size - 1].expression;
        }

        public bool TryGetUserVariable(Expression expression, out Parameter parameter) {
            for (int i = 0; i < variables.size; i++) {
                if (variables[i].expression == expression) {
                    parameter = variables[i];
                    return true;
                }
            }

            if (parent != null) {
                return parent.TryGetUserVariable(expression, out parameter);
            }

            parameter = default;
            return false;
        }

        public Expression GetLastStatement() {
            return statements[statements.size - 1];
        }

    }

}