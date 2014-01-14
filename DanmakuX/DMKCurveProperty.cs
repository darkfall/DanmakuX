using UnityEngine;
using System;

[System.Serializable]
public class DMKCurveProperty {
	public float 			value = 0f;
	public bool  			useCurve = false;
	public AnimationCurve 	curve = null;
	
	public DMKCurveProperty(float v = 0) {
		value = v;
		curve = DMKUtil.NewCurve(0, 0);
	}

	public DMKCurveProperty(float v1, float v2) {
		value = 0f;
		curve = DMKUtil.NewCurve(v1, v2);
	}
	
	public float Update(float t) {
		if(useCurve)
			value = curve.Evaluate(t);
		return value;
	}

	public static DMKCurveProperty Copy(DMKCurveProperty p) {
		DMKCurveProperty n = new DMKCurveProperty();
		n.value		= p.value;
		n.useCurve	= p.useCurve;
		n.curve		= p.curve;
		return n;
	}

	public static DMKCurveProperty Copy(DMKCurveProperty p, float x) {
		DMKCurveProperty n = new DMKCurveProperty();
		n.value = p.value * x;
		n.useCurve = p.useCurve;
		n.curve = DMKUtil.CopyCurve(p.curve, v => {
			return v * x;
		});
		return n;
	}
};
