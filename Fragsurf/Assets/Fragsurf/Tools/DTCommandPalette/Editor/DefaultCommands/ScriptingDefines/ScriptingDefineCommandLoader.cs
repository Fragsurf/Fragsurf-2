using System;
using System.Collections.Generic;
using UnityEngine;

using DTCommandPalette.ScriptingDefines;

namespace DTCommandPalette.DefaultCommands {
	public class ScriptingDefineCommandLoader : ICommandLoader {
		// PRAGMA MARK - Public Interface
		public ScriptingDefineCommandLoader(Action<string> executeAction) {
			executeAction_ = executeAction;
		}


		// PRAGMA MARK - ICommandLoader
		public ICommand[] Load() {
			List<ICommand> objects = new List<ICommand>();

			foreach (string symbol in ScriptingDefinesManager.GetCurrentDefines()) {
				objects.Add(new ScriptingDefineCommand(symbol, executeAction_));
			}

			return objects.ToArray();
		}


		// PRAGMA MARK - Internal
		private Action<string> executeAction_;
	}
}
