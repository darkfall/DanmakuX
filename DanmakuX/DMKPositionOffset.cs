using System;
using UnityEngine;
using UnityEditor;

public enum DMKPositionOffsetType {
	Circular,
	Curve,
	Relative,
	Absolute
}

[Serializable]
public class DMKPositionOffsetInterface: ScriptableObject {
	public virtual Vector2 Evaluate(float t) { return new Vector2(0, 0); }
	public virtual void OnEditorGUI(bool help) {} 
}

[Serializable]
public class DMKPositionOffset {

	[SerializeField]
	DMKPositionOffsetType _type;
	public DMKPositionOffsetType type {
		set {
			if(_type != value || evaluator == null) {
				evaluator = (DMKPositionOffsetInterface) ScriptableObject.CreateInstance("DMKPositionOffset" + value.ToString());
			}
			_type = value;
		}
		get {
			return _type;
		}
	}
	[SerializeField]
	public DMKPositionOffsetInterface evaluator;

	public DMKPositionOffset() {
		this._type = DMKPositionOffsetType.Relative;
	}

	public Vector2 Evaluate(float t) {
		return evaluator.Evaluate(t);
	}

	public void OnEditorGUI(bool help) {
		evaluator.OnEditorGUI(help);
	}

};
