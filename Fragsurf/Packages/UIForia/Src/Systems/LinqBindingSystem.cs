using System;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class LinqBindingSystem : ISystem {

        private readonly LightList<UIElement> rootNodes;
        private UIElement currentElement;
        private int iteratorIndex;
        private uint iterationId;
        private uint currentFrameId;

        public LinqBindingSystem() {
            this.rootNodes = new LightList<UIElement>();
        }

        public void OnReset() { }
        
        public void OnUpdate() { }
        
        public void OnDestroy() { }

        public void OnViewAdded(UIView view) {
//            // todo -- need keep this sorted or just iterate application views
            rootNodes.Add(view.dummyRoot);
        }

        public void OnViewRemoved(UIView view) {
            rootNodes.Remove(view.dummyRoot);
            // todo -- if currently iterating this view, need to bail out
        }

        public void OnElementCreated(UIElement element) {
            // if creating something higher in the tree than current, need to reset
            if (element.parent == currentElement) {
                iteratorIndex = 0;
            }
        }

        public void OnElementEnabled(UIElement element) {
            // if enabling something higher in the tree than current, need to reset
            if (element.parent == currentElement) {
                iteratorIndex = 0;
            }
        }

        public void OnElementDisabled(UIElement element) {
            // if disabling current or ancestor of current need to bail out
            if (element.parent == currentElement) {
                iteratorIndex = 0;
            }
        }

        public void OnElementDestroyed(UIElement element) {
            // if destroying current or ancestor of current need to bail out
            iteratorIndex = 0;
        }

        public void OnAttributeSet(UIElement element, string attributeName, string currentValue, string previousValue) { }

        public void NewUpdateFn(UIElement element) {
            // Debug.Log($"{new string(' ', element.hierarchyDepth * 4)}Before {element.GetDisplayName()}");

            element.bindingNode?.updateBindings?.Invoke(element.bindingNode.root, element);

            if (element.isEnabled) {
                
                for (int i = 0; i < element.children.size; i++) {
                    NewUpdateFn(element.children[i]);
                }

                // Debug.Log($"{new string(' ', element.hierarchyDepth * 4)}After {element.GetDisplayName()}");

                element.bindingNode?.lateBindings?.Invoke(element.bindingNode.root, element);
            }
        }
        
        public void BeginFrame() {
            iteratorIndex = 0;
            currentFrameId++;
        }
        
    }

}