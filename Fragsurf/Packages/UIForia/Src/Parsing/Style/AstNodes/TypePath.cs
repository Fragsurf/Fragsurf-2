using System.Collections.Generic;
using System.Text;
using UIForia.Util;

namespace UIForia.Parsing.Style.AstNodes {

    public struct TypePath {

        public List<string> path;
        public List<TypePath> genericArguments;

        public void Release() {
            ListPool<string>.Release(ref path);
            ReleaseGenerics();
        }

        public void ReleaseGenerics() {
            if (genericArguments != null && genericArguments.Count > 0) {
                for (int i = 0; i < genericArguments.Count; i++) {
                    genericArguments[i].Release();
                }

                ListPool<TypePath>.Release(ref genericArguments);
                genericArguments = null;
            }
        }

        private static readonly StringBuilder s_Builder = new StringBuilder(128);

        private void GetConstructedPathStep() {
            for (int i = 0; i < path.Count - 1; i++) {
                s_Builder.Append(path[i]);
                s_Builder.Append('.');
            }
            
            s_Builder.Append(path[path.Count - 1]);
            if (genericArguments != null && genericArguments.Count > 0) {
                s_Builder.Append('`');
                s_Builder.Append(genericArguments.Count);
                s_Builder.Append('[');
                for (int i = 0; i < genericArguments.Count; i++) {
                    genericArguments[i].GetConstructedPathStep();
                    if (i != genericArguments.Count - 1) {
                        s_Builder.Append(',');
                    }
                }

                s_Builder.Append(']');
            }
        }

        public string GetConstructedPath() {
            if (path == null) {
                return string.Empty;
            }

            GetConstructedPathStep();
            string retn = s_Builder.ToString();
            s_Builder.Clear();
            return retn;
        }

    }

}