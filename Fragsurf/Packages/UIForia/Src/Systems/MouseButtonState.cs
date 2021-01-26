using Packages.UIForia.Src.Systems;
using UnityEngine;

namespace UIForia.Systems.Input {
    public struct MouseButtonState {
        public MouseButtonType mouseButtonType;
        public bool isDrag;
        public bool isUp;
        public bool isDown;
        public float downTimestamp;
        public Vector2 downPosition;
        public Vector2 upPosition;
        public int clickCount;
        public bool isUpThisFrame;
        public bool isDownThisFrame;

        public bool ReleasedDrag => isDrag && !isDown;
    }
}