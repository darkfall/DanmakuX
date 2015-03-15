using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace danmakux {

	[Serializable]
	class DMKPositionOffsetAbsolute: DMKPositionOffsetInterface {
		public Vector2 offset = Vector2.zero;
		
		public override Vector2 Evaluate(float t) {
			return offset;
		}
#if UNITY_EDITOR		
		public override void OnEditorGUI(bool help) {
			offset = EditorGUILayout.Vector2Field("", offset);
		}
#endif
	}

	
}
