using System;
using System.Collections.Generic;
using UnityEditor;

using DTCommandPalette.Internal;

namespace DTCommandPalette {
	public abstract class AssetCommandLoader<T> : ICommandLoader where T : UnityEngine.Object {
		// PRAGMA MARK - ICommandLoader
		public ICommand[] Load() {
			List<T> assets = AssetDatabaseUtil.AllAssetsOfType<T>();

			List<ICommand> objects = new List<ICommand>();
			foreach (var asset in assets) {
				objects.Add(new AssetCommand<T>(asset, HandleAssetExecuted));
			}
			return objects.ToArray();
		}


		// PRAGMA MARK - Internal
		protected abstract void HandleAssetExecuted(T asset);
	}
}