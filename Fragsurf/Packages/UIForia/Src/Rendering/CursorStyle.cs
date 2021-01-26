using UnityEngine;

namespace UIForia.Rendering {

    public class CursorStyle {

        public readonly string name;

        public readonly Texture2D texture;

        public readonly Vector2 hotSpot;

        public CursorStyle(string name, Texture2D texture, Vector2 hotSpot) {
            this.name = name;
            this.texture = texture;
            this.hotSpot = hotSpot;
        }

        public bool Equals(CursorStyle other) {
            if (other == null) return false;
            return string.Equals(name, other.name) && Equals(texture, other.texture) && hotSpot.Equals(other.hotSpot);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CursorStyle other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (texture != null ? texture.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ hotSpot.GetHashCode();
                return hashCode;
            }
        }

    }

}