using System;
using Packages.UIForia.Src.Systems;
using UnityEngine;

namespace UIForia.Systems.Input {

    public struct MouseState {
        
        public MouseButtonState leftMouseButtonState;
        public MouseButtonState middleMouseButtonState;
        public MouseButtonState rightMouseButtonState;
        
        public Vector2 mousePosition;
        public Vector2 previousMousePosition;

        public Vector2 scrollDelta;

        public bool isDoubleClick;
        public bool isTripleClick;
        public bool isSingleClick;
        public int clickCount;
        public bool isLeftMouseUpThisFrame => leftMouseButtonState.isUpThisFrame;
        public bool isRightMouseUpThisFrame => rightMouseButtonState.isUpThisFrame;
        public bool isMiddleMouseUpThisFrame => middleMouseButtonState.isUpThisFrame;
        
        public bool isLeftMouseDownThisFrame => leftMouseButtonState.isDownThisFrame;
        public bool isRightMouseDownThisFrame => rightMouseButtonState.isDownThisFrame;
        public bool isMiddleMouseDownThisFrame => middleMouseButtonState.isDownThisFrame;
        
        public bool isLeftMouseDown => leftMouseButtonState.isDown;
        public bool isRightMouseDown => rightMouseButtonState.isDown;
        public bool isMiddleMouseDown => middleMouseButtonState.isDown;

        public Vector2 MouseDownDelta(MouseButtonType mouseButtonType) {

            MouseButtonState state;
            
            switch (mouseButtonType) {
                case MouseButtonType.Left:
                    state = leftMouseButtonState;
                    break;
                case MouseButtonType.Middle:
                    state = middleMouseButtonState;
                    break;
                case MouseButtonType.Right:
                    state = rightMouseButtonState;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mouseButtonType), mouseButtonType, null);
            }
            
            if (state.downPosition.x < 0 || state.downPosition.y < 0) {
                return Vector2.zero;
            }

            return mousePosition - state.downPosition;
        }

        public Vector2 MouseDelta => previousMousePosition - mousePosition;
        public bool DidMove => MouseDelta.sqrMagnitude > 0;
        public bool AnyMouseDown => leftMouseButtonState.isDown || rightMouseButtonState.isDown || middleMouseButtonState.isDown;
        public bool AnyMouseDownThisFrame => leftMouseButtonState.isDownThisFrame || rightMouseButtonState.isDownThisFrame || middleMouseButtonState.isDownThisFrame;
        public bool ReleasedDrag => leftMouseButtonState.ReleasedDrag || rightMouseButtonState.ReleasedDrag || middleMouseButtonState.ReleasedDrag;

        public Vector2 MouseDownPosition => leftMouseButtonState.isDown
            ? leftMouseButtonState.downPosition
            : rightMouseButtonState.isDown
                ? rightMouseButtonState.downPosition
                : middleMouseButtonState.isDown
                    ? middleMouseButtonState.downPosition
                    : Vector2.zero;

    }
}
