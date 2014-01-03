using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(DMKSettings))]
public class DMKSettingsInspector: Editor {
		
	public override void OnInspectorGUI() {
		DMKSettingsEditor.SettingsGUI();
	}

}

