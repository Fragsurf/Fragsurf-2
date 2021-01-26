using System;
using System.Collections.Generic;
using Systems.SelectorSystem;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public interface IStyleChangeHandler {

        void OnStylePropertyChanged(in StyleProperty property);

    }

    public class StyleSystem : IStyleSystem {

        public event Action<UIElement, StructList<StyleProperty>> onStylePropertyChanged;

        private static readonly Stack<UIElement> s_ElementStack = new Stack<UIElement>();

        private readonly IntMap<ChangeSet> m_ChangeSets;

        public StyleSystem() {
            this.m_ChangeSets = new IntMap<ChangeSet>();
        }

        public void OnReset() { }

        public void OnElementCreated(UIElement element) { }

        public void OnUpdate() {
            
            if (onStylePropertyChanged == null) {
                return;
            }

            m_ChangeSets.ForEach(this, (id, changeSet, self) => {
                if (!changeSet.element.isEnabled) {
                    StructList<StyleProperty>.Release(ref changeSet.changes);
                    changeSet.element = null;
                    return;
                }

                // if (changeSet.element is IStylePropertiesWillChangeHandler willChangeHandler) {
                //     willChangeHandler.OnStylePropertiesWillChange();
                // }

                self.onStylePropertyChanged.Invoke(changeSet.element, changeSet.changes);

                if (changeSet.element is IStyleChangeHandler changeHandler) {
                    StyleProperty[] properties = changeSet.changes.Array;
                    int count = changeSet.changes.Count;
                    for (int i = 0; i < count; i++) {
                        changeHandler.OnStylePropertyChanged(properties[i]);
                    }
                }

                // if (changeSet.element is IStylePropertiesDidChangeHandler didChangeHandler) {
                //     didChangeHandler.OnStylePropertiesDidChange();
                // }

                StructList<StyleProperty>.Release(ref changeSet.changes);
                changeSet.element = null;
            });

            m_ChangeSets.Clear();
        }

        public void OnDestroy() { }

        public void OnViewAdded(UIView view) { }

        public void OnViewRemoved(UIView view) { }

        public void OnElementEnabled(UIElement element) { }

        public void OnElementDisabled(UIElement element) {
            m_ChangeSets.Remove(element.id);
        }

        public void OnElementDestroyed(UIElement element) {
            element.style = null;
            m_ChangeSets.Remove(element.id);
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string attributeValue) {
            element.style.UpdateApplicableAttributeRules();
        }

        private void AddToChangeSet(UIElement element, StyleProperty property) {
            if (!m_ChangeSets.TryGetValue(element.id, out ChangeSet changeSet)) {
                changeSet = new ChangeSet(element, StructList<StyleProperty>.Get());
                m_ChangeSets[element.id] = changeSet;
            }

            changeSet.changes.Add(property);
        }

        public void SetStyleProperty(UIElement element, StyleProperty property) {

            if (element.isDisabled) return;

            AddToChangeSet(element, property);

            if (!StyleUtil.IsInherited(property.propertyId) || element.children == null || element.children.Count == 0) {
                return;
            }

            if (!property.hasValue) {

                UIElement ptr = element.parent;

                StyleProperty parentProperty = new StyleProperty(property.propertyId);

                while (ptr != null) {
                    parentProperty = ptr.style.GetPropertyValue(property.propertyId);
                    if (parentProperty.hasValue) {
                        break;
                    }

                    ptr = ptr.parent;
                }

                if (!parentProperty.hasValue) {
                    parentProperty = DefaultStyleValues_Generated.GetPropertyValue(property.propertyId);
                }

                property = parentProperty;
            }

            for (int i = 0; i < element.children.Count; i++) {
                s_ElementStack.Push(element.children[i]);
            }

            while (s_ElementStack.Count > 0) {
                UIElement descendant = s_ElementStack.Pop();

                if (!descendant.style.SetInheritedStyle(property)) {
                    continue;
                }

                // todo -- we might want to cache font size lookups for em values, this would be the place 
                // if (property.propertyId == StylePropertyId.TextFontSize) {
                // do caching    
                // }

                AddToChangeSet(descendant, property);

                if (descendant.children == null) {
                    continue;
                }

                for (int i = 0; i < descendant.children.Count; i++) {
                    s_ElementStack.Push(descendant.children[i]);
                }
            }
        }

        public void AddSelectors(Selector[] selectors) {
            if (selectors == null) return;
        }

        public void RemoveSelectors(Selector[] selectors) {
            if (selectors == null) return;
        }

        private struct ChangeSet {

            public UIElement element;
            public StructList<StyleProperty> changes;

            public ChangeSet(UIElement element, StructList<StyleProperty> changes) {
                this.element = element;
                this.changes = changes;
            }

        }

    }

}