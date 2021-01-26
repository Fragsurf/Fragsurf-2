using UnityEngine;

namespace UIForia.Systems.Input {

    public class GameInputSystem : InputSystem {

        public GameInputSystem(ILayoutSystem layoutSystem, KeyboardInputManager keyboardInputManager) : base(layoutSystem, keyboardInputManager) { }

        private int clickCount;
        private float lastMouseDownTime;
        private Vector2 lastMouseDownPosition;
        private const float k_clickThresholdSeconds = 0.33f;
        private const float k_clickDistanceThreshold = 3f;
        private Vector2 unsetDownPosition = new Vector2(-1, -1);

        protected override MouseState GetMouseState() {
            MouseState retn = new MouseState();
            retn.leftMouseButtonState.isDown = UnityEngine.Input.GetMouseButton(0);
            retn.rightMouseButtonState.isDown = UnityEngine.Input.GetMouseButton(1);
            retn.middleMouseButtonState.isDown = UnityEngine.Input.GetMouseButton(2);

            retn.leftMouseButtonState.isDownThisFrame = UnityEngine.Input.GetMouseButtonDown(0);
            retn.rightMouseButtonState.isDownThisFrame = UnityEngine.Input.GetMouseButtonDown(1);
            retn.middleMouseButtonState.isDownThisFrame = UnityEngine.Input.GetMouseButtonDown(2);

            retn.leftMouseButtonState.isUpThisFrame = UnityEngine.Input.GetMouseButtonUp(0);
            retn.rightMouseButtonState.isUpThisFrame = UnityEngine.Input.GetMouseButtonUp(1);
            retn.middleMouseButtonState.isUpThisFrame = UnityEngine.Input.GetMouseButtonUp(2);
            
            retn.leftMouseButtonState.downPosition = mouseState.leftMouseButtonState.downPosition;
            retn.rightMouseButtonState.downPosition = mouseState.rightMouseButtonState.downPosition;
            retn.middleMouseButtonState.downPosition = mouseState.middleMouseButtonState.downPosition;
            
            retn.leftMouseButtonState.isDrag = mouseState.leftMouseButtonState.isDrag;
            retn.rightMouseButtonState.isDrag = mouseState.rightMouseButtonState.isDrag;
            retn.middleMouseButtonState.isDrag = mouseState.middleMouseButtonState.isDrag;
            
            
            retn.mousePosition = ConvertMousePosition(UnityEngine.Input.mousePosition);
            
            float now = Time.unscaledTime;

            bool didClick = false;


            if (clickCount > 0 && now - lastMouseDownTime > k_clickThresholdSeconds) {
                clickCount = 0;
            }

            if (retn.isRightMouseDownThisFrame) {
                retn.rightMouseButtonState.downPosition = retn.mousePosition;
            }

            if (retn.isMiddleMouseDownThisFrame) {
                retn.middleMouseButtonState.downPosition = retn.mousePosition;
            }

            if (retn.isLeftMouseDownThisFrame ) {
                retn.leftMouseButtonState.downPosition = retn.mousePosition;
                lastMouseDownTime = now;
                lastMouseDownPosition = retn.leftMouseButtonState.downPosition;
            }
            if (retn.isLeftMouseUpThisFrame) {
                if (clickCount == 0 || now - lastMouseDownTime <= k_clickThresholdSeconds) {
                    if (Vector2.Distance(lastMouseDownPosition, retn.mousePosition) <= k_clickDistanceThreshold / global::UIForia.Application.dpiScaleFactor) {
                        clickCount++;
                        didClick = true;
                    }
                }

                if (!retn.isLeftMouseDownThisFrame) {
                    retn.leftMouseButtonState.downPosition = unsetDownPosition;
                }
            }

            retn.isSingleClick = didClick && clickCount == 1;
            retn.isDoubleClick = didClick && clickCount == 2;
            retn.isTripleClick = didClick && clickCount == 3;
            retn.clickCount = clickCount;
            retn.scrollDelta = UnityEngine.Input.mouseScrollDelta;
            retn.previousMousePosition = mouseState.mousePosition;

            return retn;
        }

        private static Vector2 ConvertMousePosition(Vector2 position) {
            Vector2 scaledPosition = position / Application.dpiScaleFactor;
            float scaledHeight = Application.UiApplicationSize.height / Application.dpiScaleFactor;
            return new Vector2(scaledPosition.x, scaledHeight - scaledPosition.y);
        }
    }
}