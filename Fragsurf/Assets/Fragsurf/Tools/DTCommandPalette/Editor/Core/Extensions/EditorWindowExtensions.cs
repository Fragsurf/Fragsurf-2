#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DTCommandPalette {
	public static class EditorWindowExtensions {
		/// <summary>
		/// Gets the main editor window position
		/// </summary>
		public static Rect GetEditorMainWindowPosition() {
			System.Type containerWinType = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject))
			.Where(t => t.Name == "ContainerWindow").FirstOrDefault();
			if (containerWinType == null) {
				throw new System.MissingMemberException("Can't find internal type ContainerWindow.");
			}

			FieldInfo showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			PropertyInfo positionProperty = containerWinType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (showModeField == null || positionProperty == null) {
				throw new System.MissingMemberException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed with Unity");
			}

			object[] windows = Resources.FindObjectsOfTypeAll(containerWinType);
			foreach (object window in windows) {
				int showMode = (int)showModeField.GetValue(window);

				// if this window is the main window
				if (showMode == 4) {
					Rect position = (Rect)positionProperty.GetValue(window, null);
					return position;
				}
			}

			throw new System.NotSupportedException("Can't find internal main window. Maybe something has changed with Unity");
		}


		/// <summary>
		/// Centers this window within the main editor window
		/// </summary>
		public static void CenterInMainEditorWindow(this EditorWindow window) {
			Rect mainEditorWindowPos = GetEditorMainWindowPosition();
			Rect windowPos = window.position;
			float w = (mainEditorWindowPos.width - windowPos.width) * 0.5f;
			float h = (mainEditorWindowPos.height - windowPos.height) * 0.5f;
			windowPos.x = mainEditorWindowPos.x + w;
			windowPos.y = mainEditorWindowPos.y = h;
			window.position = windowPos;
		}
	}
}
#endif