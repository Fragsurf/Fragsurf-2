using System.IO;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public abstract class GameObjectCommand : ICommand {
		// PRAGMA MARK - ICommand
		public string DisplayTitle {
			get {
				if (!IsValid()) {
					return "";
				}
				return obj_.name;
			}
		}

		public string DisplayDetailText {
			get {
				if (!IsValid()) {
					return "";
				}
				return obj_.FullName();
			}
		}

		public abstract Texture2D DisplayIcon {
			get;
		}

		public float SortingPriority {
			get { return -0.5f; }
		}

		public bool IsValid() {
			return obj_ != null;
		}

		public abstract void Execute();


		// PRAGMA MARK - Constructors
		public GameObjectCommand(GameObject obj) {
			obj_ = obj;
		}


		// PRAGMA MARK - Internal
		protected GameObject obj_;
	}
}