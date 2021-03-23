using System.Collections;
using UnityEngine;

namespace DTCommandPalette {
	public static class ColorUtil {
		/// <summary>
		/// Creates a washed out color (random color mixed with white) that isn't too bad looking
		/// Not sure it's the best way to generate nice colors, but it's alright.
		/// </summary>
		public static Color RandomPleasingColor() {
			float red = (Random.value + 1.0f) / 2.0f;
			float green = (Random.value + 1.0f) / 2.0f;
			float blue = (Random.value + 1.0f) / 2.0f;

			return new Color(red, green, blue);
		}

		public static Color HexStringToColor(string hex) {
			hex = hex.Replace("0x", "");        // in case the string is formatted 0xFFFFFF
			hex = hex.Replace("#", "");         // in case the string is formatted #FFFFFF
			byte a = 255;                        // assume fully visible unless specified in hex
			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			// Only use alpha if the string has enough characters
			if (hex.Length == 8) {
				a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			}
			return new Color32(r, g, b, a);
		}
	}
}
