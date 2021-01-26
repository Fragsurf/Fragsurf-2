using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UIForia.Compilers {

    internal static class ExpressionFactory {

        private static Func<Expression, Type, MethodInfo, UnaryExpression> s_ConversionFactory;
        private static Func<Expression, Expression, BinaryExpression> s_AssignmentFactory;
        private static Func<MethodInfo, Expression, IReadOnlyList<Expression>, MethodCallExpression> s_InstanceCallFactory;
        private static Func<MethodInfo, IReadOnlyList<Expression>, MethodCallExpression> s_StaticCallFactory;

        public static Expression Convert(Expression expression, Type type, MethodInfo methodInfo = null) {
            return GetConversionFactory().Invoke(expression, type, methodInfo);
        }

        public static Expression AssignUnchecked(Expression target, Expression src) {
            return GetAssignmentFactory().Invoke(target, src);
        }

        public static MethodCallExpression CallInstanceUnchecked(Expression target, MethodInfo method, params Expression[] arguments) {
            if(method == null) throw new NullReferenceException();
            return GetInstanceCall0Factory().Invoke(method, target, new ReadOnlyCollection<Expression>(arguments));
        }

        public static Expression CallStaticUnchecked(MethodInfo method, params Expression[] arguments) {
            if(method == null) throw new NullReferenceException();
            return GetStaticCallFactory().Invoke(method, new ReadOnlyCollection<Expression>(arguments));
        }

        private static Func<Expression, Type, MethodInfo, UnaryExpression> GetConversionFactory() {
            if (s_ConversionFactory != null) return s_ConversionFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.UnaryExpression");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

            var expressionParam = Expression.Parameter(typeof(Expression), "expression");
            var typeParam = Expression.Parameter(typeof(Type), "type");
            var methodParam = Expression.Parameter(typeof(MethodInfo), "methodInfo");

            var newExpression = Expression.New(info, Expression.Constant(ExpressionType.Convert), expressionParam, typeParam, methodParam);
            BlockExpression block = Expression.Block(newExpression);
            s_ConversionFactory = Expression.Lambda<Func<Expression, Type, MethodInfo, UnaryExpression>>(block, expressionParam, typeParam, methodParam).Compile();
            return s_ConversionFactory;
        }

        private static Func<Expression, Expression, BinaryExpression> GetAssignmentFactory() {
            if (s_AssignmentFactory != null) return s_AssignmentFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.AssignBinaryExpression");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            ParameterExpression dstParam = Expression.Parameter(typeof(Expression), "dst");
            ParameterExpression srcParam = Expression.Parameter(typeof(Expression), "src");
            NewExpression newExpression = Expression.New(info, dstParam, srcParam);
            BlockExpression block = Expression.Block(newExpression);
            s_AssignmentFactory = Expression.Lambda<Func<Expression, Expression, BinaryExpression>>(block, dstParam, srcParam).Compile();
            return s_AssignmentFactory;
        }

        private static Func<MethodInfo, Expression, IReadOnlyList<Expression>, MethodCallExpression> GetInstanceCall0Factory() {
            if (s_InstanceCallFactory != null) return s_InstanceCallFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.InstanceMethodCallExpressionN");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
            ParameterExpression targetParam = Expression.Parameter(typeof(Expression), "expression");
            ParameterExpression methodParam = Expression.Parameter(typeof(MethodInfo), "methodInfo");
            ParameterExpression args = Expression.Parameter(typeof(IReadOnlyList<Expression>), "args");
            NewExpression newExpression = Expression.New(info, methodParam, targetParam, args);
            BlockExpression block = Expression.Block(newExpression);
            s_InstanceCallFactory = Expression.Lambda<Func<MethodInfo, Expression, IReadOnlyList<Expression>, MethodCallExpression>>(block, methodParam, targetParam, args).Compile();
            return s_InstanceCallFactory;
        }

        private static Func<MethodInfo, IReadOnlyList<Expression>, MethodCallExpression> GetStaticCallFactory() {
            if (s_StaticCallFactory != null) return s_StaticCallFactory;
            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.MethodCallExpressionN");
            ConstructorInfo info = cheating.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0];
            ParameterExpression methodParam = Expression.Parameter(typeof(MethodInfo), "methodInfo");
            ParameterExpression args = Expression.Parameter(typeof(IReadOnlyList<Expression>), "args");
            NewExpression newExpression = Expression.New(info, methodParam, args);
            BlockExpression block = Expression.Block(newExpression);
            s_StaticCallFactory = Expression.Lambda<Func<MethodInfo, IReadOnlyList<Expression>, MethodCallExpression>>(block, methodParam, args).Compile();
            return s_StaticCallFactory;
        }

    }

}