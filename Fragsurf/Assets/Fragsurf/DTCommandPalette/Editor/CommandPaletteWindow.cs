using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public class CommandPaletteWindow : EditorWindow {
		// PRAGMA MARK - Constants
		private const string kTextFieldControlName = "CommandPaletteWindowTextField";

		private const int kMaxRowsDisplayed = 8;
		private const float kWindowWidth = 400.0f;
		private const float kWindowHeight = 30.0f;

		private const float kRowHeight = 32.0f;
		private const float kRowTitleHeight = 20.0f;
		private const float kRowSubtitleHeightPadding = -5.0f;
		private const float kRowSubtitleHeight = 15.0f;

		private const int kSubtitleMaxSoftLength = 35;
		private const int kSubtitleMaxTitleAdditiveLength = 15;

		private const float kIconEdgeSize = 15.0f;
		private const float kIconPadding = 7.0f;

		private const int kFontSize = 21;

		public static bool _debug = false;

		public static string _scriptDirectory = null;
		public static string ScriptDirectory {
			get { return ScriptableObjectEditorUtil.PathForScriptableObjectType<CommandPaletteWindow>(); }
		}


		// PRAGMA MARK - Public Interface
		[MenuItem("Window/Command Palette.. %t")]
		public static void ShowObjectWindow() {
			var commandManager = new CommandManager();
			if (!Application.isPlaying) {
				commandManager.AddLoader(new PrefabAssetCommandLoader(PrefabAssetCommand.OnPrefabGUIDExecuted));
				commandManager.AddLoader(new SceneAssetCommandLoader());
			}
			commandManager.AddLoader(new SelectGameObjectCommandLoader());
			commandManager.AddLoader(new MethodCommandLoader());

			InitializeWindow("Command Palette ", commandManager);
		}

		//[MenuItem("Window/Open Command Palette.. %t")]
		//public static void ShowCommandPaletteWindow() {
		//	var commandManager = new CommandManager();
		//	commandManager.AddLoader(new MethodCommandLoader());

		//	InitializeWindow("Command Palette.. ", commandManager);
		//}

		public static void SelectPrefab(string title, Action<GameObject> selectCallback) {
			var commandManager = new CommandManager();
			commandManager.AddLoader(new PrefabAssetCommandLoader((guid) => {
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
				selectCallback.Invoke(prefab);
			}));
			InitializeWindow(title, commandManager, clearInput: true);
		}

		public static void InitializeWindow(string title, CommandManager commandManager, bool clearInput = false) {
			if (inProgressCommand_ != null) {
				Debug.LogWarning("CommandPaletteWindow: Failed to open because of inProgressCommand_!");
				return;
			}

			if (commandManager == null) {
				Debug.LogError("CommandPaletteWindow: Can't initialize a window without a command manager!");
				return;
			}

			if (clearInput) {
				input_ = "";
			}

			EditorWindow window = EditorWindow.GetWindow(typeof(CommandPaletteWindow), utility: true, title: title, focus: true);
			window.position = new Rect(0.0f, 0.0f, kWindowWidth, kWindowHeight);
			window.CenterInMainEditorWindow();
			window.wantsMouseMove = true;

			selectedIndex_ = 0;
			focusTrigger_ = true;
			isClosing_ = false;

			commandManager_ = commandManager;
			ReloadObjects();
		}

		// PRAGMA MARK - Internal
		private static string input_ = "";
		private static bool focusTrigger_ = false;
		private static bool isClosing_ = false;
		private static int selectedIndex_ = 0;
		private static ICommand[] objects_ = new ICommand[0];
		private static CommandManager commandManager_ = null;
		private static Color selectedBackgroundColor_ = ColorUtil.HexStringToColor("#4076d3").WithAlpha(0.4f);
		private static InProgressCommand inProgressCommand_;

		private void OnGUI() {
			Event e = Event.current;
			switch (e.type) {
				case EventType.KeyDown:
					HandleKeyDownEvent(e);
					break;
				default:
					break;
			}

			if (objects_.Length > 0) {
				selectedIndex_ = MathUtil.Wrap(selectedIndex_, 0, Mathf.Min(objects_.Length, kMaxRowsDisplayed));
			} else {
				selectedIndex_ = 0;
			}

			GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
			textFieldStyle.fontSize = kFontSize;

			GUI.SetNextControlName(kTextFieldControlName);
			string updatedInput = EditorGUI.TextField(new Rect(0.0f, 0.0f, kWindowWidth, kWindowHeight), input_, textFieldStyle);
			if (updatedInput != input_) {
				input_ = updatedInput;
				HandleInputUpdated();
			}

			int displayedAssetCount = Mathf.Min(objects_.Length, kMaxRowsDisplayed);
			DrawDropDown(displayedAssetCount);

			this.position = new Rect(this.position.x, this.position.y, this.position.width, kWindowHeight + displayedAssetCount * kRowHeight);

			if (focusTrigger_) {
				focusTrigger_ = false;
				EditorGUI.FocusTextInControl(kTextFieldControlName);
			}
		}

		private void HandleInputUpdated() {
			selectedIndex_ = 0;
			ReloadObjects();
		}

		private static void ReloadObjects() {
			if (commandManager_ == null) {
				return;
			}

			objects_ = commandManager_.ObjectsSortedByMatch(input_);
		}

		private void HandleKeyDownEvent(Event e) {
			switch (e.keyCode) {
				case KeyCode.Escape:
					CloseIfNotClosing();
					break;
				case KeyCode.Return:
					ExecuteCommandAtIndex(selectedIndex_);
					break;
				case KeyCode.DownArrow:
					selectedIndex_++;
					e.Use();
					break;
				case KeyCode.UpArrow:
					selectedIndex_--;
					e.Use();
					break;
				default:
					break;
			}
		}

		private void DrawDropDown(int displayedAssetCount) {
			HashSet<char> inputSet = new HashSet<char>();
			foreach (char c in input_.ToLower()) {
				inputSet.Add(c);
			}

			GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
			titleStyle.fontStyle = FontStyle.Bold;
			titleStyle.richText = true;

			GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label);
			subtitleStyle.fontSize = 9;

			int currentIndex = 0;
			for (int i = 0; i < displayedAssetCount; i++) {
				ICommand command = objects_[i];
				if (!command.IsValid()) {
					continue;
				}

				float topY = kWindowHeight + kRowHeight * currentIndex;

				Rect rowRect = new Rect(0.0f, topY, kWindowWidth, kRowHeight);

				Event e = Event.current;
				if (e.type == EventType.MouseMove) {
					if (rowRect.Contains(e.mousePosition) && selectedIndex_ != currentIndex) {
						selectedIndex_ = currentIndex;
						Repaint();
					}
				} else if (e.type == EventType.MouseDown && e.button == 0) {
					if (rowRect.Contains(e.mousePosition)) {
						ExecuteCommandAtIndex(currentIndex);
					}
				}

				if (currentIndex == selectedIndex_) {
					EditorGUI.DrawRect(rowRect, selectedBackgroundColor_);
				}

				string title = command.DisplayTitle;
				string subtitle = command.DisplayDetailText;

				int subtitleMaxLength = Math.Min(kSubtitleMaxSoftLength + title.Length, kSubtitleMaxSoftLength + kSubtitleMaxTitleAdditiveLength);
				if (subtitle.Length > subtitleMaxLength + 2) {
					subtitle = ".." + subtitle.Substring(subtitle.Length - subtitleMaxLength);
				}

				string colorHex = EditorGUIUtility.isProSkin ? "#8e8e8e" : "#383838";

				StringBuilder consecutiveBuilder = new StringBuilder();
				List<string> consecutives = new List<string>();
				bool startedConsecutive = false;
				foreach (char c in title) {
					if (inputSet.Contains(char.ToLower(c))) {
						startedConsecutive = true;
						consecutiveBuilder.Append(c);
					} else {
						if (startedConsecutive) {
							consecutives.Add(consecutiveBuilder.ToString());
							consecutiveBuilder.Reset();
							startedConsecutive = false;
						}
					}
				}

				// flush whatever is in the string builder
				consecutives.Add(consecutiveBuilder.ToString());

				string maxConsecutive = consecutives.MaxOrDefault(s => s.Length);
				if (!string.IsNullOrEmpty(maxConsecutive)) {
					title = title.ReplaceFirst(maxConsecutive, string.Format("</color>{0}<color={1}>", maxConsecutive, colorHex));
				}
				title = string.Format("<color={0}>{1}</color>", colorHex, title);

				if (_debug) {
					double score = commandManager_.ScoreFor(command, input_);
					subtitle += string.Format(" (score: {0})", score.ToString("F2"));
				}

				EditorGUI.LabelField(new Rect(0.0f, topY, kWindowWidth, kRowTitleHeight), title, titleStyle);
				EditorGUI.LabelField(new Rect(0.0f, topY + kRowTitleHeight + kRowSubtitleHeightPadding, kWindowWidth, kRowSubtitleHeight), subtitle, subtitleStyle);

				GUIStyle textureStyle = new GUIStyle();
				textureStyle.normal.background = command.DisplayIcon;
				EditorGUI.LabelField(new Rect(kWindowWidth - kIconEdgeSize - kIconPadding, topY + kIconPadding, kIconEdgeSize, kIconEdgeSize), GUIContent.none, textureStyle);

				// NOTE (darren): we only increment currentIndex if we draw the object
				// because it is used for positioning the UI
				currentIndex++;
			}
		}

		private void OnFocus() {
			focusTrigger_ = true;
		}

		private void OnLostFocus() {
			CloseIfNotClosing();
		}

		private void CloseIfNotClosing() {
			if (!isClosing_) {
				isClosing_ = true;
				Close();
			}
		}

		private void ExecuteCommandAtIndex(int index) {
			if (!objects_.ContainsIndex(index)) {
				Debug.LogError("Can't execute command with index because out-of-bounds: " + index);
				return;
			}

			ICommand command = objects_[index];

			EditorApplication.delayCall += () => {
				ICommandWithArguments commandWithArguments = command as ICommandWithArguments;
				if (commandWithArguments != null && commandWithArguments.Arguments.Length > 0) {
					inProgressCommand_ = new InProgressCommand(commandWithArguments, HandleInProgressCommandFinished);
				} else {
					command.Execute();
				}
			};

			CloseIfNotClosing();
		}

		private void HandleInProgressCommandFinished() {
			inProgressCommand_ = null;
		}
	}
}