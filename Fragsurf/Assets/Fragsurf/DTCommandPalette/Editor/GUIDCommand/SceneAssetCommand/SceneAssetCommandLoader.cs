using System;
using System.Collections.Generic;
using UnityEditor;

namespace DTCommandPalette {
	public class SceneAssetCommandLoader : ICommandLoader {
		// PRAGMA MARK - ICommandLoader
		public ICommand[] Load() {
			string[] guids = AssetDatabase.FindAssets("t:Scene");

			List<ICommand> objects = new List<ICommand>();
			foreach (string guid in guids) {
				objects.Add(new SceneAssetCommand(guid));
			}
			return objects.ToArray();
		}
	}
}