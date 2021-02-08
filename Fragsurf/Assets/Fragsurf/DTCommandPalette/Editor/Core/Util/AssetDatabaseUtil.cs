#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette.Internal {
	public class ClearCachedAssetsOnPostProcess : AssetPostprocessor {
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			AssetDatabaseUtil.ClearCachedAssets();
		}
	}

	public static class AssetDatabaseUtil {
		public static void ClearCachedAssets() {
			_cachedAssets.Clear();
		}

		public static List<T> AllAssetsOfType<T>() where T : UnityEngine.Object {
			var type = typeof(T);
			if (!_cachedAssets.ContainsKey(type)) {
				List<T> assets = new List<T>();

				var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
				foreach (string guid in guids) {
					var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
					if (asset == null) {
						continue;
					}

					assets.Add(asset);
				}

				_cachedAssets[type] = assets;
			}

			return (List<T>)_cachedAssets[type];
		}


		private static Dictionary<Type, object> _cachedAssets = new Dictionary<Type, object>();
	}
}
#endif