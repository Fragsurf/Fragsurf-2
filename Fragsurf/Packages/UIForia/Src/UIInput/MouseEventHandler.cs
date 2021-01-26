//using System;
//using System.Reflection;
//using UIForia.Expressions;
//
//namespace UIForia.UIInput {
//
//    public abstract class MouseEventHandler : IComparable<MouseEventHandler> {
//
//        public readonly InputEventType eventType;
//        public readonly KeyboardModifiers requiredModifiers;
//        public readonly EventPhase eventPhase;
//        public readonly ExpressionContext ctx;
//        
//#if DEBUG
//        public MethodInfo methodInfo;
//#endif
//        protected MouseEventHandler(InputEventType eventType, KeyboardModifiers requiredModifiers, EventPhase eventPhase) {
//            this.eventType = eventType;
//            this.requiredModifiers = requiredModifiers;
//            this.eventPhase = eventPhase;
//        }
//
//        public abstract void Invoke(object target, ExpressionContext context, MouseInputEvent evt);
//
//        public int CompareTo(MouseEventHandler other) {
//            int modifierResult = CompareModifiers(other);
//            if (modifierResult != 0) return modifierResult;
//
//            return 1;
//        }
//
//        protected bool ShouldRun(MouseInputEvent evt) {
//            if (evt.type != eventType) return false;
//
//            // if all required modifiers are present these should be equal
//            return (requiredModifiers & evt.modifiers) == requiredModifiers;
//        }
//
//        private int CompareModifiers(MouseEventHandler other) {
//            if (requiredModifiers == KeyboardModifiers.None && other.requiredModifiers == KeyboardModifiers.None) return 0;
//            if (requiredModifiers != KeyboardModifiers.None && other.requiredModifiers != KeyboardModifiers.None) return 0;
//            return (requiredModifiers != KeyboardModifiers.None) ? 1 : -1;
//        }
//
//    }
//
//    public class MouseEventHandler_Expression : MouseEventHandler {
//
//        private readonly Expression<Terminal> expression;
//
//        public MouseEventHandler_Expression(InputEventType evtType, Expression<Terminal> expression, KeyboardModifiers modifiers, EventPhase phase)
//            : base(evtType, modifiers, phase) {
//            this.expression = expression;
//        }
//
//        public override void Invoke(object target, ExpressionContext context, MouseInputEvent evt) {
//            if (ShouldRun(evt)) {
//                expression.Evaluate(context);
//            }
//        }
//
//    }
//
//    public class MouseEventHandler_IgnoreEvent<T> : MouseEventHandler {
//
//        private readonly Action<T> handler;
//
//        public MouseEventHandler_IgnoreEvent(InputEventType eventType, KeyboardModifiers requiredModifiers, EventPhase phase, Action<T> handler)
//            : base(eventType, requiredModifiers, phase) {
//            this.handler = handler;
//        }
//
//        public override void Invoke(object target, ExpressionContext context, MouseInputEvent evt) {
//            if (ShouldRun(evt)) {
//                handler((T) target);
//            }
//        }
//
//    }
//
//    public class MouseEventHandler_WithEvent<T> : MouseEventHandler {
//
//        private readonly Action<T, MouseInputEvent> handler;
//
//        public MouseEventHandler_WithEvent(InputEventType eventType, KeyboardModifiers requiredModifiers, EventPhase phase, Action<T, MouseInputEvent> handler)
//            : base(eventType, requiredModifiers, phase) {
//            this.handler = handler;
//        }
//
//        public override void Invoke(object target, ExpressionContext context, MouseInputEvent evt) {
//            if (ShouldRun(evt)) {
//                handler((T) target, evt);
//            }
//        }
//
//    }
//
//}