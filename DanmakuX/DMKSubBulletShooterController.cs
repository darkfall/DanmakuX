using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class DMKSubBulletShooterController: DMKNode {

	public DMKBulletShooterController internalController;
	public List<BulletInfo> trackingBullets = new List<BulletInfo>();

	public int emissionCooldown { get { return (int)internalController.emissionCooldown.value; } }
	public int emissionLength { get { return internalController.emissionLength; } }
	public int interval { get { return internalController.interval; } }
	public int startFrame { get { return internalController.startFrame; } }
	public int overallLength { get { return internalController.overallLength; } }

	public class BulletInfo {
		public int currentCooldown;
		public int currentInterval;
		public int prevFrame;
		public int currentFrame;
		
		public DMKSubBulletShooterController parent;
		public WeakReference bulletRef;
		
		public BulletInfo(DMKSubBulletShooterController p, DMKBullet b) {
			currentCooldown = currentInterval = prevFrame = currentFrame = 0;
			this.parent = p;
			this.bulletRef = new WeakReference(b);
		}
		
		public void UpdateFrame(int f) {
			currentFrame++;
			if(currentFrame < parent.startFrame)
				return;

			if(currentInterval != 0) {
				--currentInterval;
				if(currentInterval == 0)
					prevFrame = currentFrame;
			} else {
				if(parent.emissionLength == 0 || currentFrame - prevFrame < parent.emissionLength) {
					if(currentCooldown != 0) {
						--currentCooldown;
					}
					if(currentCooldown == 0) {
						parent.OnShoot(this);
						
						currentCooldown = parent.emissionCooldown;
					}
				} else {
					prevFrame = currentFrame;
					currentInterval = parent.interval;
				}
			}
		}
	};

	public DMKSubBulletShooterController(DMKBulletShooterController internalController) {
		this.internalController = internalController;
	}

	public void DMKInit() {
		this.trackingBullets = new List<BulletInfo>();

		if(this.internalController != null)
			this.internalController.DMKInit();
	}

	public void DMKUpdateFrame(int frame) {
		foreach(BulletInfo info in this.trackingBullets) {
			info.UpdateFrame(frame);
		}

		this.trackingBullets.RemoveAll(o => {
			return !o.bulletRef.IsAlive || 
				((o.currentFrame - this.startFrame) > this.internalController.overallLength);
		});

		if(this.internalController.subController != null)
			this.internalController.subController.DMKUpdateFrame(frame);
	}

	public string DMKName() {
		return this.internalController.DMKName();
	}

	public void OnShoot(BulletInfo info) {
		if(info.bulletRef.IsAlive) {
			DMKBullet bullet = (info.bulletRef.Target as DMKBullet);
			if(bullet != null) {
				internalController.gameObject = bullet.gameObject;
				internalController.shooter.OnShoot(info.currentFrame);
			}
		}
	}

	public void OnShootBullet(DMKBullet bullet) {
		if(trackingBullets == null)
			trackingBullets = new List<BulletInfo>();
		trackingBullets.Add(new BulletInfo(this, bullet));
	}

};
