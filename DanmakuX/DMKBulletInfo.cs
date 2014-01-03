using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public void CopyFrom(DMKCurveProperty p) {
		value 		= p.value;
		useCurve 	= p.useCurve;
		curve 		= p.curve;
	}
};

[System.Serializable]
public class DMKBulletInfoInternal {
	public float  direction = 0;
	public int    damage = 1;
	public Color  bulletColor = Color.white;
	public Sprite bulletSprite;
	public bool   died = false;

	public Vector2 scale =  new Vector2(1, 1);

	public int maxLifetime = 0;
	public int startFrame;
	public Rect collisionRect = new Rect();

	public bool			  useScaleCurve = false;
	public AnimationCurve scaleCurveX;
	public AnimationCurve scaleCurveY;

	public DMKCurveProperty speed = new DMKCurveProperty(5);
	public DMKCurveProperty accel = new DMKCurveProperty(0);
	public DMKCurveProperty angularAccel = new DMKCurveProperty(0);

	public DMKBulletInfoInternal() {
		scaleCurveX = DMKUtil.NewCurve(1, 1);
		scaleCurveY = DMKUtil.NewCurve(1, 1);
	}

	public void CopyFrom(DMKBulletInfoInternal prototype) {
		this.speed.CopyFrom(prototype.speed);
		this.accel.CopyFrom(prototype.accel);
		this.angularAccel.CopyFrom(prototype.angularAccel);
		this.angularAccel.value =  prototype.angularAccel.value * Mathf.Deg2Rad;

		this.bulletSprite = prototype.bulletSprite;
		this.damage = prototype.damage;
		this.speed = prototype.speed;
		this.accel = prototype.accel;
		this.bulletColor = prototype.bulletColor;
		this.died = false;

		this.useScaleCurve = prototype.useScaleCurve;
		this.scaleCurveX    = prototype.scaleCurveX;
		this.scaleCurveY    = prototype.scaleCurveY;

		this.collisionRect = prototype.collisionRect;
		this.maxLifetime = prototype.maxLifetime;
	}
};

public class DMKBulletInfo: MonoBehaviour {

	public DMKBulletInfoInternal bulletInfo = new DMKBulletInfoInternal();

};