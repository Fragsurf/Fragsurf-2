using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Compilers;
using UIForia.Elements;

namespace UIForia.Systems {

    public abstract class ContextVariable {

        public int id;
        public string name;
        public ContextVariable next;
        public ContextVariable reference;

        public bool IsReference => reference != null;

        public abstract ContextVariable CreateReference();

    }

    [DebuggerDisplay("{name} {id}")]
    public class ContextVariable<T> : ContextVariable {

        public T value;

        private ContextVariable(ContextVariable<T> original) {
            this.id = original.id;
            this.name = original.name;
            this.reference = original;
            this.value = original.value;
        }

        public ContextVariable(int id, string name, T value) {
            this.id = id;
            this.name = name;
            this.value = value;
        }

        public override ContextVariable CreateReference() {
            return new ContextVariable<T>((ContextVariable<T>) reference ?? this);
        }

    }

    public class LinqBindingNode {

        public UIElement root;
        public UIElement element;
        public UIElement innerContext;

        internal uint lastBeforeUpdateFrame;
        internal uint lastAfterUpdateFrame;

        internal Action<UIElement, UIElement> createdBinding;
        internal Action<UIElement, UIElement> enabledBinding;
        internal Action<UIElement, UIElement> updateBindings;
        internal Action<UIElement, UIElement> lateBindings;

        internal ContextVariable localVariable;
        internal LinqBindingNode parent;
        public UIElement[] referencedContexts;

        public void InitializeContextArray(string slotName, TemplateScope templateScope, int size) {
            referencedContexts = new UIElement[size + 1];

            if (templateScope.slotInputs != null) {
                int idx = 1;
                for (int i = templateScope.slotInputs.size - 1; i >= 0; i--) {
                    if (templateScope.slotInputs.array[i].slotName == slotName) {
                        referencedContexts[idx++] = templateScope.slotInputs.array[i].context;
                    }
                }
            }

            referencedContexts[0] = templateScope.innerSlotContext;
        }

        public void CreateLocalContextVariable(ContextVariable variable) {
            if (localVariable == null) {
                localVariable = variable;
                return;
            }

            ContextVariable ptr = localVariable;
            while (true) {
                if (ptr.next == null) {
                    ptr.next = variable;
                    return;
                }

                ptr = ptr.next;
            }
        }

        public ContextVariable GetContextVariable(int id) {
            ContextVariable ptr = localVariable;
            while (ptr != null) {
                if (ptr.id == id) {
                    return ptr.reference ?? ptr;
                }

                ptr = ptr.next;
            }

            // if didnt find a local variable, start a search upwards

            // if found, reference locally. should only hit this once

            if (parent == null) {
                UIElement elemPtr = element.parent;
                while (elemPtr != null) {
                    if (elemPtr.bindingNode != null) {
                        parent = elemPtr.bindingNode;
                        break;
                    }

                    elemPtr = elemPtr.parent;
                }
            }

            ContextVariable value = parent?.GetContextVariable(id);

            // it is technically an error if we can't find the context variable, something went wrong with compilation
            Debug.Assert(value != null, nameof(value) + " != null");

            value = value.CreateReference();

            CreateLocalContextVariable(value);

            return value;
        }

        public static LinqBindingNode GetSlotNode(Application application, UIElement rootElement, UIElement element, UIElement innerContext, int createdId, int enabledId, int updatedId, int lateId, string slotName, TemplateScope templateScope, int slotContextSize) {
            LinqBindingNode node = new LinqBindingNode(); // todo -- pool
            node.root = rootElement;
            node.element = element;
            node.innerContext = innerContext;
            element.bindingNode = node;

            // todo -- profile this against skip tree
            UIElement ptr = element.parent;
            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    node.parent = ptr.bindingNode;
                    break;
                }

                ptr = ptr.parent;
            }

            node.InitializeContextArray(slotName, templateScope, slotContextSize);

            node.SetBindings(application, rootElement, createdId, enabledId, updatedId, lateId);

            return node;
        }

        public static LinqBindingNode GetSlotModifyNode(Application application, UIElement rootElement, UIElement element, UIElement innerContext, int createdId, int enabledId, int updatedId, int lateId) {
            LinqBindingNode node = new LinqBindingNode(); // todo -- pool
            node.root = rootElement;
            node.element = element;
            node.innerContext = innerContext;
            element.bindingNode = node;

            // todo -- profile this against skip tree
            UIElement ptr = element.parent;
            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    node.parent = ptr.bindingNode;
                    break;
                }

                ptr = ptr.parent;
            }

            node.referencedContexts = new UIElement[1];
            node.referencedContexts[0] = innerContext;

            node.SetBindings(application, rootElement, createdId, enabledId, updatedId, lateId);

            return node;
        }

        private void SetBindings(Application application, UIElement rootElement, int createdId, int enabledId, int updatedId, int lateId) {
            if (createdId != -1) {
                try {
                    createdBinding = application.templateData.bindings[createdId];
                    createdBinding?.Invoke(rootElement, element);
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogWarning(e);
                }
            }

            if (enabledId != -1) {
                enabledBinding = application.templateData.bindings[enabledId];
            }

            if (updatedId != -1) {
                updateBindings = application.templateData.bindings[updatedId];
            }

            if (lateId != -1) {
                lateBindings = application.templateData.bindings[lateId];
            }
        }

        [UsedImplicitly] // called from template functions, 
        public static LinqBindingNode Get(Application application, UIElement rootElement, UIElement element, UIElement innerContext, int createdId, int enabledId, int updatedId, int lateId) {
            LinqBindingNode node = new LinqBindingNode(); // todo -- pool
            node.root = rootElement;
            node.element = element;
            node.innerContext = innerContext;
            element.bindingNode = node;

            // todo -- profile this against skip tree
            UIElement ptr = element.parent;
            while (ptr != null) {
                if (ptr.bindingNode != null) {
                    node.parent = ptr.bindingNode;
                    break;
                }

                ptr = ptr.parent;
            }

            node.SetBindings(application, rootElement, createdId, enabledId, updatedId, lateId);

            return node;
        }

        public ContextVariable<T> GetRepeatItem<T>(int id) {
            return (ContextVariable<T>) GetContextVariable(id);
        }

    }

}