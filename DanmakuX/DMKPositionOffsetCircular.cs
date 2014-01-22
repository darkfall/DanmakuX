using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace danmakux {

	[Serializable]
	class DMKPositionOffsetCircular: DMKPositionOffsetInterface {
		public int 	circularTime = 60;
		public float radius = 0f;
		
		public override Vector2 Evaluate(float t) {
			float ctime = (float)circularTime * DMKSettings.instance.frameInterval;
			float ratio = t % ctime / ctime;
			return new Vector2(radius * Mathf.Cos(ratio * Mathf.PI * 2),
			                   radius * Mathf.Sin(ratio * Mathf.PI * 2));
		}
#if UNITY_EDITOR
		public override void OnEditorGUI(bool help) {
			circularTime = EditorGUILayout.IntField("Circular Time", circularTime);
			if(circularTime <= 0f)
				circularTime = 1;
			radius = EditorGUILayout.FloatField("Radius", radius);
		}
#endif
	}
		
}
