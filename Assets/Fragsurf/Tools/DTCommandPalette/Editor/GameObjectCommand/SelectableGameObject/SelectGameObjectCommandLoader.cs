using System;
using UnityEngine;

namespace DTCommandPalette {
	public class SelectGameObjectCommandLoader : GameObjectCommandLoader {
		protected override ICommand MakeGameObjectCommand(GameObject obj) {
			return new SelectGameObjectCommand(obj);
		}
	}
}