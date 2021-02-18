using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public abstract class GameObjectCommandLoader : ICommandLoader {
		// PRAGMA MARK - ICommandLoader
		public ICommand[] Load() {
			GameObject[] gameObjects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];

			List<ICommand> objects = new List<ICommand>();
			foreach (GameObject obj in gameObjects) {
				objects.Add(MakeGameObjectCommand(obj));
			}
			return objects.ToArray();
		}

		protected abstract ICommand MakeGameObjectCommand(GameObject obj);
	}
}