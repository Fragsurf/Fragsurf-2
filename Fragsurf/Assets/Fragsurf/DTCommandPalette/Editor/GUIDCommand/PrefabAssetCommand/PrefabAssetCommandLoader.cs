using System;
using System.Collections.Generic;
using UnityEditor;

namespace DTCommandPalette {
	public class PrefabAssetCommandLoader : ICommandLoader {
		// PRAGMA MARK - Public Interface
		public PrefabAssetCommandLoader(Action<string> guidCallback) {
			guidCallback_ = guidCallback;
		}


		// PRAGMA MARK - ICommandLoader
		public ICommand[] Load() {
			if (guidCallback_ == null) {
				return kEmptyArray;
			}

			string[] guids = AssetDatabase.FindAssets("t:Prefab");

			List<ICommand> objects = new List<ICommand>();
			foreach (string guid in guids) {
				objects.Add(new PrefabAssetCommand(guid, guidCallback_));
			}
			return objects.ToArray();
		}


		// PRAGMA MARK - Internal
		private static readonly ICommand[] kEmptyArray = new ICommand[0];

		private Action<string> guidCallback_;
	}
}