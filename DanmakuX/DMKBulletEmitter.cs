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

	int _cooldown = 30;
	int _length = 0;
	int _interval = 0;

	public int cooldown {
		get { return _cooldown; }
		set {
			if(value != _cooldown)
				DMKInit();
			_cooldown = value;
		}
	}

	public int length {
		get { return _length; }
		set {
			if(value != _length)
				DMKInit();
			_length = value;
		}
	}

	public int interval {
		get { return _interval; }
		set {
			if(value != _interval)
				DMKInit();
			_interval = value;
		}
	}

	public GameObject	gameObject;
	
	public bool				usePositionOffCurve;
	public Vector2	 		positionOffset;
	public AnimationCurve 	positionOffsetX;
	public AnimationCurve 	positionOffsetY;
	
	public List<DMKBulletInfo> bullets;

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
				if(interval == 0)
					enabled = false;
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

	public virtual void CopyFrom(DMKBulletEmitter emitter) {
		this.interval 			= emitter.interval;
		this.cooldown 			= emitter.cooldown;
		this.length   			= emitter.length;
		this.positionOffset 	= emitter.positionOffset;
		this.bulletContainer 	= emitter.bulletContainer;
		this.gameObject 		= emitter.gameObject;
		this.tag 				= emitter.tag;

		this.bulletInfo.CopyFrom(emitter.bulletInfo);
	}

	DMKBulletInfo _CreateBullet(Vector3 position, float direction) {
		if(parentController.CanAddBullet()) {
			GameObject bulletObj = new GameObject();
			DMKBulletInfo bullet = bulletObj.AddComponent<DMKBulletInfo>();
			bullet.bulletInfo.CopyFrom(this.bulletInfo);
			bullet.bulletInfo.direction = direction * Mathf.Deg2Rad;
			bullet.bulletInfo.startFrame = _currentFrame;

			SpriteRenderer renderer = bulletObj.AddComponent<SpriteRenderer>();
			renderer.sprite = this.bulletInfo.bulletSprite;
			renderer.color = this.bulletInfo.bulletColor;
			renderer.sortingOrder = 1;
			
			//bulletObj.AddComponent<BoxCollider2D>().isTrigger = true;

			if(bulletContainer != null)
				bullet.transform.parent = bulletContainer.transform;
			
			bullet.transform.localScale = new Vector3(this.bulletInfo.scale.x, this.bulletInfo.scale.y, 1);
			bullet.transform.rotation = Quaternion.AngleAxis(direction + 90, Vector3.forward);
			if(!this.usePositionOffCurve) {
				position.x += this.positionOffset.x;
				position.y += this.positionOffset.y;
			} else {
				float ctime = _currentFrame * DMKSettings.instance.frameInterval;
				position.x += this.positionOffsetX.Evaluate(ctime);
				position.y += this.positionOffsetY.Evaluate(ctime);
			}
			bullet.transform.position = position;
			
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
