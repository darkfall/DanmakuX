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
		GUILayout.Label("DanmukuX Ver " + DMKSettings.Version + " by Robert Bu (darkfall3@gmail.com)", GUILayout.MaxWidth(this.position.width - 10));
		GUILayout.EndHorizontal();
	
		EditorGUILayout.Space();

		DMKSettings.instance.targetFPS = EditorGUILayout.IntField("Target FPS", DMKSettings.instance.targetFPS);
		DMKSettings.instance.pixelPerUnit = EditorGUILayout.IntField("Pixel To Units", DMKSettings.instance.pixelPerUnit);
		DMKSettings.instance.unitPerPixel = 1f / DMKSettings.instance.pixelPerUnit;
		DMKSettings.instance.frameInterval = 1f / DMKSettings.instance.targetFPS;

		GUILayout.BeginHorizontal("box");
		GUI.skin.label.wordWrap = true;
		GUILayout.Label("Orthographic Size and Offset are measured in units, see Pixel To Units to convert to pixels", GUILayout.MaxWidth(this.position.width - 10));
		GUILayout.EndHorizontal();

		DMKSettings.instance.mainCamera = (Camera)EditorGUILayout.ObjectField("Camera", DMKSettings.instance.mainCamera, typeof(Camera), true);
		if(DMKSettings.instance.mainCamera &&
		   !DMKSettings.instance.mainCamera.isOrthoGraphic) {
			Debug.LogError("DMKSettings: Main Camera must be orthigraphic");
			DMKSettings.instance.mainCamera = null;
		}
		bool customOrthoSize = EditorGUILayout.Toggle("Custom Ortho Size", DMKSettings.instance.useCustomOrthoSize);
		if(customOrthoSize && !DMKSettings.instance.useCustomOrthoSize) {
			Camera camera = DMKSettings.instance.mainCamera == null ? Camera.main : DMKSettings.instance.mainCamera;
			DMKSettings.instance.orthoSizeVertical = camera.orthographicSize;
			DMKSettings.instance.orthoSizeHorizontal = camera.orthographicSize * camera.aspect;
		}
		DMKSettings.instance.useCustomOrthoSize = customOrthoSize;
		if(DMKSettings.instance.useCustomOrthoSize) {
			DMKSettings.instance.centerOffsetX = EditorGUILayout.FloatField("Center Offset X", DMKSettings.instance.centerOffsetX);
			DMKSettings.instance.centerOffsetY = EditorGUILayout.FloatField("Center Offset Y", DMKSettings.instance.centerOffsetY);
			DMKSettings.instance.orthoSizeHorizontal = EditorGUILayout.FloatField("Horizontal Ortho Size", DMKSettings.instance.orthoSizeHorizontal);
			DMKSettings.instance.orthoSizeVertical = EditorGUILayout.FloatField("Vertical Ortho Size", DMKSettings.instance.orthoSizeVertical);
		}

		GUILayout.EndVertical();
	}

}