using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

using DTCommandPalette.ScriptingDefines;

namespace DTCommandPalette.DefaultCommands {
	public static class ScriptingDefinesCommands {
		[MethodCommand]
		public static void ScriptingDefines() {
			var commandManager = new CommandManager();
			commandManager.AddCommand(new GenericCommand<string>("Add New Define", "ScriptingDefine", AddScriptingDefineIfNotFound));
			commandManager.AddLoader(new ScriptingDefineCommandLoader(s => ShowScriptingDefineContextMenu(s)));

			CommandPaletteWindow.InitializeWindow("Scripting Defines..", commandManager, clearInput: true);
		}


		private static void ShowScriptingDefineContextMenu(string scriptingDefine) {
			var commandManager = new CommandManager();
			commandManager.AddCommand(new GenericCommand("Remove", () => RemoveDefine(scriptingDefine)));

			CommandPaletteWindow.InitializeWindow(scriptingDefine, commandManager, clearInput: true);
		}

		private static void AddScriptingDefineIfNotFound(string scriptingDefine) {
			bool addedDefine = ScriptingDefinesManager.AddDefineIfNotFound(scriptingDefine);
			if (addedDefine) {
				Debug.Log(string.Format("Added scripting define: {0}!", scriptingDefine));
			} else {
				Debug.Log(string.Format("Scripting define already defined, did not add: {0}!", scriptingDefine));
			}
		}

		private static void RemoveDefine(string scriptingDefine) {
			bool removed = ScriptingDefinesManager.RemoveDefine(scriptingDefine);
			if (removed) {
				Debug.Log(string.Format("Removed scripting define: {0}!", scriptingDefine));
			} else {
				Debug.Log(string.Format("Scripting define not found, did not remove: {0}!", scriptingDefine));
			}
		}
	}
}
