using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DMKBulletInfoInternal {
	public float  direction = 0;
	public int    damage = 1;
	public Color  bulletColor = Color.white;
	public Sprite bulletSprite;
	public bool   died = false;

	public Vector2 scale =  new Vector2(1, 1);

	public int maxLifetime = 0;
	public int lifeFrame = 0;
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
		this.bulletSprite = prototype.bulletSprite;
		this.damage = prototype.damage;
		this.speed = DMKCurveProperty.CopyFrom(prototype.speed);
		this.accel = DMKCurveProperty.CopyFrom(prototype.accel);
		this.angularAccel = DMKCurveProperty.CopyFrom(prototype.angularAccel);
		this.bulletColor = prototype.bulletColor;
		this.died = false;

		this.useScaleCurve = prototype.useScaleCurve;
		this.scaleCurveX    = prototype.scaleCurveX;
		this.scaleCurveY    = prototype.scaleCurveY;

		this.collisionRect = prototype.collisionRect;
		this.maxLifetime = prototype.maxLifetime;
		this.lifeFrame = 0;
	}
};

public class DMKBulletInfo: MonoBehaviour {

	public DMKBulletInfoInternal bulletInfo = new DMKBulletInfoInternal();
	public DMKBulletEmitter parentEmitter = null;


};