using UnityEngine;
using UnityEditor;

namespace danmakux {

	class DMKMenu {

		[MenuItem("DanmakuX/Danmaku Editor", false, 1)]
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
					if(DMKDanmakuEditorX.IsOpen) {
						DMKDanmakuEditorX.instance.OnSelectionChange();
					}
				}
				Selection.activeObject = null;
				Selection.activeObject = obj;
			} catch {
				Debug.Log("Please select the object you want to add Controller to first");
			}
		}


	}

}
