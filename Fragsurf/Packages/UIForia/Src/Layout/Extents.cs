using UIForia.Systems;
using UnityEngine;

namespace UIForia.Layout {

    public struct Extents {

        public readonly Vector2 min;
        public readonly Vector2 max;

        public Extents(Vector2 min, Vector2 max) {
            this.min = min;
            this.max = max;
        }

        public static Extents zero => new Extents(Vector2.zero, Vector2.zero);

        public Size Size => new Size(max.x - min.x, max.y - min.y);

    }
}
