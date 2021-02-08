using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public class MethodCommandConfig {
		public MethodInfo methodInfo;
		public Type classType;
		public string methodDisplayName;
	}

	public class MethodCommand : ICommandWithArguments {
		private static Texture2D methodDisplayIcon_;
		private static Texture2D MethodDisplayIcon_ {
			get {
				if (methodDisplayIcon_ == null) {
					methodDisplayIcon_ = AssetDatabase.LoadAssetAtPath(CommandPaletteWindow.ScriptDirectory + "/Icons/MethodIcon.png", typeof(Texture2D)) as Texture2D;
				}
				return methodDisplayIcon_ ?? new Texture2D(0, 0);
			}
		}

		// PRAGMA MARK - ICommand
		public string DisplayTitle {
			get {
				return methodDisplayName_;
			}
		}

		public string DisplayDetailText {
			get {
				if (method_.IsStatic) {
					return classType_.Name + "::" + methodDisplayName_;
				} else {
					return classType_.Name + "->" + methodDisplayName_;
				}
			}
		}

		public Texture2D DisplayIcon {
			get {
				return MethodDisplayIcon_;
			}
		}

		public float SortingPriority {
			get { return 0.0f; }
		}

		public bool IsValid() {
			return true;
		}

		public void Execute() {
			ExecuteInteral();
		}


		// PRAGMA MARK - ICommandWithArguments Implementation
		public ArgumentInfo[] Arguments {
			get {
				if (arguments_ == null) {
					arguments_ = method_.GetParameters().Select(p => new ArgumentInfo(p.Name, p.ParameterType)).ToArray();
				}
				return arguments_;
			}
		}

		public void Execute(object[] args) {
			ExecuteInteral(args);
		}


		// PRAGMA MARK - Public Interface
		public bool IsStatic {
			get { return method_.IsStatic; }
		}

		public Type ClassType {
			get { return classType_; }
		}

		public MethodCommand(MethodCommandConfig config) {
			method_ = config.methodInfo;
			methodDisplayName_ = config.methodDisplayName ?? method_.Name;
			classType_ = config.classType;
		}


		// PRAGMA MARK - Internal
		private MethodInfo method_;
		private Type classType_;
		private string methodDisplayName_;
		private ArgumentInfo[] arguments_;

		private void ExecuteInteral(object[] args = null) {
			args = args ?? new object[0];

			var defaultArgs = method_.GetParameters().Skip(args == null ? 0 : args.Length).Select(a => a.IsOptional ? a.DefaultValue : null);
			object[] allArgs = args.Concat(defaultArgs).ToArray();

			if (IsStatic) {
				method_.Invoke(null, allArgs);
			} else if (typeof(UnityEngine.Component).IsAssignableFrom(classType_)) {
				var activeGameObject = Selection.activeGameObject;
				if (activeGameObject == null) {
					Debug.LogWarning("MethodCommand: cannot run method without selected game object!");
					return;
				}

				UnityEngine.Component classTypeComponent = activeGameObject.GetComponent(classType_);
				if (classTypeComponent == null) {
					Debug.LogWarning("MethodCommand: failed to grab component of type: " + classType_.Name + " :from selected game object!");
					return;
				}

				method_.Invoke(classTypeComponent, allArgs);
				SceneView.RepaintAll();
				EditorUtility.SetDirty(classTypeComponent);
			} else {
				Debug.LogWarning("MethodCommand: instance method not assignable to UnityEngine.Component has no way to be run!");
			}
		}
	}
}