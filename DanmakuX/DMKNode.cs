using UnityEngine;

[System.Serializable]
public class DMKNode: ScriptableObject {

	#region editor

	public virtual void OnEditorGUI(bool showHelp = false) {

	}

	public Rect editorWindowRect;

	#endregion

};