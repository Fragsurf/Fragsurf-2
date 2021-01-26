using System;

namespace UIForia.UIInput {

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OnDragCreateAttribute : Attribute {

        public readonly EventPhase phase;
        public readonly KeyboardModifiers modifiers;

        public OnDragCreateAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble) {
            this.modifiers = modifiers;
            this.phase = phase;
        }

        public OnDragCreateAttribute(EventPhase phase) {
            this.modifiers = KeyboardModifiers.None;
            this.phase = phase;
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class DragEventHandlerAttribute : Attribute {

        public readonly KeyboardModifiers modifiers;
        public readonly InputEventType eventType;
        public readonly EventPhase phase;
        public readonly Type requiredType;

        protected DragEventHandlerAttribute(Type requiredType, KeyboardModifiers modifiers, InputEventType eventType, EventPhase phase) {
            this.modifiers = modifiers;
            this.eventType = eventType;
            this.phase = phase;
            this.requiredType = requiredType;
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnDragEnterAttribute : DragEventHandlerAttribute {

        public OnDragEnterAttribute(Type requiredType, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(requiredType, modifiers, InputEventType.DragEnter, phase) { }

        public OnDragEnterAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(null, modifiers, InputEventType.DragEnter, phase) { }

        public OnDragEnterAttribute(EventPhase phase)
            : base(null, KeyboardModifiers.None, InputEventType.DragEnter, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnDragExitAttribute : DragEventHandlerAttribute {

        public OnDragExitAttribute(Type requiredType, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(requiredType, modifiers, InputEventType.DragExit, phase) { }

        public OnDragExitAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(null, modifiers, InputEventType.DragExit, phase) { }


        public OnDragExitAttribute(EventPhase phase)
            : base(null, KeyboardModifiers.None, InputEventType.DragExit, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnDragMoveAttribute : DragEventHandlerAttribute {

        public OnDragMoveAttribute(Type requiredType, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(requiredType, modifiers, InputEventType.DragMove, phase) { }

        public OnDragMoveAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(null, modifiers, InputEventType.DragMove, phase) { }


        public OnDragMoveAttribute(EventPhase phase)
            : base(null, KeyboardModifiers.None, InputEventType.DragMove, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnDragHoverAttribute : DragEventHandlerAttribute {

        public OnDragHoverAttribute(Type requiredType, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(requiredType, modifiers, InputEventType.DragHover, phase) { }

        public OnDragHoverAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(null, modifiers, InputEventType.DragHover, phase) { }


        public OnDragHoverAttribute(EventPhase phase)
            : base(null, KeyboardModifiers.None, InputEventType.DragHover, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnDragCancelAttribute : DragEventHandlerAttribute {

        public OnDragCancelAttribute(Type requiredType, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(requiredType, modifiers, InputEventType.DragCancel, phase) { }

        public OnDragCancelAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(null, modifiers, InputEventType.DragCancel, phase) { }


        public OnDragCancelAttribute(EventPhase phase)
            : base(null, KeyboardModifiers.None, InputEventType.DragCancel, phase) { }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OnDragDropAttribute : DragEventHandlerAttribute {

        public OnDragDropAttribute(Type requiredType, KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(requiredType, modifiers, InputEventType.DragDrop, phase) { }

        public OnDragDropAttribute(KeyboardModifiers modifiers = KeyboardModifiers.None, EventPhase phase = EventPhase.Bubble)
            : base(null, modifiers, InputEventType.DragDrop, phase) { }


        public OnDragDropAttribute(EventPhase phase)
            : base(null, KeyboardModifiers.None, InputEventType.DragDrop, phase) { }

    }

}