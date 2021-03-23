using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DTCommandPalette {
	public static class StringBuilderExtensions {
		public static void Reset(this StringBuilder stringBuilder) {
			stringBuilder.Length = 0;
		}
	}
}
