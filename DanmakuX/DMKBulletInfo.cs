using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DMKBulletInfoInternal {
	public float  speed = 3;
	public float  accel = 0;
	public float  direction = 0;
	public float  angularAccel = 0;
	public int    damage = 1;
	public Color  bulletColor = Color.white;
	public Sprite bulletSprite;
	public bool   died = false;

	public Vector2 scale =  new Vector2(1, 1);

	public int startFrame;

	public bool   		  useSpeedCurve = false;
	public AnimationCurve speedCurve;

	public bool  		  useAccelCurve = false;
	public AnimationCurve accelCurve;

	public bool			  useScaleCurve = false;
	public AnimationCurve scaleCurveX;
	public AnimationCurve scaleCurveY;

	public bool			  useAngularAccelCurve = false;
	public AnimationCurve angularAccelCurve;

	AnimationCurve NewCurve(float v1, float v2) {
		AnimationCurve curve = new AnimationCurve();
		curve.AddKey(new Keyframe(0, v1));
		curve.AddKey(new Keyframe(1, v2));
		
		return curve;
	}

	public DMKBulletInfoInternal() {
		speedCurve = NewCurve(1, 1);
		accelCurve = NewCurve(0, 0);
		scaleCurveX = NewCurve(1, 1);
		scaleCurveY = NewCurve(1, 1);
		angularAccelCurve = NewCurve(0, 0);
	}
};

public class DMKBulletInfo: MonoBehaviour {

	public DMKBulletInfoInternal bulletInfo = new DMKBulletInfoInternal();

};