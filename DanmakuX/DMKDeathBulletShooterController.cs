using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DMKDeathBulletShooterController: DMKBulletShooterController {

	public int lifeFrame = 1;
	public Vector2 objectOffset;

	public class GameObjectInfo {
		public int startFrame;
		public int currentCooldown;
		public int currentInterval;
		public int prevFrame;
		public int currentFrame;

		public bool valid;
		public Vector2 objectOffset;
		public DMKDeathBulletShooterController parent;

		public GameObjectInfo(Vector2 offset, DMKDeathBulletShooterController p) {
			startFrame = currentCooldown = currentInterval = prevFrame = currentFrame = 0;
			valid = true;
			objectOffset = offset;
			parent = p;
		}

		public void UpdateFrame(int f) {
			int currentFrame = f - startFrame;
			if(currentFrame > parent.lifeFrame)
				valid = false;

			if(!valid)
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
						// reassign gameobject
						parent.gameObject = null;
						parent.objectOffset = this.objectOffset;
						parent.OnShoot(currentFrame);
					
						currentCooldown = (int)Mathf.Clamp(parent.emissionCooldown.value, 0, 9999);
					}
				} else {
					prevFrame = currentFrame;
					currentInterval = parent.interval;

					if(parent.interval == 0)
						this.valid = false;
				}
			}
		}
	};

	public List<GameObjectInfo> trackingObjects = new List<GameObjectInfo>();
	
	public override void DMKInit() {
		base.DMKInit();
		this.trackingObjects.Clear();
	}

	public override void DMKUpdateFrame(int currentFrame) {
		_currentFrame = currentFrame;
		this.emissionCooldown.Update((float)currentFrame / 60f);

		foreach(GameObjectInfo info in trackingObjects)
			info.UpdateFrame(currentFrame);

		trackingObjects.RemoveAll(o => {
			return !o.valid;
		});
	}

	public void OnShoot(int frame) {
		this.shooter.OnShoot(frame);
	}

	// override DMKShoot and using objectOffset here

	///
	public override void CopyFrom(DMKBulletShooterController emitter)
	{
		Type t = emitter.GetType();
		if(t == typeof(DMKDeathBulletShooterController)) {
			DMKDeathBulletShooterController de = emitter as DMKDeathBulletShooterController;
			this.lifeFrame = de.lifeFrame;
			this.objectOffset = de.objectOffset;
		}
		base.CopyFrom (emitter);
	}

	public void AddTrackObject(GameObject obj) {
		GameObjectInfo info = new GameObjectInfo((Vector2)obj.transform.position, this);
		info.startFrame = _currentFrame;
		trackingObjects.Add (info);
	}
};
