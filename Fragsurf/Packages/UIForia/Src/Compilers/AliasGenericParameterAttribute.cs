using System;

namespace UIForia.Compilers {

    public sealed class AliasGenericParameterAttribute : Attribute {

        public string aliasName;
        public int parameterIndex;

        public AliasGenericParameterAttribute(int index, string aliasName) {
            this.parameterIndex = index;
            this.aliasName = aliasName;
        }

    }

}