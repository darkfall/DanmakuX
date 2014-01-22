using UnityEngine;

namespace danmakux {

	[System.Serializable]
	public class DMKNode: ScriptableObject {

		#region editor

		public virtual void OnEditorGUI(bool showHelp = false) {

		}

		public Rect editorWindowRect;
		public bool editorEnabled = true;

		#endregion

	};
		
}
