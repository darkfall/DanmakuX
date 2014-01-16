using UnityEngine;
using UnityEditor;

class DMKMenu {

	[MenuItem("DanmakuX/Danmaku Editor", false, 1)]
	static void OpenAttackStyleEditor() {
		DMKDanmakuEditor.Create();
	}

	[MenuItem("DanmakuX/Danmaku EditorX", false, 1)]
	static void OpenAttackStyleEditorX() {
		DMKDanmakuEditorX.Create();
	}
	
	[MenuItem("DanmakuX/Settings", false, 13)]
	static void OpenSettingsEditor() {
		DMKSettingsEditor.Create();
	}
	
	[MenuItem("DanmakuX/Add Controller to Selected Object", false, 2)]
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


}