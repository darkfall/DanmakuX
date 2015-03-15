using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace danmakux {

	[Serializable]
	class DMKPositionOffsetCurve: DMKPositionOffsetInterface {
		public AnimationCurve offsetX = DMKUtil.NewCurve(0, 0);
		public AnimationCurve offsetY = DMKUtil.NewCurve(0, 0);
		
		public override Vector2 Evaluate(float t) {
			return new Vector2(offsetX.Evaluate(t),
			                   offsetY.Evaluate(t));
		}
#if UNITY_EDITOR
		public override void OnEditorGUI(bool help) {
			offsetX	= EditorGUILayout.CurveField("X", offsetX);
			offsetY	= EditorGUILayout.CurveField("Y", offsetY);
		}
#endif
	}

	
}
 