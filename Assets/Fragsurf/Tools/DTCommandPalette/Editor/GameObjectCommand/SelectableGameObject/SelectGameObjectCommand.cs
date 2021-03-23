using System.IO;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public class SelectGameObjectCommand : GameObjectCommand {
		private static Texture2D selectableGameObjectDisplayIcon_;
		private static Texture2D SelectGameObjectCommandDisplayIcon_ {
			get {
				if (selectableGameObjectDisplayIcon_ == null) {
					selectableGameObjectDisplayIcon_ = AssetDatabase.LoadAssetAtPath(CommandPaletteWindow.ScriptDirectory + "/Icons/GameObjectIcon.png", typeof(Texture2D)) as Texture2D;
				}
				return selectableGameObjectDisplayIcon_ ?? new Texture2D(0, 0);
			}
		}

		// PRAGMA MARK - ICommand
		public override Texture2D DisplayIcon {
			get {
				return SelectGameObjectCommandDisplayIcon_;
			}
		}

		public override void Execute() {
			Selection.activeGameObject = obj_;
		}


		// PRAGMA MARK - Constructors
		public SelectGameObjectCommand(GameObject obj) : base(obj) {
		}
	}
}