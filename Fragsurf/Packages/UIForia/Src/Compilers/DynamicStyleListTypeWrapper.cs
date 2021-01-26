using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Compilers.Style;
using UIForia.Rendering;

namespace UIForia.Compilers {

    public class DynamicStyleListTypeWrapper : ITypeWrapper {

        private static readonly ConstructorInfo s_StringCtor = typeof(DynamicStyleList).GetConstructor(new[] {typeof(string)});
        private static readonly ConstructorInfo s_StringListCtor = typeof(DynamicStyleList).GetConstructor(new[] {typeof(IList<string>)});
        private static readonly ConstructorInfo s_StyleRefCtor = typeof(DynamicStyleList).GetConstructor(new[] {typeof(UIStyleGroupContainer)});
        private static readonly ConstructorInfo s_StyleRefListCtor = typeof(DynamicStyleList).GetConstructor(new[] {typeof(IList<UIStyleGroupContainer>)});
        private static readonly ConstructorInfo s_CharArrayCtor = typeof(DynamicStyleList).GetConstructor(new[] {typeof(char[])});

        public Expression Wrap(Type targetType, Expression input) {
            if (targetType != typeof(DynamicStyleList)) {
                return null;
            }

            Type inputType = input.Type;

            if (inputType == typeof(string)) {
                return Expression.New(s_StringCtor, input);
            }
            else if (inputType == typeof(char[])) {
                return Expression.New(s_CharArrayCtor, input);
            }
            else if (typeof(IList<string>).IsAssignableFrom(inputType)) {
                return Expression.New(s_StringListCtor, input);
            }
            else if (inputType == typeof(UIStyleGroupContainer)) {
                return Expression.New(s_StyleRefCtor, input);
            }
            else if (typeof(IList<UIStyleGroupContainer>).IsAssignableFrom(inputType)) {
                return Expression.New(s_StyleRefListCtor, input);
            }

            return null;
        }

    }

}