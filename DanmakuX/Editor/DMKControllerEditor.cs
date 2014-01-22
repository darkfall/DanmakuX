using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace danmakux {

	[CustomEditor (typeof(DMKController))]
	class DMKControllerEditor: Editor {

		DMKController selectedController = null;

		public void OnEnable() {
			EditorApplication.update += OnUpdateCallback;

			if(Selection.activeObject != null) {
				GameObject targetObj = Selection.activeObject as GameObject;
				selectedController = targetObj.GetComponent<DMKController>();
			}
		}

		void OnDisable()
		{
			EditorApplication.update -= OnUpdateCallback;
		}

		private void OnUpdateCallback()
		{
			selectedController.Update();
		}

		private void PlayerGUI() {
			GUILayout.BeginVertical();
			GUILayout.Label("Preview (" + selectedController.bulletContainer.Count.ToString() + " Bullets)");

			/*
			GUILayout.BeginVertical();
			selectedController.maxBulletCount = EditorGUILayout.IntField("Max Num Bullets", selectedController.maxBulletCount);
			GUILayout.EndVertical();

			if(selectedController.danmakus.Count == 0) {
				EditorGUILayout.HelpBox("No Danmakus Available", MessageType.Info);
			} else {
				selectedPreviewIndex = DMKGUIUtility.MakeSimpleList(selectedPreviewIndex, selectedController.danmakus, () => {
					selectedController.StartDanmaku(-1);
					selectedController.paused = false;
				});
				if(selectedPreviewIndex >= selectedController.danmakus.Count)
					selectedPreviewIndex = -1;
				
				GUILayout.BeginHorizontal();
				{
					if(selectedPreviewIndex != -1) {
						if(selectedController.currentAttackIndex != -1) {
							if(GUILayout.Button("Stop")) {
								selectedController.StartDanmaku(-1);
								selectedController.paused = false;
							}
						} else {
							if(GUILayout.Button("Play")) {
								selectedController.StartDanmaku(selectedPreviewIndex);
							}
						}
						selectedController.paused = (GUILayout.Toggle(selectedController.paused, "Pause", "button"));
					} else {
						GUI.enabled = false;
						GUILayout.Button("Play");
						GUILayout.Toggle(false, "Pause", "button");
						GUI.enabled = false;
					}
				}
				GUILayout.EndHorizontal();
			}*/


			GUILayout.EndVertical();
		}
		
		public override void OnInspectorGUI() {
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Danmaku Editor")) {
				DMKDanmakuEditorX.Create();
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Separator();
			this.PlayerGUI();
		}

	};
		
}
