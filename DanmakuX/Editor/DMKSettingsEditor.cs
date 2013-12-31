using UnityEditor;
using UnityEngine;
using System;

public class DMKSettingsEditor: EditorWindow {

	public static void Create() {
		DMKSettingsEditor editor = (DMKSettingsEditor)EditorWindow.GetWindow<DMKSettingsEditor>("DanmakuX Settings", true);
	}

	public void OnGUI() {
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("DanmukuX Ver 0.1 by Robert Bu (darkfall3@gmail.com)");
		GUILayout.EndHorizontal();
	
		EditorGUILayout.Space();

		DMKSettings.targetFPS = EditorGUILayout.IntField("Target FPS", DMKSettings.targetFPS);
		DMKSettings.pixelPerUnit = EditorGUILayout.IntField("Pixel To Units", DMKSettings.pixelPerUnit);
		DMKSettings.unitPerPixel = 1f / DMKSettings.pixelPerUnit;
		GUILayout.EndVertical();
	}

}