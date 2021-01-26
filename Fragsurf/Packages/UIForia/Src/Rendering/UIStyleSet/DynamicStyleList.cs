using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Util;

namespace UIForia.Rendering {

    public struct DynamicStyleList {

        public readonly object data;
        private readonly DataType type;

        private enum DataType {

            String,
            StringList,
            StyleRef,
            StyleRefList,
            CharArray

        }

        public DynamicStyleList(string styleName) {
            type = DataType.String;
            data = styleName;
        }

        public DynamicStyleList(char[] styleName) {
            type = DataType.CharArray;
            data = styleName;
        }

        public DynamicStyleList(IList<string> styleList) {
            type = DataType.StringList;
            data = styleList;
        }

        public DynamicStyleList(UIStyleGroupContainer styleRef) {
            type = DataType.StyleRef;
            data = styleRef;
        }

        public DynamicStyleList(IList<UIStyleGroupContainer> styleRefList) {
            type = DataType.StyleRefList;
            data = styleRefList;
        }

        public void Flatten(TemplateMetaData metaData, LightList<UIStyleGroupContainer> containers) {
            if (data == null) return;

            switch (type) {
                case DataType.String: {
                    UIStyleGroupContainer style = metaData.ResolveStyleByName((string) data);
                    if (style != null) {
                        containers.Add(style);
                    }

                    break;
                }

                case DataType.CharArray: {
                    UIStyleGroupContainer style = metaData.ResolveStyleByName((char[]) data);
                    if (style != null) {
                        containers.Add(style);
                    }

                    break;
                }

                case DataType.StringList: {
                    IList<string> list = (IList<string>) data;
                    for (int i = 0; i < list.Count; i++) {
                        UIStyleGroupContainer style = metaData.ResolveStyleByName(list[i]);
                        if (style != null) {
                            containers.Add(style);
                        }
                    }

                    break;
                }

                case DataType.StyleRef: {
                    containers.Add((UIStyleGroupContainer) data);
                    break;
                }

                case DataType.StyleRefList: {
                    IList<UIStyleGroupContainer> list = (IList<UIStyleGroupContainer>) data;
                    for (int i = 0; i < list.Count; i++) {
                        if (list[i] == null) continue;
                        containers.Add(list[i]);
                    }

                    break;
                }
            }
        }

    }

}