using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class DMKBulletEmitterMixinInterface: ScriptableObject {
	public DMKBulletEmitterMixinInterface next = null;
	public DMKBulletEmitter				  parentEmitter = null;

	public virtual void DMKUpdateFrame(int currentFrame) {
		if(next != null)
			next.DMKUpdateFrame(currentFrame);
	}
	
	public virtual void DMKShoot(int frame) {
		if(next != null)
			next.DMKShoot(frame);
	}
	
	public virtual void DMKInit() {
		if(next != null)
			next.DMKInit();
	}
	
	public virtual string DMKName() {
		return "DMK Bullet Emitter";
	}
	
	public virtual string DMKSummary() {
		return "";
	}
};

[Serializable]
public class DMKBulletEmitter: ScriptableObject {

	public DMKController parentController = null;
	public DMKBulletInfoInternal bulletInfo = new DMKBulletInfoInternal();

	public GameObject	bulletContainer;
	public string 		tag;
	public string 		identifier;
	public int 			simulationCount = 1;

	[SerializeField]
	int _emissionCooldown = 30;
	[SerializeField]
	int _emissionLength = 0;
	[SerializeField]
	int _interval = 0;
	[SerializeField]
	int _startFrame = 0;
	[SerializeField]
	int _overallLength = 0;

	public int startFrame {
		get { return _startFrame; }
		set {
			if(value != _startFrame) 
				DMKInit();
			_startFrame = value;
		}
	}

	public int emissionCooldown {
		get { return _emissionCooldown; }
		set {
			if(value != _emissionCooldown)
				DMKInit();
			_emissionCooldown = value;
		}
	}

	public int emissionLength {
		get { return _emissionLength; }
		set {
			if(value != _emissionLength)
				DMKInit();
			_emissionLength = value;
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

	public int overallLength {
		get { return _overallLength; }
		set {
			if(value != _overallLength) 
				DMKInit();
			_overallLength = value;
		}
	}

	public GameObject	gameObject;

	[SerializeField]
	public DMKPositionOffset positionOffset = new DMKPositionOffset();

	[SerializeField]
	public DMKDeathBulletEmitter deathEmitter = null;

	[SerializeField]
	public DMKBulletEmitterMixinInterface emitter = null;

	public List<DMKBulletInfo> bullets;

	bool  _enabled;
	public bool	enabled { 
		get { return _enabled; }
		set { 
			if(_enabled != value)
				DMKInit();
			_enabled = value;
		}
	}

	public bool Ended {
		get {
			return (_overallLength != 0 && (_currentFrame - _startFrame) >= _overallLength);
		}
	}

	protected int _currentCooldown = 0;
 	protected int _currentInterval = 0;
	protected int _prevFrame = 0;
	protected int _currentFrame = 0;
	protected int _internalFrame = 0;

	public void Start() {
		_currentCooldown = 0;
		_currentInterval = 0;
		_prevFrame = 0;
		_internalFrame = 0;
	}

	public void Update() {

	}

	public override string ToString ()
	{
		return this.identifier;
	}

	public virtual void DMKUpdateFrame(int currentFrame) {
		_currentFrame = currentFrame;

		if(!enabled || 
		   currentFrame < startFrame ||
		   this.Ended)
			return;

		if(_currentInterval != 0) {
			--_currentInterval;
			if(_currentInterval == 0)
				_prevFrame = currentFrame;
		} else {
			if(this.emissionLength == 0 || currentFrame - _prevFrame < this.emissionLength) {
				if(_currentCooldown != 0) {
					--_currentCooldown;
				}
				if(_currentCooldown == 0) {
					for(int i=0; i<this.simulationCount; ++i) {
						this.DMKShoot(currentFrame);
						if(i != 0)
							_internalFrame += 1;
					}
					
					_currentCooldown = this.emissionCooldown;
				}
			} else {
				_prevFrame = currentFrame;
				_currentInterval = interval;
				if(interval == 0)
					enabled = false;
			}
		}

		if(deathEmitter != null)
			deathEmitter.DMKUpdateFrame(currentFrame);
	}

	public virtual void DMKShoot(int frame) {

	}

	public virtual void DMKInit() {
		_currentCooldown = _currentFrame = _currentInterval = _prevFrame = _internalFrame = 0;
		if(deathEmitter != null)
			deathEmitter.DMKInit();
	}

	public virtual string DMKName() {
		return "DMK Emitter";
	}

	public virtual string DMKSummary() {
		return "";
	}

	public virtual void CopyFrom(DMKBulletEmitter emitter) {
		this.interval 			= emitter.interval;
		this.emissionCooldown	= emitter.emissionCooldown;
		this.emissionLength		= emitter.emissionLength;
		this.positionOffset 	= emitter.positionOffset;
		this.bulletContainer 	= emitter.bulletContainer;
		this.gameObject 		= emitter.gameObject;
		this.tag 				= emitter.tag;
		if(emitter.deathEmitter != null) {
			this.deathEmitter = (DMKDeathBulletEmitter)ScriptableObject.CreateInstance(emitter.deathEmitter.GetType());
			this.deathEmitter.CopyFrom(emitter.deathEmitter);
		}
		else
			this.deathEmitter = null;

		this.bulletInfo.CopyFrom(emitter.bulletInfo);
	}

	DMKBulletInfo _CreateBullet(Vector3 position, float direction) {
		if(parentController.CanAddBullet()) {
			GameObject bulletObj = new GameObject();
			DMKBulletInfo bullet = bulletObj.AddComponent<DMKBulletInfo>();

			bullet.bulletInfo.CopyFrom(this.bulletInfo);
			bullet.bulletInfo.direction = direction * Mathf.Deg2Rad;
			bullet.bulletInfo.lifeFrame = 0;

			SpriteRenderer renderer = bulletObj.AddComponent<SpriteRenderer>();
			renderer.sprite = this.bulletInfo.bulletSprite;
			renderer.color = this.bulletInfo.bulletColor;
			renderer.sortingOrder = 1;

			if(bulletContainer != null)
				bullet.transform.parent = bulletContainer.transform;
			
			bullet.transform.localScale = new Vector3(this.bulletInfo.scale.x, this.bulletInfo.scale.y, 1);
			bullet.transform.rotation = Quaternion.AngleAxis(direction + 90, Vector3.forward);

			float ctime = (_currentFrame + _internalFrame) * DMKSettings.instance.frameInterval;
			Vector2 offset = this.positionOffset.Evaluate(ctime);
			position.x += offset.x;
			position.y += offset.y;
			if(positionOffset.type != DMKPositionOffsetType.Absolute && this.gameObject != null)
				position += this.gameObject.transform.position;
			bullet.transform.position = position;
			
			bullet.tag = this.tag;
			bullet.parentEmitter = this;

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
	public int  editorDeathSubEmitterIndex = -1;
	
	public virtual void OnEditorGUI() {
	}
	
#endregion


};
