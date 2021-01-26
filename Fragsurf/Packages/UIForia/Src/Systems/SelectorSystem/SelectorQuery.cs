using System;
using UIForia.Elements;
using UIForia.Util;

namespace Systems.SelectorSystem {

    public class SelectorQuery {

        public SelectorIndex[] indices;
        public SelectorModifier[] modifiers;
        public NavigationOperator navigationOperator;
        public SelectorQuery next;

        public bool Run(UIElement origin, UIElement element) {
            LightList<UIElement> list = LightList<UIElement>.Get();
            int templateId = origin.templateMetaData.id;

            switch (navigationOperator) {
                case NavigationOperator.Parent: {
                    UIElement ptr = element.parent;

                    while (ptr != origin) {
                        if (ptr.templateMetaData.id == templateId) {
                            list.Add(ptr);
                            break;
                        }

                        ptr = ptr.parent;
                    }

                    break;
                }
                case NavigationOperator.Ancestor: {
                    UIElement ptr = element.parent;

                    while (ptr != origin) {
                        if (ptr.templateMetaData.id == templateId) {
                            list.Add(ptr);
                        }

                        ptr = ptr.parent;
                    }

                    break;
                }
                case NavigationOperator.Sibling:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ApplyModifiers(origin, list);
            
            Filter(origin, list);

            if (list.size == 0) {
                list.Release();
                return false;
            }

            if (next == null) {
                list.Release();
                return true;
            }

            for (int i = 0; i < list.size; i++) {
                if (!next.Run(origin, list.array[i])) {
                    list.Release();
                    return false;
                }
            }

            list.Release();
            return true;
        }

        private void ApplyModifiers(UIElement origin, LightList<UIElement> list) {
            if (modifiers == null) return;
            for (int i = 0; i < modifiers.Length; i++) {
                SelectorModifier modifier = modifiers[i];
                for (int j = 0; j < list.size; j++) {
                    // ??
                }
            }
        }
        
        public void Gather(UIElement element, LightList<UIElement> resultSet) {
            int templateId = element.templateMetaData.id; // templateId might be outer or inner, style should know this
            int idx = 0;
            int min = indices[0].size;
            for (int i = 1; i < indices.Length; i++) {
                int size = indices[i].size;
                if (size < min) {
                    min = size;
                    idx = i;
                }
            }

            indices[idx].Gather(element, element.templateMetaData.id, resultSet);

            for (int i = 0; i < indices.Length; i++) {
                if (i == idx) continue;
                indices[i].Filter(element, templateId, resultSet);
            }
        }
        
        public void Filter(UIElement origin, LightList<UIElement> elements) {
            // group 
            // filter
            // forward
        }

    }

}