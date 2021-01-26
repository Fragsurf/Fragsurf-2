using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Extensions;
using UIForia.Util;

namespace UIForia.Compilers {

    internal static class ExpressionUtil {

        // used in expressions to output comments in compiled functions
        public static void Comment(string comment) { }
        
        public static void InlineComment(string comment) { }

        // used in expressions to output comments in compiled functions
        public static void CommentNewLineBefore(string comment) { }

        // used in expressions to output comments in compiled functions
        public static void CommentNewLineAfter(string comment) { }

        // public static bool IsConstant(Expression n) {
        //     while (true) {
        //         switch (n) {
        //             case DefaultExpression _:
        //             case ConstantExpression _:
        //                 return true;
        //
        //             case ConditionalExpression conditionalExpression:
        //                 return IsConstant(conditionalExpression.Test) && IsConstant(conditionalExpression.IfTrue) && IsConstant(conditionalExpression.IfFalse);
        //
        //             case UnaryExpression unary:
        //                 n = unary.Operand;
        //                 continue;
        //
        //             case BinaryExpression binaryExpression:
        //                 return IsConstant(binaryExpression.Left) && IsConstant(binaryExpression.Right);
        //         }
        //
        //         return false;
        //     }
        // }

        internal struct ParameterConversion {

            public readonly Expression expression;
            public readonly bool requiresConversion;
            public readonly MethodInfo userConversion;
            public readonly Type convertTo;
            public readonly object defaultValue;

            public ParameterConversion(Expression expression, bool requiresConversion, Type convertTo, MethodInfo userConversion = null, object defaultValue = null) {
                this.expression = expression;
                this.requiresConversion = requiresConversion;
                this.userConversion = userConversion;
                this.convertTo = convertTo;
                this.defaultValue = defaultValue;
            }

            public Expression Convert() {
                if (!requiresConversion) {
                    if (defaultValue != null) {
                        return Expression.Constant(defaultValue);
                    }

                    return expression;
                }

                return ExpressionFactory.Convert(expression, convertTo, userConversion);
            }

        }

        internal static MethodInfo SelectEligibleMethod(IList<MethodInfo> methodInfos, Expression[] arguments, StructList<ParameterConversion> winningConversions) {
            if (methodInfos.Count == 0) {
                return null;
            }

            if (methodInfos.Count == 1) {
                if (CheckCandidate(new Candidate(methodInfos[0].GetParameters()), arguments, out int unused, winningConversions)) {
                    return methodInfos[0];
                }

                return null;
            }

            StructList<Candidate> candidates = StructList<Candidate>.GetMinSize(methodInfos.Count);
            StructList<ParameterConversion> conversions = StructList<ParameterConversion>.Get();

            int argCount = arguments.Length;

            for (int i = 0; i < methodInfos.Count; i++) {
                MethodInfo methodInfo = methodInfos[i];
                ParameterInfo[] parameterInfos = methodInfo.GetParametersCached();

                if (parameterInfos.Length == argCount) {
                    candidates.Add(new Candidate(methodInfo, parameterInfos));
                }
                else if (parameterInfos.Length > argCount) {
                    bool valid = true;
                    for (int j = 0; j < parameterInfos.Length; j++) {
                        if (!parameterInfos[j].HasDefaultValue) {
                            valid = false;
                            break;
                        }
                    }

                    if (valid) {
                        candidates.Add(new Candidate(methodInfo, parameterInfos));
                    }
                }
            }

            int winner = -1;
            int winnerPoints = -1;

            for (int i = 0; i < candidates.Count; i++) {
                int candidatePoints;
                conversions.QuickClear();

                if (!CheckCandidate(candidates[i], arguments, out candidatePoints, conversions)) {
                    continue;
                }

                // todo -- handle the ambiguous case
                if (BestScoreSoFar(candidatePoints, winnerPoints)) {
                    winner = i;
                    winnerPoints = candidatePoints;

                    winningConversions.QuickClear();
                    winningConversions.AddRange(conversions);
                }
            }

            StructList<ParameterConversion>.Release(ref conversions);

            if (winner != -1) {
                MethodInfo retn = candidates[winner].methodInfo;
                StructList<Candidate>.Release(ref candidates);
                return retn;
            }

            return null;
        }

