using UnityEngine;

namespace UIForia.Util {

    public struct ReadOnlyVector2 {

        public readonly float x;
        public readonly float y;

        public ReadOnlyVector2(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public ReadOnlyVector2(Vector2 vec) {
            this.x = vec.x;
            this.y = vec.y;
        }

        public static implicit operator Vector2(ReadOnlyVector2 vec) {
            return new Vector2(vec.x, vec.y);
        }

        public static implicit operator ReadOnlyVector2(Vector2 vec) {
            return new ReadOnlyVector2(vec.x, vec.y);
        }
        
    }

}