using UnityEditor;
using UnityEngine;
using System;


[Serializable]
class DMKPositionOffsetCurve: DMKPositionOffsetInterface {
	public AnimationCurve offsetX = DMKUtil.NewCurve(0, 0);
	public AnimationCurve offsetY = DMKUtil.NewCurve(0, 0);
	
	public override Vector2 Evaluate(float t) {
		return new Vector2(offsetX.Evaluate(t),
		                   offsetY.Evaluate(t));
	}
	
	public override void OnEditorGUI(bool help) {
		offsetX	= EditorGUILayout.CurveField("X", offsetX);
		offsetY	= EditorGUILayout.CurveField("Y", offsetY);
	}
}

