using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(DMKSettings))]
public class DMKSettingsInspector: Editor {
		
	public override void OnInspectorGUI() {
		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal ("box");
		GUILayout.Label("DanmakuX ver " + DMKSettings.Version);
		GUILayout.EndHorizontal();

		if(GUILayout.Button("DanmakuX Settings")) {
			DMKSettingsEditor.Create();
		}
		GUILayout.EndVertical();
	}

}

