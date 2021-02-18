using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DTCommandPalette {
	public class SceneAssetCommand : GUIDCommand {
		private static Texture2D sceneDisplayIcon_;
		private static Texture2D SceneDisplayIcon_ {
			get {
				if (sceneDisplayIcon_ == null) {
					sceneDisplayIcon_ = AssetDatabase.LoadAssetAtPath(CommandPaletteWindow.ScriptDirectory + "/Icons/SceneIcon.png", typeof(Texture2D)) as Texture2D;
				}
				return sceneDisplayIcon_ ?? new Texture2D(0, 0);
			}
		}

		// PRAGMA MARK - ICommand
		public override Texture2D DisplayIcon {
			get { return SceneDisplayIcon_; }
		}

		public override void Execute() {
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
				EditorSceneManager.OpenScene(path_);
			}
		}


		// PRAGMA MARK - Constructors
		public SceneAssetCommand(string guid) : base(guid) {
			if (!PathUtil.IsScene(path_)) {
				throw new ArgumentException("SceneAssetCommand loaded with guid that's not a scene!");
			}
		}
	}
}