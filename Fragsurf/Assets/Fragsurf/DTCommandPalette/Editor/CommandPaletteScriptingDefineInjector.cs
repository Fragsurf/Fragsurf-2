using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	[InitializeOnLoad]
	public static class CommandPaletteScriptingDefineInjector {
		public static readonly string kScriptingDefineSymbol = "DT_COMMAND_PALETTE";

		static CommandPaletteScriptingDefineInjector() {
			EditorUserBuildSettings.activeBuildTargetChanged += RefreshScriptingDefineSymbolInjection;
			RefreshScriptingDefineSymbolInjection();
		}

		private static void RefreshScriptingDefineSymbolInjection() {
			PlayerSettingsUtil.AddScriptingDefineSymbolIfNotFound(EditorUserBuildSettings.selectedBuildTargetGroup, kScriptingDefineSymbol);
		}
	}
}