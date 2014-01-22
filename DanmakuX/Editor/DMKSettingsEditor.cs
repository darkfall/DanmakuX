using UnityEditor;
using UnityEngine;
using System;

namespace danmakux {

	public class DMKSettingsEditor: EditorWindow {

		public static void Create() {
			EditorWindow.GetWindow<DMKSettingsEditor>("DanmakuX Settings", true);
		}

		public static void SettingsGUI() {
			GUILayout.BeginVertical();
			GUI.skin.label.wordWrap = false;

			EditorGUILayout.HelpBox("DanmukuX Ver " + DMKSettings.Version + " by Robert Bu (darkfall3@gmail.com)", MessageType.None);

			EditorGUILayout.Space();

			DMKSettings settings = DMKSettings.instance;
			
			settings.targetFPS = EditorGUILayout.IntField("Target FPS", settings.targetFPS);
			settings.pixelPerUnit = EditorGUILayout.IntField("Pixel To Units", settings.pixelPerUnit);
			settings.unitPerPixel = 1f / settings.pixelPerUnit;
			settings.frameInterval = 1f / settings.targetFPS;

			EditorGUILayout.HelpBox("Global max number of bullets. Can be changed in individual danmakus.", MessageType.None);
			settings.MaxNumBullets = EditorGUILayout.IntField("Max N-Bullets", settings.MaxNumBullets);
			EditorGUILayout.Space();

			settings.sortingLayerIndex = EditorGUILayout.Popup("Sorting Layer", settings.sortingLayerIndex, DMKUtil.GetSortingLayerNames());
			settings.sortingOrder = EditorGUILayout.IntField("Sorting Order", settings.sortingOrder);

			EditorGUILayout.HelpBox("If bullets are stored by controller, then bullets will die when the controller dies", MessageType.None);
			settings.bulletStorePolicy = (DMKBulletStorePolicy)EditorGUILayout.EnumPopup("Bullet Store Policy", settings.bulletStorePolicy);

			EditorGUILayout.HelpBox("Orthographic Size and Offset are measured in units, see Pixel To Units to convert to pixels", MessageType.None);
		
			settings.mainCamera = (Camera)EditorGUILayout.ObjectField("Camera", settings.mainCamera, typeof(Camera), true);
			if(settings.mainCamera &&
			   !settings.mainCamera.isOrthoGraphic) {
				Debug.LogError("DMKSettings: Main Camera must be orthigraphic");
				settings.mainCamera = null;
			}
			bool customOrthoSize = EditorGUILayout.Toggle("Custom Ortho Size", settings.useCustomOrthoSize);
			if(customOrthoSize && !settings.useCustomOrthoSize) {
				Camera camera = settings.mainCamera == null ? Camera.main : settings.mainCamera;
				settings.orthoSizeVertical = camera.orthographicSize;
				settings.orthoSizeHorizontal = camera.orthographicSize * camera.aspect;
			}
			settings.useCustomOrthoSize = customOrthoSize;
			if(settings.useCustomOrthoSize) {
				EditorGUI.BeginChangeCheck();
				settings.centerOffsetX = EditorGUILayout.FloatField("Center Offset X", settings.centerOffsetX);
				settings.centerOffsetY = EditorGUILayout.FloatField("Center Offset Y", settings.centerOffsetY);
				settings.orthoSizeHorizontal = EditorGUILayout.FloatField("Horizontal Ortho Size", settings.orthoSizeHorizontal);
				settings.orthoSizeVertical = EditorGUILayout.FloatField("Vertical Ortho Size", settings.orthoSizeVertical);
				if(EditorGUI.EndChangeCheck()) {
					SceneView.RepaintAll();
				}
			}

			GUI.skin.label.wordWrap = true;
			GUILayout.EndVertical();
		}

		public void OnGUI() {
			SettingsGUI();
		}

	}

	
}
