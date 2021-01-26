using System;
using UIForia.Util;

namespace UIForia {

    public struct TypeLookup {

        public string typeName;
        public string namespaceName;
        public StructList<TypeLookup> generics;
        public bool isArray;
        public Type resolvedType;

        public TypeLookup(string typeName) {
            this.typeName = typeName;
            this.namespaceName = null;
            this.generics = null;
            this.isArray = false;
            this.resolvedType = null;
        }
        
        
        public TypeLookup(Type type) {
            this.resolvedType = type;
            this.typeName = null;
            this.namespaceName = null;
            this.generics = null;
            this.isArray = false;
        }

        public void Release() {
            typeName = null;
            namespaceName = null;
            if (generics != null) {
                for (int i = 0; i < generics.Count; i++) {
                    generics[i].Release();
                }
                StructList<TypeLookup>.Release(ref generics);
            }
        }

        public string GetBaseTypeName() {
            if (generics == null || generics.Count == 0) {
                return typeName;
            }

            return typeName + "`" + generics.Count;
        }
        
        public override string ToString() {
            string retn = "";
            
            if (!string.IsNullOrEmpty(namespaceName) && !string.IsNullOrWhiteSpace(namespaceName)) {
                retn += namespaceName + ".";
            }

            retn += typeName;
            if (generics != null && generics.Count > 0) {
                retn += "[";
                for (int i = 0; i < generics.Count; i++) {
                    retn += generics[i].ToString();
                    if (i != generics.Count - 1) {
                        retn += ",";
                    }
                }

                retn += "]";
            }

            return retn;
        }

    }

}