//using System;
//using System.Reflection;
//using UIForia.Expressions;
//
//namespace UIForia.UIInput {
//
//    public abstract class DragEventHandler : IComparable<DragEventHandler> {
//
//        public readonly InputEventType eventType;
//        public readonly KeyboardModifiers requiredModifiers;
//        public readonly EventPhase eventPhase;
//        public readonly Type requiredType;
//
//#if DEBUG
//        public MethodInfo methodInfo;
//#endif
//        protected DragEventHandler(InputEventType eventType, Type requiredType, KeyboardModifiers requiredModifiers, EventPhase eventPhase) {
//            this.eventType = eventType;
//            this.requiredModifiers = requiredModifiers;
//            this.eventPhase = eventPhase;
//            this.requiredType = requiredType;
//        }
//
//        public abstract void Invoke(object target, ExpressionContext context, DragEvent evt);
//
//        public int CompareTo(DragEventHandler other) {
//            int modifierResult = CompareModifiers(other);
//            if (modifierResult != 0) return modifierResult;
//
//            return 1;
//        }
//
//        protected bool ShouldRun(DragEvent evt) {
//            if (evt.CurrentEventType != eventType) return false;
//            if (requiredType != null && requiredType != evt.type) return false;
//
//            // if all required modifiers are present these should be equal
//            return (requiredModifiers & evt.Modifiers) == requiredModifiers;
//        }
//
//        private int CompareModifiers(DragEventHandler other) {
//            if (requiredModifiers == KeyboardModifiers.None && other.requiredModifiers == KeyboardModifiers.None) return 0;
//            if (requiredModifiers != KeyboardModifiers.None && other.requiredModifiers != KeyboardModifiers.None) return 0;
//            return (requiredModifiers != KeyboardModifiers.None) ? 1 : -1;
//        }
//
//    }
//
//    public class DragEventHandler_Expression : DragEventHandler {
//
//        private readonly Expression<Terminal> expression;
//
//        public DragEventHandler_Expression(InputEventType evtType, Expression<Terminal> expression, KeyboardModifiers modifiers, EventPhase phase)
//            : base(evtType, null, modifiers, phase) {
//            this.expression = expression;
//        }
//
//        public override void Invoke(object target, ExpressionContext context, DragEvent evt) {
//            if (ShouldRun(evt)) {
//                expression.Evaluate(context);
//            }
//        }
//
//    }
//
//    public class DragEventHandler_IgnoreEvent<T> : DragEventHandler {
//
//        private readonly Action<T> handler;
//
//        public DragEventHandler_IgnoreEvent(InputEventType eventType, Type requiredType, KeyboardModifiers requiredModifiers, EventPhase phase, Action<T> handler)
//            : base(eventType, requiredType, requiredModifiers, phase) {
//            this.handler = handler;
//        }
//
//        public override void Invoke(object target, ExpressionContext context, DragEvent evt) {
//            if (ShouldRun(evt)) {
//                handler((T) target);
//            }
//        }
//
//    }
//
//    public class DragEventHandler_WithEvent<T> : DragEventHandler {
//
//        private readonly Action<T, DragEvent> handler;
//
//        public DragEventHandler_WithEvent(InputEventType eventType, Type requiredType, KeyboardModifiers requiredModifiers, EventPhase phase, Action<T, DragEvent> handler)
//            : base(eventType, requiredType, requiredModifiers, phase) {
//            this.handler = handler;
//        }
//
//        public override void Invoke(object target, ExpressionContext context, DragEvent evt) {
//            if (ShouldRun(evt)) {
//                handler((T) target, evt);
//            }
//        }
//
//    }
//
//}