using UnityEngine;
using UnityEditor;
using System;

namespace danmakux {

	[Serializable]
	class DMKPositionOffsetAbsolute: DMKPositionOffsetInterface {
		public Vector2 offset;
		
		public override Vector2 Evaluate(float t) {
			return offset;
		}
		
		public override void OnEditorGUI(bool help) {
			offset = EditorGUILayout.Vector2Field("", offset);
		}
	}

	
}
