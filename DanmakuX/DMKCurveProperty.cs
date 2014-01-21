using UnityEngine;
using System;

public enum DMKCurvePropertyType {
	Curve,
	Constant,
	RandomVal,
	// to do with waves
};

[System.Serializable]
public class DMKCurveProperty {
	public float 					value = 0f;
	public float 					valEnd = 0f;
	public DMKCurvePropertyType		type = DMKCurvePropertyType.Constant;
	public AnimationCurve 			curve = null;

	public float currentVal;

	public float get() {
		return currentVal;
	}

	public DMKCurveProperty(float v = 0) {
		value = v;
		curve = DMKUtil.NewCurve(v, v);

		currentVal = value;
	}

	public DMKCurveProperty(float v1, float v2) {
		value = v1;
		curve = DMKUtil.NewCurve(v1, v2);

		currentVal = value;
	}
	
	public float Update(float t, bool newRand = false) {
		switch(this.type) {
		case DMKCurvePropertyType.Constant:
			currentVal = value;
			break;
		case DMKCurvePropertyType.Curve:
			currentVal = curve.Evaluate(t);
			break;
		case DMKCurvePropertyType.RandomVal:
			if(newRand)
				currentVal = UnityEngine.Random.Range(value, valEnd);
			break;
		}
		return currentVal;
	}

	public static DMKCurveProperty Copy(DMKCurveProperty p) {
		DMKCurveProperty n = new DMKCurveProperty();
		n.value		= p.value;
		n.type		= p.type;
		n.curve		= DMKUtil.CopyCurve(p.curve);
		n.valEnd 	= p.valEnd;
		n.Update(0, true);
		return n;
	}

	public static DMKCurveProperty Copy(DMKCurveProperty p, float x) {
		DMKCurveProperty n = new DMKCurveProperty();
		n.value 	= p.value * x;
		n.valEnd	= p.valEnd * x;
		n.type 		= p.type;
		n.curve = DMKUtil.CopyCurve(p.curve, v => {
			return v * x;
		});
		n.currentVal = p.currentVal * x;
		return n;
	}
};
