using UnityEngine;

namespace UIForia.Extensions {

    public static class ColorExtensions {

        public static bool IsDefined(this Color color) {
            return color.r >= 0 && color.g >= 0 && color.b >= 0 && color.a >= 0;
        }

    }

}