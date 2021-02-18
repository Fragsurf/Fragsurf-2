using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public class CommandPaletteArgumentWindow : EditorWindow {
		// PRAGMA MARK - Constants
		private const string kTextFieldControlName = "CommandPaletteArgumentWindowTextField";

		private const float kWindowWidth = 400.0f;
		private const float kWindowHeight = 30.0f;

		private const int kFontSize = 21;


		// PRAGMA MARK - Public Interface
		public static void Show(string title, Action<string> argumentCallback, Action cancelCallback = null) {
			cancelCallback_ = cancelCallback;
			argumentCallback_ = argumentCallback;

			input_ = "";

			EditorWindow window = EditorWindow.GetWindow(typeof(CommandPaletteArgumentWindow), utility: true, title: title, focus: true);
			window.position = new Rect(0.0f, 0.0f, kWindowWidth, kWindowHeight);
			window.CenterInMainEditorWindow();
			window.wantsMouseMove = true;

			focusTrigger_ = true;
			isClosing_ = false;
		}

		// PRAGMA MARK - Internal
		private static string input_ = "";
		private static bool focusTrigger_ = false;
		private static bool isClosing_ = false;

		private static Action cancelCallback_;
		private static Action<string> argumentCallback_;

		private void OnGUI() {
			Event e = Event.current;
			switch (e.type) {
				case EventType.KeyDown:
					HandleKeyDownEvent(e);
					break;
				default:
					break;
			}

			GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
			textFieldStyle.fontSize = kFontSize;

			GUI.SetNextControlName(kTextFieldControlName);
			input_ = EditorGUI.TextField(new Rect(0.0f, 0.0f, kWindowWidth, kWindowHeight), input_, textFieldStyle);

			this.position = new Rect(this.position.x, this.position.y, this.position.width, kWindowHeight);

			if (focusTrigger_) {
				focusTrigger_ = false;
				EditorGUI.FocusTextInControl(kTextFieldControlName);
			}
		}

		private void HandleKeyDownEvent(Event e) {
			switch (e.keyCode) {
				case KeyCode.Escape:
					CloseIfNotClosing();
					break;
				case KeyCode.Return:
					ReturnArgument(input_);
					break;
				default:
					break;
			}
		}

		private void ReturnArgument(string argument) {
			CloseIfNotClosing();
			cancelCallback_ = null;
			if (argumentCallback_ != null) {
				var argumentCallback = argumentCallback_;
				argumentCallback_ = null;
				argumentCallback.Invoke(argument);
			}
		}

		private void OnFocus() {
			focusTrigger_ = true;
		}

		private void OnLostFocus() {
			CloseIfNotClosing();
		}

		private void CloseIfNotClosing() {
			if (cancelCallback_ != null) {
				cancelCallback_.Invoke();
				cancelCallback_ = null;
			}

			if (!isClosing_) {
				isClosing_ = true;
				Close();
			}
		}
	}
}