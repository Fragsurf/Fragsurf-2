using System;
using System.Collections.Generic;
using UIForia.Animation;
using UIForia.Compilers.Style;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Compilers {

    public struct StyleSheetReference {

        public string alias;
        public StyleSheet styleSheet;

        public StyleSheetReference(string alias, StyleSheet styleSheet) {
            this.alias = alias;
            this.styleSheet = styleSheet;
        }

    }

    public class TemplateMetaData {

        public int id;
        public string filePath;
        public StyleSheetReference[] styleReferences;

        public CompiledTemplateData compiledTemplateData;
        internal UIStyleGroupContainer[] styleMap;

        private IndexedAnimationRef[] animationSearchMap;
        private IndexedStyleRef[] styleSearchMap;

        private static readonly IndexedStyleRef[] s_EmptySearchMap = { };
        private static readonly IndexedAnimationRef[] s_EmptyAnimationSearchMap = { };

        public TemplateMetaData(int id, string filePath, UIStyleGroupContainer[] styleMap, StyleSheetReference[] styleReferences) {
            this.id = id;
            this.filePath = filePath;
            this.styleReferences = styleReferences;
            this.styleMap = styleMap;
        }

        public void BuildSearchMap() {
            if (styleSearchMap != null) {
                return;
            }

            if (styleReferences == null) {
                styleSearchMap = s_EmptySearchMap;
                animationSearchMap = s_EmptyAnimationSearchMap;
                return;
            }

            int animCount = 0;
            int styleCount = 0;

            for (int i = 0; i < styleReferences.Length; i++) {
                styleCount += styleReferences[i].styleSheet.styleGroupContainers.Length;
                animCount += styleReferences[i].styleSheet.animations.Length;
            }

            if (styleCount != 0) {
                styleSearchMap = new IndexedStyleRef[styleCount];

                int cnt = 0;

                for (int i = 0; i < styleReferences.Length; i++) {
                    string alias = styleReferences[i].alias;
                    StyleSheet sheet = styleReferences[i].styleSheet;

                    if (alias != null && alias.Length != 0) {
                        for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                            string name = sheet.styleGroupContainers[j].name;
                            styleSearchMap[cnt++] = new IndexedStyleRef(i, name, alias + "." + name, sheet.styleGroupContainers[j]);
                        }
                    }
                    else {
                        for (int j = 0; j < sheet.styleGroupContainers.Length; j++) {
                            string name = sheet.styleGroupContainers[j].name;
                            styleSearchMap[cnt++] = new IndexedStyleRef(i, name, null, sheet.styleGroupContainers[j]);
                        }
                    }
                }

                Array.Sort(styleSearchMap, (a, b) => {
                    if (a.name != b.name) {
                        return string.CompareOrdinal(a.name, b.name);
                    }

                    return a.index - b.index;
                });
            }
            else {
                styleSearchMap = s_EmptySearchMap;
            }

            if (animCount != 0) {
                int cnt = 0;

                animationSearchMap = new IndexedAnimationRef[animCount];
                for (int i = 0; i < styleReferences.Length; i++) {
                    string alias = styleReferences[i].alias;
                    StyleSheet sheet = styleReferences[i].styleSheet;

                    AnimationData[] animations = sheet.animations;
                    if (alias != null && alias.Length != 0) {
                        for (int j = 0; j < animations.Length; j++) {
                            ref AnimationData animationData = ref animations[j];
                            animationSearchMap[cnt++] = new IndexedAnimationRef(alias + "." + animationData.name, animationData);
                        }
                    }
                    else {
                        for (int j = 0; j < animations.Length; j++) {
                            ref AnimationData animationData = ref animations[j];
                            animationSearchMap[cnt++] = new IndexedAnimationRef(animationData.name, animationData);
                        }
                    }
                }

                Array.Sort(animationSearchMap, (a, b) => string.CompareOrdinal(a.name, b.name));
            }
            else {
                animationSearchMap = s_EmptyAnimationSearchMap;
            }
        }

        private struct IndexedStyleRef {

            public readonly int index;
            public readonly string name;
            public readonly string aliasedName;
            public readonly UIStyleGroupContainer container;

            public IndexedStyleRef(int index, string name, string aliasedName, UIStyleGroupContainer container) {
                this.index = index;
                this.name = name;
                this.aliasedName = aliasedName;
                this.container = container;
            }

        }

        private struct IndexedAnimationRef {

            public readonly string name;
            public readonly AnimationData animation;

            public IndexedAnimationRef(string aliasedName, in AnimationData animation) {
                this.name = aliasedName;
                this.animation = animation;
            }

        }

        public UIStyleGroupContainer ResolveStyleByName(string name) {
            if (string.IsNullOrEmpty(name) || styleSearchMap == null) {
                return null;
            }

            int idx = BinarySearchStyle(name);

            if (idx >= 0) {
                while (idx > 0) {
                    if (styleSearchMap[idx - 1].name == name) {
                        idx--;
                    }
                    else {
                        return styleSearchMap[idx].container;
                    }
                }

                return styleSearchMap[idx].container;
            }
            else {
                idx = BinarySearchAliasedStyle(name);
            }

            return idx >= 0 ? styleSearchMap[idx].container : null;
        }

        public int ResolveStyleNameSlow(string name) {
            if (styleReferences == null) return -1;

            BuildSearchMap();

            int originalIndex = BinarySearchStyle(name);
            int idx = originalIndex;
            
            if (idx >= 0) {
                while (idx > 0) {
                    if (styleSearchMap[idx - 1].name == name) {
                        // Debug.LogWarning("Duplicate style " + name);
                        idx--;
                    }
                    else {
                        return idx;
                    }
                }

                return idx;
            }
            else {
                idx = BinarySearchAliasedStyle(name);
            }

            return idx;
        }

        public UIStyleGroupContainer ResolveStyleByName(char[] name) {
            if (name == null || name.Length == 0) return null;
            int idx = BinarySearchStyle(name);
            if (idx >= 0) {
                return styleSearchMap[idx].container;
            }

            return null;
        }

        public bool TryResolveAnimationByName(string name, out AnimationData animationData) {
            if (name == null || name.Length == 0) {
                animationData = default;
                return false;
            }

            int idx = BinarySearchAnimation(name);
            if (idx >= 0) {
                animationData = animationSearchMap[idx].animation;
                return true;
            }

            animationData = default;
            return false;
        }

        private int BinarySearchAnimation(string name) {
            int num1 = 0;
            int num2 = animationSearchMap.Length - 1;
            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);

                int num3 = string.CompareOrdinal(animationSearchMap[index1].name, name);

                if (num3 == 0) {
                    return index1;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return ~num1;
        }

        private int BinarySearchAliasedStyle(string name) {
            int num1 = 0;
            int num2 = styleSearchMap.Length - 1;

            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);

                int num3 = string.CompareOrdinal(styleSearchMap[index1].aliasedName, name);

                if (num3 == 0) {
                    return index1;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return ~num1;
        }

        private int BinarySearchStyle(string name) {
            int num1 = 0;
            int num2 = styleSearchMap.Length - 1;

            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);

                int num3 = string.CompareOrdinal(styleSearchMap[index1].name, name);

                if (num3 == 0) {
                    return index1;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return ~num1;
        }

        private int BinarySearchStyle(char[] name) {
            int num1 = 0;
            int num2 = styleSearchMap.Length - 1;
            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);
                int num3 = StringUtil.CharCompareOrdinal(styleSearchMap[index1].name, name);
                if (num3 == 0) {
                    return index1;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return ~num1;
        }

        public UIStyleGroupContainer ResolveStyleByName(char[] alias, string name) {
            if (string.IsNullOrEmpty(name)) return null;

            if (name.Contains(".")) {
                throw new NotImplementedException("Cannot resolve style name with aliases yet");
            }

            for (int i = 0; i < styleReferences.Length; i++) {
                if (styleReferences[i].alias == name) {
                    return styleReferences[i].styleSheet.GetStyleByName(name);
                }
            }

            return null;
        }

        public UIStyleGroupContainer GetStyleById(int styleId) {
            return styleSearchMap[styleId].container;
        }

        public int ResolveStyleByIdSlow(int id) {
            BuildSearchMap();
            for (int i = 0; i < styleSearchMap.Length; i++) {
                if (styleSearchMap[i].container.id == id) return i;
            }

            return -1;
        }

    }

}