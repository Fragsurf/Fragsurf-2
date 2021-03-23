#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public static class PlayerSettingsUtil {
		public static void AddScriptingDefineSymbolIfNotFound(BuildTargetGroup targetGroup, string symbol) {
			string scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			List<string> scriptingDefines = new List<string>(scriptingDefinesString.Split(';'));
			if (scriptingDefines.Contains(symbol)) {
				return;
			}

			scriptingDefines.Add(symbol);

			PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", scriptingDefines.ToArray()));
		}
	}
}
#endif