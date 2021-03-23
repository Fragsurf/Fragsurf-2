using System.IO;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public static class DefaultApplicationCommands {
		[MethodCommand]
		public static void Quit() {
			if (!EditorApplication.isPlaying) {
				return;
			}

			EditorApplication.isPlaying = false;
		}

		[MethodCommand]
		public static void Play() {
			if (EditorApplication.isPlaying) {
				return;
			}

			EditorApplication.isPlaying = true;
		}

		//[MethodCommand]
		//public static void DeletePersistentData() {
		//	if (Application.isPlaying) {
		//		Debug.Log("Won't delete persistent data because the application is playing!");
		//		return;
		//	}

		//	DirectoryInfo dataDir = new DirectoryInfo(Application.persistentDataPath);
		//	dataDir.Delete(recursive: true);
		//	Debug.Log("Successfully deleted persistent data!");
		//}

		//[MethodCommand]
		//public static void DeletePlayerPrefs() {
		//	if (Application.isPlaying) {
		//		Debug.Log("Won't delete player prefs because the application is playing!");
		//		return;
		//	}

		//	PlayerPrefs.DeleteAll();
		//	Debug.Log("Successfully deleted player prefs!");
		//}

		//[MethodCommand]
		//public static void DeleteUserData() {
		//	DeletePlayerPrefs();
		//	DeletePersistentData();
		//}
	}
}
