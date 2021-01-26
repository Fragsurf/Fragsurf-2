using System;
using System.Collections.Generic;
using System.Diagnostics;
using Systems.SelectorSystem;
using JetBrains.Annotations;
using UIForia.Compilers;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Elements {

    // public struct UIElementRef {
    //
    //     private readonly int id;
    //     private UIElement element;
    //
    //     public UIElementRef(UIElement element) {
    //         this.id = element?.id ?? -1;
    //         this.element = element;
    //     }
    //
    //     public UIElement Element {
    //         get {
    //             if (id != element.id) {
    //                 element = null;
    //                 return null;
    //             }
    //
    //             return element;
    //         }
    //     }
    //
    //     public static implicit operator UIElement(UIElementRef elementRef) {
    //         return elementRef.Element;
    //     }
    //
    // }
    //
    // public struct UIElementRef<T> where T : UIElement {
    //
    //     private readonly int id;
    //     private T element;
    //
    //     public UIElementRef(T element) {
    //         this.id = element?.id ?? -1;
    //         this.element = element;
    //     }
    //
    //     public T Element {
    //         get {
    //             if (id != element.id) {
    //                 element = null;
    //                 return null;
    //             }
    //
    //             return element;
    //         }
    //     }
    //
    //     public static implicit operator UIElementRef(UIElementRef<T> elementRef) {
    //         return new UIElementRef(elementRef.Element);
    //     }
    //
    //     public static implicit operator UIElement(UIElementRef<T> elementRef) {
    //         return elementRef.Element;
    //     }
    //
    //     public static implicit operator T(UIElementRef<T> elementRef) {
    //         return elementRef.Element;
    //     }
    //
    // }

    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public abstract class UIElement : IHierarchical {

        public int id; // todo -- internal with accessor

        public InputHandlerGroup inputHandlers; // todo -- internal with accessor

        public LightList<UIElement> children; // todo -- replace w/ linked list & child count

        internal UIElementFlags flags;
        public UIElement parent;
        internal TagNameIndex tagNameIndex;

        // todo -- maybe move a lot of this data to an internal representation of UIElement
        public LayoutResult layoutResult;
        internal AwesomeLayoutBox layoutBox;
        internal RenderBox renderBox;
        public UIStyleSet style; // todo -- make internal with accessor
        public LinqBindingNode bindingNode; // todo -- make internal with accessor

        internal int enableStateChangedFrameId;
        public StructList<ElementAttribute> attributes;
        public TemplateMetaData templateMetaData; // todo - internal / private / whatever

        public UIView View { get; internal set; }
        public Application application { get; internal set; }
        public int hierarchyDepth { get; internal set; }
        private int _siblingIndex;

        public int siblingIndex {
            get => _siblingIndex;
            internal set {
                if (_siblingIndex == value) return;
                _siblingIndex = value;
                flags |= UIElementFlags.IndexChanged;
            }
        }

//        
        // not actually used since we get elements from the pool as uninitialized
        protected internal UIElement() { }

        public IInputProvider Input => View.application.InputSystem; // todo -- remove

        public int ChildCount => children?.Count ?? 0;

        public bool __internal_isEnabledAndNeedsUpdate => (flags & UIElementFlags.EnabledFlagSetWithUpdate ) == (UIElementFlags.EnabledFlagSetWithUpdate);
        
        public bool isSelfEnabled => (flags & UIElementFlags.Enabled) != 0;
        
        public bool hasBeenEnabled => (flags & UIElementFlags.HasBeenEnabled) != 0;

        public bool isSelfDisabled => (flags & UIElementFlags.Enabled) == 0;

        public bool isEnabled => (flags & UIElementFlags.EnabledFlagSet) == (UIElementFlags.EnabledFlagSet);
        //!isDestroyed && (flags & UIElementFlags.SelfAndAncestorEnabled) == UIElementFlags.SelfAndAncestorEnabled;

        public bool isDisabled => (flags & UIElementFlags.EnabledFlagSet) != (UIElementFlags.EnabledFlagSet);

        //isDestroyed || (flags & UIElementFlags.Enabled) == 0 || (flags & UIElementFlags.AncestorEnabled) == 0;

        public bool isDestroyed => (flags & UIElementFlags.Alive) == 0;

        public virtual void OnCreate() { }

        public virtual void OnUpdate() { }
        
        public virtual void OnBeforePropertyBindings() { }
        
        public virtual void OnAfterPropertyBindings() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnDestroy() { }

        public void Destroy() {
            View.application.DoDestroyElement(this);
        }

        public UIElement InsertChild(uint idx, UIElement element) {
            throw new NotImplementedException();
            // if (element == null || element == this || element.isDestroyed) {
            //     return null;
            // }
            //
            // if (View == null) {
            //     element.parent = this;
            //     element.View = null;
            //     element.siblingIndex = children.Count;
            //     element.depth = depth + 1;
            //     children.Insert((int) idx, element);
            // }
            // else {
            //     Application.InsertChild(this, element, (uint) children.Count);
            // }
            //
            // return element;
        }

        internal UIElement AddChild(UIElement element) {
            // todo -- if <Children/> is defined in the template, attach child to that element instead
            if (element == null || element == this || element.isDestroyed) {
                return null;
            }

            if (View == null) {
                element.parent = this;
                element.View = null;
                element.siblingIndex = children.Count;
                element.hierarchyDepth = hierarchyDepth + 1;
                children.Add(element);
            }
            else {
                application.InsertChild(this, element, (uint) children.Count);
            }

            return element;
        }

        public bool internal__dontcallmeplease_SetEnabledIfBinding(bool enabled) {
            if (enabled && isSelfDisabled) {
                application.DoEnableElement(this, false);
            }
            else if (!enabled && isSelfEnabled) {
                application.DoDisableElement(this);
            }

            return false;
        }

        public void SetEnabled(bool enabled) {
            if (enabled && isSelfDisabled) {
                application.DoEnableElement(this, true);
            }
            else if (!enabled && isSelfEnabled) {
                application.DoDisableElement(this);
            }
        }

        public UIElement GetChild(int index) {
            if (children == null || (uint) index >= (uint) children.Count) {
                return null;
            }

            return children[index];
        }

        public UIElement FindById(string elementId) {
            return FindById<UIElement>(elementId);
        }

        [PublicAPI]
        public T FindById<T>(string elementId) where T : UIElement {
            LightStack<UIElement> elementStack = LightStack<UIElement>.Get();
            elementStack.Push(this);
            while (elementStack.size > 0) {
                UIElement element = elementStack.array[--elementStack.size];

                if (element.children == null) {
                    continue;
                }

                elementStack.EnsureAdditionalCapacity(element.children.size);

                for (int i = element.children.size - 1; i >= 0; i--) {
                    UIElement child = element.children.array[i];

                    // todo -- need to figure out if we should descend into children. probably want to scrap this whole method and do something better with selectors
                    // if (child.templateMetaData == element.templateMetaData) {
                        if (child is T castChild && child.GetAttribute("id") == elementId) {
                            LightStack<UIElement>.Release(ref elementStack);
                            return castChild;
                        }
                    // }

                    elementStack.array[elementStack.size++] = child;
                }
            }

            LightStack<UIElement>.Release(ref elementStack);

            return null;
        }

        [PublicAPI]
        public T FindFirstByType<T>() where T : UIElement {
            if (children == null) {
                return null;
            }

            for (int i = 0; i < children.Count; i++) {
                if (children[i] is T) {
                    return (T) children[i];
                }

                if (children[i] is UIChildrenElement) {
                    continue;
                }

                UIElement childResult = children[i].FindFirstByType<T>();
                if (childResult != null) {
                    return (T) childResult;
                }
            }

            return null;
        }

        public List<T> FindByType<T>(List<T> retn = null) where T : UIElement {
            retn = retn ?? new List<T>();
            if (children == null) {
                return retn;
            }

            for (int i = 0; i < children.Count; i++) {
                if (children[i] is T) {
                    retn.Add((T) children[i]);
                }

                if (children[i] is UIChildrenElement) {
                    continue;
                }

                children[i].FindByType<T>(retn);
            }

            return retn;
        }

        public override string ToString() {
            if (style != null) {
                string idText = GetAttribute("id");
                string styleNames = style.GetStyleNames();
                return $"<{GetDisplayName()}[{id}]{(idText != null ? ":" + idText : "")}> {styleNames}";
            }
            else {
                return "<" + GetDisplayName() + " " + id + ">";
            }
        }

        public virtual string GetDisplayName() {
            return GetType().Name;
        }

        public List<ElementAttribute> GetAttributes(List<ElementAttribute> retn = null) {
            retn = retn ?? new List<ElementAttribute>();
            if (attributes == null || attributes.size == 0) {
                return retn;
            }

            for (int i = 0; i < attributes.size; i++) {
                retn.Add(attributes.array[i]);
            }

            return retn;
        }

        public void SetAttribute(string name, string value) {
            if (attributes == null) {
                attributes = StructList<ElementAttribute>.Get();
            }

            ElementAttribute[] attrs = attributes.array;
            int attrCount = attributes.size;

            for (int i = 0; i < attrCount; i++) {
                if (attrs[i].name == name) {
                    if (attrs[i].value == value) {
                        return;
                    }
                    else {
                        string oldValue = attrs[i].value;
                        attrs[i] = new ElementAttribute(name, value);
                        OnSetAttribute(name, value, oldValue);
                        application.OnAttributeSet(this, name, value, oldValue);
                        return;
                    }
                }
            }

            attributes.Add(new ElementAttribute(name, value));
            OnSetAttribute(name, value, null);
            application.OnAttributeSet(this, name, value, null);
        }

        protected virtual void OnSetAttribute(string attrName, string newValue, string oldValue) { }

        public bool TryGetAttribute(string key, out string value) {
            if (attributes == null) {
                value = null;
                return false;
            }

            ElementAttribute[] attrs = attributes.array;
            int attrCount = attributes.size;

            for (int i = 0; i < attrCount; i++) {
                if (attrs[i].name == key) {
                    value = attrs[i].value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public string GetAttribute(string attr) {
            if (attributes == null) {
                return null;
            }

            ElementAttribute[] attrs = attributes.array;
            int attrCount = attributes.size;

            for (int i = 0; i < attrCount; i++) {
                if (attrs[i].name == attr) {
                    return attrs[i].value;
                }
            }

            return null;
        }

        public bool HasAttribute(string name) {
            return GetAttribute(name) != null;
        }

        public int UniqueId => id;
        public IHierarchical Element => this;
        public IHierarchical Parent => parent;

        public List<UIElement> GetChildren(List<UIElement> retn = null) {
            retn = ListPool<UIElement>.Get();

            if (children == null) {
                return retn;
            }

            UIElement[] childArray = children.Array;
            for (int i = 0; i < children.Count; i++) {
                retn.Add(childArray[i]);
            }

            return retn;
        }

        internal void InternalDestroy() {
            LightList<UIElement>.Release(ref children);
            parent = null;
        }

        public MaterialInterface Material {
            get => new MaterialInterface(this, application);
        }

        public bool IsAncestorOf(UIElement potentialParent) {
            if (potentialParent == this || potentialParent == null) {
                return false;
            }

            UIElement ptr = this;
            while (ptr != null) {
                if (ptr.parent == potentialParent) {
                    return true;
                }

                ptr = ptr.parent;
            }

            return false;
        }

        public UIElement this[int i] {
            get { return GetChild(i); }
        }

        public UIElement this[string id] {
            get { return FindById(id); }
        }

        public ElementAnimator Animator => new ElementAnimator(application.animationSystem, this);

        public IInputSystem InputSystem => application.inputSystem;

        // element.animator.Stop();

        public void ScrollIntoView() {
            UIElement ptr = parent;

            float crawlPositionX = layoutResult.localPosition.x;
            float crawlPositionY = layoutResult.localPosition.y;
            
            while (ptr != null) {

                if (ptr is ScrollView scrollView) {
                    scrollView.ScrollElementIntoView(this, crawlPositionX, crawlPositionY);
                    return;
                }

                crawlPositionX += ptr.layoutResult.localPosition.x;
                crawlPositionY += ptr.layoutResult.localPosition.y;
                
                ptr = ptr.parent;
            }
        }

        public T FindParent<T>() where T : UIElement{
            UIElement ptr = parent;
            while (ptr != null) {
                if (ptr is T retn) {
                    return retn;
                }

                ptr = ptr.parent;
            }

            return null;
        }
        
        // traverses hierarchy upwards
        public T FindNextWithInterface<T>(){
            UIElement ptr = this;
            while (ptr != null) {
                if (ptr is T retn) {
                    return retn;
                }

                ptr = ptr.parent;
            }

            return default;
        }

        public void RunBindings() {
            flags &= ~UIElementFlags.NeedsUpdate;
            bindingNode.updateBindings?.Invoke(bindingNode.root, this);
            flags |= UIElementFlags.NeedsUpdate;
        }

    }

}
