using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;

namespace ElementTests {
    
    [Template("Data/Element/DragElementWithSyncBinding.xml")]
    public class DragElementWithSyncBinding : UIElement {
        public float value;

        private Action<Vector2> onDragUpdate;

        public override void OnCreate() {
            onDragUpdate = OnDragUpdate;
        }

        private void OnDragUpdate(Vector2 position) {
            value = position.x;
        }

        public class TestDragEvent : DragEvent {
            public readonly Action<Vector2> dragUpdate;

            public TestDragEvent(Action<Vector2> dragUpdate) {
                this.dragUpdate = dragUpdate;
            }

            public override void Update() {
                dragUpdate?.Invoke(MousePosition);
            }
        }

        [OnDragCreate()]
        public DragEvent OnDragCreate() {
            return new TestDragEvent(onDragUpdate);
        }
    }
}