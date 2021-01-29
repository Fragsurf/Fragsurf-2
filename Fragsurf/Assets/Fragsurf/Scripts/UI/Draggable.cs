using Fragsurf.Utility;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fragsurf.UI
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Draggable : GameVisualElement, IPreserveData
    {
        public new class UxmlFactory : UxmlFactory<Draggable> { }

        private Vector2 _startVector;
        private Vector2 _diffVector;
        private bool _dragging;
        private bool _resizing;

        public Rect DragBoundary { get; set; }

        [JsonProperty]
        public SVector2 Position
        {
            get => transform.position;
            set => SetPosition(value);
        }

        [JsonProperty]
        public SVector2 Size
        { 
            get => new Vector2(resolvedStyle.width, resolvedStyle.height);
            set => SetSize(value);
        }

        protected override void Initialize()
        {
            DragBoundary = panel.visualTree.worldBound;

            RegisterCallback<MouseDownEvent>(MouseDown);
        }

        private void MouseDown(MouseDownEvent ev)
        {
            style.width = resolvedStyle.width;
            style.height = resolvedStyle.height;

            _dragging = true;
            _startVector = transform.position;
            _diffVector = ScaledMousePosition(Input.mousePosition);

            var brRect = new Rect(worldBound.xMax - 32, worldBound.yMax - 32, 32, 32);
            if (brRect.Contains(ev.mousePosition, true))
            {
                _resizing = true;
                _startVector = new Vector2(resolvedStyle.width, resolvedStyle.height);
            }
        }

        private Vector2 ScaledMousePosition(Vector2 input)
        {
            input.x *= DragBoundary.width / Screen.width;
            input.y *= DragBoundary.height / -Screen.height;
            return input;
        }

        private void SetSize(Vector2 size)
        {
           
            size = ClampSize(size);
            style.width = size.x;
            style.height = size.y;
        }

        private void SetPosition(Vector2 position)
        {
            transform.position = position;
            ClampPosition();
        }

        public override void Update()
        {
            if (!Input.GetKey(KeyCode.Mouse0))
            {
                _dragging = false;
                _resizing = false;
            }

            if (_dragging)
            {
                var diff = _startVector + ScaledMousePosition(Input.mousePosition) - _diffVector;

                if (_resizing)
                {
                    SetSize(diff);
                }
                else
                {
                    SetPosition(diff);
                }
            }
        }

        private Vector2 ClampSize(Vector2 size)
        {
            var maxX = size.x + worldBound.x;
            var maxY = size.y + worldBound.y;

            if(maxX > DragBoundary.width)
            {
                size.x -= maxX - DragBoundary.width;
            }

            if (maxY > DragBoundary.height)
            {
                size.y -= maxY - DragBoundary.height;
            }

            size.x = Mathf.Max(size.x, 128);
            size.y = Mathf.Max(size.y, 128);

            return size;
        }

        private void ClampPosition()
        {
            var clampVector = new Vector3();

            if (worldBound.x < 0)
            {
                clampVector.x = -worldBound.x;
            }
            else if(worldBound.xMax > DragBoundary.width)
            {
                clampVector.x = DragBoundary.width - worldBound.xMax;
            }

            if (worldBound.y < 0)
            {
                clampVector.y = -worldBound.y;
            }
            else if (worldBound.yMax > DragBoundary.height)
            {
                clampVector.y = DragBoundary.height - worldBound.yMax;
            }

            transform.position = transform.position + clampVector;
        }

    }
}

