using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class DMKBulletEmitter: ScriptableObject {

	public DMKController parentController = null;
	public DMKBulletInfoInternal bulletInfo = new DMKBulletInfoInternal();

	public GameObject	bulletContainer;

	public string 		tag;
	public int 			cooldown = 30;

	public int 			length = 0;
	public int 			interval = 0;

	public GameObject	gameObject;

	public Vector3  	positionOffset;


	bool  _enabled;
	public bool	enabled { 
		get { return _enabled; }
		set { 
			_enabled = value;
			if(_enabled)
				this.DMKInit();
		}
	}

	int _currentCooldown = 0;
	int _currentInterval = 0;
	int _prevFrame = 0;
	int _currentFrame = 0;

	public List<DMKBulletInfo> bullets;
		
	public void Start() {
		_currentCooldown = 0;
		_currentInterval = 0;
		_prevFrame = 0;
	}

	public void Update() {

	}

	public virtual void DMKUpdateFrame(int currentFrame) {
		if(!enabled)
			return;

		_currentFrame = currentFrame;
		if(_currentInterval != 0) {
			--_currentInterval;
			if(_currentInterval == 0)
				_prevFrame = currentFrame;
		} else {
			if(length == 0 || currentFrame - _prevFrame < length) {
				if(_currentCooldown != 0) {
					--_currentCooldown;
				} else {
					this.DMKShoot(currentFrame);
					
					_currentCooldown = this.cooldown;
				}
			} else {
				_prevFrame = currentFrame;
				_currentInterval = interval;
			}
		}
	}

	public virtual void DMKShoot(int frame) {

	}

	public virtual void DMKInit() {
		_currentCooldown = _currentFrame = _currentInterval = _prevFrame = 0;
	}

	public virtual string DMKName() {
		return "DMK Emitter";
	}

	void CopyInfo(DMKBulletInfo infoComponent, DMKBulletInfoInternal prototype, float dir) {
		infoComponent.bulletInfo.speed = prototype.speed;
		infoComponent.bulletInfo.accel = prototype.accel;
		infoComponent.bulletInfo.angularAccel = prototype.angularAccel * Mathf.Deg2Rad;
		infoComponent.bulletInfo.bulletSprite = prototype.bulletSprite;
		infoComponent.bulletInfo.damage = prototype.damage;
		infoComponent.bulletInfo.direction = dir * Mathf.Deg2Rad;
		infoComponent.bulletInfo.speed = prototype.speed;
		infoComponent.bulletInfo.accel = prototype.accel;
		infoComponent.bulletInfo.bulletColor = prototype.bulletColor;
		infoComponent.bulletInfo.died = false;

		infoComponent.bulletInfo.useAccelCurve = prototype.useAccelCurve;
		infoComponent.bulletInfo.useScaleCurve = prototype.useScaleCurve;
		infoComponent.bulletInfo.useSpeedCurve = prototype.useSpeedCurve;
		infoComponent.bulletInfo.speedCurve	   = prototype.speedCurve;
		infoComponent.bulletInfo.accelCurve	   = prototype.accelCurve;
		infoComponent.bulletInfo.scaleCurveX    = prototype.scaleCurveX;
		infoComponent.bulletInfo.scaleCurveY    = prototype.scaleCurveY;
		infoComponent.bulletInfo.useAngularAccelCurve = prototype.useAngularAccelCurve;
		infoComponent.bulletInfo.angularAccelCurve = prototype.angularAccelCurve;

		infoComponent.bulletInfo.startFrame = _currentFrame;
	}

	DMKBulletInfo _CreateBullet(Vector3 position, float direction) {
		if(parentController.CanAddBullet()) {
			GameObject bulletObj = new GameObject();
			DMKBulletInfo bullet = bulletObj.AddComponent<DMKBulletInfo>();
			CopyInfo(bullet, this.bulletInfo, direction);
			
			SpriteRenderer renderer = bulletObj.AddComponent<SpriteRenderer>();
			renderer.sprite = this.bulletInfo.bulletSprite;
			renderer.color = this.bulletInfo.bulletColor;
			renderer.sortingOrder = 1;
			
			//bulletObj.AddComponent<BoxCollider2D>().isTrigger = true;

			bullet.transform.parent = bulletContainer.transform;
			
			bullet.transform.localScale = new Vector3(this.bulletInfo.scale.x, this.bulletInfo.scale.y, 1);
			bullet.transform.rotation = Quaternion.AngleAxis(direction + 90, Vector3.forward);
			bullet.transform.position = position + this.positionOffset;
			
			bullet.tag = this.tag;

			parentController.bulletContainer.Add(bullet);
			return bullet;
		}
		return null;
	}
	
	public DMKBulletInfo ShootBullet(Vector3 position, float direction) {
		return this._CreateBullet(position, direction);
	}
	
	public DMKBulletInfo ShootBulletTo(Vector3 position, GameObject target) {
		Vector3 targetPos = target.transform.position;
		Vector3 dis = targetPos - position ;
		float angle = (float)(Math.Atan2(dis.y, dis.x) * Mathf.Rad2Deg);
		return this._CreateBullet(position, angle);
	}

#region editor
	
	public bool editorExpanded;
	public bool editorEnabled = true;
	public bool editorBulletInfoExpanded = true;
	public bool editorEmitterInfoExpanded = true;
	
	public virtual void OnEditorGUI() {
	}
	
#endregion


};
