using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DMKBulletInfo {
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
	
	public DMKCurveProperty speed = new DMKCurveProperty(5, 5);
	public DMKCurveProperty accel = new DMKCurveProperty(0);
	public DMKCurveProperty angularAccel = new DMKCurveProperty(0);
	
	public DMKBulletInfo() {
		scaleCurveX = DMKUtil.NewCurve(1, 1);
		scaleCurveY = DMKUtil.NewCurve(1, 1);
	}
	
	public void CopyFrom(DMKBulletInfo prototype, float speedMultiplier = 1f) {
		this.bulletSprite = prototype.bulletSprite;
		this.damage = prototype.damage;
		this.speed = DMKCurveProperty.Copy(prototype.speed, speedMultiplier);
		this.accel = DMKCurveProperty.Copy(prototype.accel);
		this.angularAccel = DMKCurveProperty.Copy(prototype.angularAccel);
		this.bulletColor = prototype.bulletColor;
		this.died = false;
		
		this.useScaleCurve	= prototype.useScaleCurve;
		this.scaleCurveX	= DMKUtil.CopyCurve(prototype.scaleCurveX);
		this.scaleCurveY    = DMKUtil.CopyCurve(prototype.scaleCurveY);
		
		this.collisionRect = prototype.collisionRect;
		this.maxLifetime = prototype.maxLifetime;
		this.lifeFrame = 0;
	}
};

