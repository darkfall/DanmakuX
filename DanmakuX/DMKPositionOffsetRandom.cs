using UnityEngine;
using UnityEditor;
using System;

namespace danmakux {

	[Serializable]
	class DMKPositionOffsetRandom: DMKPositionOffsetInterface {
		public Vector2 offsetStart;
		public Vector2 offsetEnd;
		
		public override Vector2 Evaluate(float t) {
			return new Vector2(UnityEngine.Random.Range(offsetStart.x, offsetEnd.x),
			                   UnityEngine.Random.Range(offsetStart.y, offsetEnd.y));
		}
		
		public override void OnEditorGUI(bool help) {
			offsetStart = EditorGUILayout.Vector2Field("", offsetStart);
			offsetEnd = EditorGUILayout.Vector2Field("", offsetEnd);
		}
	}


	
}
