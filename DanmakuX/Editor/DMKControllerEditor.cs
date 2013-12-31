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

	[MenuItem("DanmakuX/Danmaku Editor", false, 1)]
	static void OpenAttackStyleEditor() {
		DMKDanmakuEditor.Create();
	}

	[MenuItem("DanmakuX/Settings", false, 13)]
	static void OpenSettingsEditor() {
		DMKSettingsEditor.Create();
	}

	[MenuItem("DanmakuX/Create Controller", false, 2)]
	static void CreateController() {
		try {
			GameObject obj = Selection.activeObject as GameObject;
			if(obj != null) {
				obj.AddComponent<DMKController>();
			}
			Selection.activeObject = null;
			Selection.activeObject = obj;
		} catch {
			Debug.Log("Please select the object you want to add DMKController to first");
		}
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
