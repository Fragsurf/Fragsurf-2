using System.IO;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public abstract class GUIDCommand : ICommand {
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

		public abstract Texture2D DisplayIcon {
			get;
		}

		public float SortingPriority {
			get { return 0.0f; }
		}

		public bool IsValid() {
			return true;
		}

		public abstract void Execute();


		// PRAGMA MARK - Constructors
		public GUIDCommand(string guid) {
			guid_ = guid;
			path_ = AssetDatabase.GUIDToAssetPath(guid_);
			assetFileName_ = Path.GetFileName(path_);
		}


		// PRAGMA MARK - Internal
		protected string guid_;
		protected string path_;
		protected string assetFileName_;
	}
}