        internal static ConstructorInfo SelectEligibleConstructor(Type type, Expression[] arguments, StructList<ParameterConversion> winningConversions) {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            if (constructors.Length == 0) {
                return null;
            }

            if (constructors.Length == 1) {
                if (CheckCandidate(new Candidate(constructors[0].GetParametersCached()), arguments, out int unused, winningConversions)) {
                    return constructors[0];
                }

                return null;
            }

            StructList<Candidate> candidates = StructList<Candidate>.GetMinSize(constructors.Length);
            StructList<ParameterConversion> conversions = StructList<ParameterConversion>.Get();

            for (int i = 0; i < constructors.Length; i++) {
                candidates[i] = new Candidate(constructors[i].GetParametersCached());
            }

            int winner = -1;
            int winnerPoints = 0;

            for (int i = 0; i < constructors.Length; i++) {
                int candidatePoints;
                conversions.QuickClear();

                if (!CheckCandidate(candidates[i], arguments, out candidatePoints, conversions)) {
                    continue;
                }

                // todo -- handle the ambiguous case
                if (BestScoreSoFar(candidatePoints, winnerPoints)) {
                    winner = i;
                    winnerPoints = candidatePoints;
                    winningConversions.QuickClear();
                    winningConversions.AddRange(conversions);
                }
            }

            StructList<Candidate>.Release(ref candidates);
            StructList<ParameterConversion>.Release(ref conversions);

            if (winner != -1) {
                return constructors[winner];
            }

            return null;
        }

        public struct Candidate {

            public MethodInfo methodInfo;
            public ParameterInfo[] dependencies;

            public Candidate(ParameterInfo[] dependencies) {
                this.methodInfo = null;
                this.dependencies = dependencies;
            }

            public Candidate(MethodInfo methodInfo, ParameterInfo[] dependencies) {
                this.methodInfo = methodInfo;
                this.dependencies = dependencies;
            }

        }

        private static bool BestScoreSoFar(int candidatePoints, int winnerPoints) {
            return winnerPoints < candidatePoints;
        }

        private static bool CheckCandidate(Candidate candidate, Expression[] context, out int candidatePoints, StructList<ParameterConversion> conversions) {
            candidatePoints = 0;

            if (context.Length > candidate.dependencies.Length) {
                candidatePoints = 0;
                return false;
            }

            for (int i = 0; i < candidate.dependencies.Length; i++) {
                if (i < context.Length) {
                    Type paramType = candidate.dependencies[i].ParameterType;
                    Type argType = context[i].Type;
                    if (paramType == argType) {
                        candidatePoints += 100;
                        conversions.Add(new ParameterConversion(context[i], false, paramType));
                    }
                    else if (TypeUtil.HasIdentityPrimitiveOrNullableConversion(argType, paramType)) {
                        candidatePoints += 50;
                        conversions.Add(new ParameterConversion(context[i], true, paramType));
                    }
                    else if (TypeUtil.HasReferenceConversion(argType, paramType)) {
                        conversions.Add(new ParameterConversion(context[i], true, paramType));
                    }
                    else if (TypeUtil.TryGetUserDefinedCoercionMethod(argType, paramType, false, out MethodInfo info)) {
                        conversions.Add(new ParameterConversion(context[i], true, paramType, info));
                    }
                    else {
                        candidatePoints = 0;
                        return false;
                    }
                }
                else if (candidate.dependencies[i].HasDefaultValue) {
                    candidatePoints += 1;
                    conversions.Add(new ParameterConversion(null, false, null, null, candidate.dependencies[i].DefaultValue));
                }
                else {
                    candidatePoints = 0;
                    return false;
                }
            }

            return true;
        }

    }

}