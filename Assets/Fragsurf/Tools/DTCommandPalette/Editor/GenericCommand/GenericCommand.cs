using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public class GenericCommand : ICommand {
		// PRAGMA MARK - ICommand
		public string DisplayTitle {
			get { return displayTitle_; }
		}

		public string DisplayDetailText {
			get { return displayDetailText_; }
		}

		public Texture2D DisplayIcon {
			get { return null; }
		}

		public float SortingPriority {
			get { return 0.0f; }
		}

		public bool IsValid() {
			return true;
		}

		public void Execute() {
			if (executeCallback_ != null) {
				executeCallback_.Invoke();
				executeCallback_ = null;
			}
		}


		// PRAGMA MARK - Constructors
		public GenericCommand(string title, Action executeCallback, string detailText = "") {
			displayTitle_ = title;
			executeCallback_ = executeCallback;
			displayDetailText_ = detailText;
		}


		// PRAGMA MARK - Internal
		private string displayTitle_;
		private Action executeCallback_;
		private string displayDetailText_;
	}

	public class GenericCommand<T> : GenericCommand, ICommandWithArguments {
		// PRAGMA MARK - ICommandWithArguments Implementation
		public ArgumentInfo[] Arguments {
			get {
				if (arguments_ == null) {
					arguments_ = new ArgumentInfo[] {
						new ArgumentInfo(argumentName_, typeof(T))
					};
				}
				return arguments_;
			}
		}

		public void Execute(object[] args) {
			try {
				T argument = (T)args[0];
				executeWithArgumentsCallback_.Invoke(argument);
			} catch (Exception e) {
				Debug.LogError(string.Format("Failed to execute GenericCommand<{0}>! Exception: {1}", typeof(T).Name, e));
			}
		}


		// PRAGMA MARK - Internal
		private ArgumentInfo[] arguments_;
		private Action<T> executeWithArgumentsCallback_;
		private string argumentName_;

		public GenericCommand(string title, string argumentName, Action<T> executeWithArgumentsCallback, string detailText = "") : base(title, null, detailText) {
			argumentName_ = argumentName;
			executeWithArgumentsCallback_ = executeWithArgumentsCallback;
		}
	}
}