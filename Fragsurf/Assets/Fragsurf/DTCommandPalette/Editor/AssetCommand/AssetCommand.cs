using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public sealed class AssetCommand<T> : ICommand where T : UnityEngine.Object {
		// PRAGMA MARK - ICommand
		public string DisplayTitle {
			get {
				return assetFileName_;
			}
		}

		public string DisplayDetailText {
			get {
				return path_;
			}
		}

		public Texture2D DisplayIcon {
			get { return null; }
		}

		public float SortingPriority {
			get { return 0.0f; }
		}

		public bool IsValid() {
			return true;
		}

		public void Execute() {
			executeHandler_.Invoke(asset_);
		}


		// PRAGMA MARK - Constructors
		public AssetCommand(T asset, Action<T> executeHandler) {
			if (executeHandler == null) {
				Debug.LogError("No execute handler for AssetCommand<" + typeof(T).Name + ">!");
				return;
			}

			asset_ = asset;
			executeHandler_ = executeHandler;
			path_ = AssetDatabase.GetAssetPath(asset_);
			assetFileName_ = Path.GetFileName(path_);
		}


		// PRAGMA MARK - Internal
		private T asset_;
		private string path_;
		private string assetFileName_;
		private Action<T> executeHandler_;
	}
}