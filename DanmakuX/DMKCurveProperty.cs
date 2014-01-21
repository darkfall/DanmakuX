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

	float _val;

	public float get() {
		return _val;
	}

	public DMKCurveProperty(float v = 0) {
		value = v;
		curve = DMKUtil.NewCurve(v, v);

		_val = value;
	}

	public DMKCurveProperty(float v1, float v2) {
		value = v1;
		curve = DMKUtil.NewCurve(v1, v2);

		_val = value;
	}
	
	public float Update(float t, bool newRand = false) {
		switch(this.type) {
		case DMKCurvePropertyType.Constant:
			_val = value;
			break;
		case DMKCurvePropertyType.Curve:
			_val = curve.Evaluate(t);
			break;
		case DMKCurvePropertyType.RandomVal:
			if(newRand)
				_val = UnityEngine.Random.Range(value, valEnd);
			break;
		}
		return _val;
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
		n.Update(0, true);
		return n;
	}
};
