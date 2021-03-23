using System;
using System.Collections.Generic;
using UnityEngine;

using DTCommandPalette.ScriptingDefines;

namespace DTCommandPalette.DefaultCommands {
	public class ScriptingDefineCommand : ICommand {
		// PRAGMA MARK - ICommand
		public string DisplayTitle {
			get { return symbol_; }
		}

		public string DisplayDetailText {
			get { return ""; }
		}

		public Texture2D DisplayIcon {
			get { return null; }
		}

		public float SortingPriority {
			get { return 0.0f; }
		}

		public bool IsValid() {
			return executeAction_ != null;
		}

		public void Execute() {
			executeAction_.Invoke(symbol_);
		}


		// PRAGMA MARK - Constructors
		public ScriptingDefineCommand(string symbol, Action<string> executeAction) {
			symbol_ = symbol;
			executeAction_ = executeAction;
		}


		// PRAGMA MARK - Internal
		private string symbol_;
		private Action<string> executeAction_;
	}
}
