//using System.Collections.Generic;
//using System.Text;
//using UIForia.Util;
//
//namespace UIForia.Parsing.Expression.AstNodes {
//
//    // todo -- remove type path and replace with TypeLookup
//    public struct TypePath {
//
//        public List<string> path;
//        public List<TypePath> genericArguments;
//
//        public void Release() {
//            ListPool<string>.Release(ref path);
//            ReleaseGenerics();
//        }
//
//        public void ReleaseGenerics() {
//            if (genericArguments != null && genericArguments.Count > 0) {
//                for (int i = 0; i < genericArguments.Count; i++) {
//                    genericArguments[i].Release();
//                }
//
//                ListPool<TypePath>.Release(ref genericArguments);
//                genericArguments = null;
//            }
//        }
//
//        private static readonly StringBuilder s_Builder = new StringBuilder(128);
//
//        private void GetConstructedPathStep() {
//            for (int i = 0; i < path.Count - 1; i++) {
//                s_Builder.Append(path[i]);
//                s_Builder.Append('.');
//            }
//
//            s_Builder.Append(path[path.Count - 1]);
//            if (genericArguments != null && genericArguments.Count > 0) {
//                s_Builder.Append('`');
//                s_Builder.Append(genericArguments.Count);
//                s_Builder.Append('[');
//                for (int i = 0; i < genericArguments.Count; i++) {
//                    genericArguments[i].GetConstructedPathStep();
//                    if (i != genericArguments.Count - 1) {
//                        s_Builder.Append(',');
//                    }
//                }
//
//                s_Builder.Append(']');
//            }
//        }
//
//        public TypeLookup ConstructTypeLookupTree() {
//            if (path == null) {
//                return default;
//            }
//
//            TypeLookup t = new TypeLookup();
//            s_Builder.Clear();
//            for (int i = 0; i < path.Count - 1; i++) {
//                s_Builder.Append(path[i]);
//                if (i != path.Count - 2) {
//                    s_Builder.Append('.');
//                }
//            }
//
//            t.namespaceName = s_Builder.ToString();
//            t.typeName = path[path.Count - 1];
//
//            if (genericArguments != null && genericArguments.Count > 0) {
//                t.typeName += "`" + genericArguments.Count;
//                t.generics = new TypeLookup[genericArguments.Count];
//                for (int i = 0; i < genericArguments.Count; i++) {
//                    t.generics[i] = genericArguments[i].ConstructTypeLookupTree();
//                }
//            }
//
//            return t;
//        }
//
//        public string GetConstructedPath() {
//            if (path == null) {
//                return string.Empty;
//            }
//
//            GetConstructedPathStep();
//            string retn = s_Builder.ToString();
//            s_Builder.Clear();
//            return retn;
//        }
//
//    }
//
//}