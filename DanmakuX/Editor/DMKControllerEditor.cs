using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
		GUILayout.BeginVertical("box");
		GUILayout.Label("Preview (" + selectedController.bulletContainer.Count.ToString() + " Bullets)");

		/*
		GUILayout.BeginVertical();
		selectedController.maxBulletCount = EditorGUILayout.IntField("Max Num Bullets", selectedController.maxBulletCount);
		GUILayout.EndVertical();
		*/

		GUILayout.BeginHorizontal();
		if(selectedController.currentAttackIndex != -1) {
			if(GUILayout.Button("Stop")) {
				selectedController.StartAttack(-1);
			}
		} else {
			if(GUILayout.Button("Play")) {
				selectedController.StartAttack(0);
			}
		}
		selectedController.paused = (GUILayout.Toggle(selectedController.paused, "Pause", "button"));

		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}
	
	public override void OnInspectorGUI() {
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Danmaku Editor")) {
			DMKDanmakuEditor.Create();
		}
		GUILayout.EndHorizontal();

		EditorGUILayout.Separator();
		this.PlayerGUI();
	}

};
