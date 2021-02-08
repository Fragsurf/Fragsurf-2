using System;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public class PrefabAssetCommand : GUIDCommand {
		public static Action<string> OnPrefabGUIDExecuted;

		private static Texture2D prefabDisplayIcon_;
		private static Texture2D PrefabDisplayIcon_ {
			get {
				if (prefabDisplayIcon_ == null) {
					prefabDisplayIcon_ = AssetDatabase.LoadAssetAtPath(CommandPaletteWindow.ScriptDirectory + "/Icons/PrefabIcon.png", typeof(Texture2D)) as Texture2D;
				}
				return prefabDisplayIcon_ ?? new Texture2D(0, 0);
			}
		}

		// PRAGMA MARK - ICommand
		public override Texture2D DisplayIcon {
			get { return PrefabDisplayIcon_; }
		}

		public override void Execute() {
			if (guidCallback_ == null) {
				Debug.LogError("No guidCallback_ to handle Prefabs Asset Commands!");
			} else {
				guidCallback_.Invoke(guid_);
			}
		}


		// PRAGMA MARK - Constructors
		public PrefabAssetCommand(string guid, Action<string> guidCallback) : base(guid) {
			if (!PathUtil.IsPrefab(path_)) {
				throw new ArgumentException("PrefabAssetCommand loaded with guid that's not a prefab!");
			}

			guidCallback_ = guidCallback;
		}


		// PRAGMA MARK - Internal
		private Action<string> guidCallback_;
	}
}