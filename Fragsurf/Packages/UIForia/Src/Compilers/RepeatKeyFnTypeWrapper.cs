using System;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;

namespace UIForia.Compilers {

    public class RepeatKeyFnTypeWrapper : ITypeWrapper {

        private static readonly ConstructorInfo s_StringCtor = typeof(RepeatItemKey).GetConstructor(new[] {typeof(string)});
        private static readonly ConstructorInfo s_LongCtor = typeof(RepeatItemKey).GetConstructor(new[] {typeof(long)});
        private static readonly ConstructorInfo s_IntCtor = typeof(RepeatItemKey).GetConstructor(new[] {typeof(int)});

        public Expression Wrap(Type targetType, Expression input) {
            if (targetType != typeof(RepeatItemKey)) {
                return null;
            }

            Type inputType = input.Type;
            if (inputType == typeof(string)) {
                return Expression.New(s_StringCtor, input);
            }

            if (inputType == typeof(int)) {
                return Expression.New(s_IntCtor, input);
            }

            if (inputType == typeof(long)) {
                return Expression.New(s_LongCtor, input);
            }

            return null;
        }

    }

}