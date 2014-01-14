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
	public string 		identifier;
	public int 			simulationCount = 1;

	public DMKCurveProperty emissionCooldown = new DMKCurveProperty(30);
	public int emissionLength 	= 0;
	public int interval 		= 0;
	public int startFrame 		= 0;
	public int overallLength 	= 0;

	public GameObject gameObject;

	[SerializeField]
	public DMKPositionOffset positionOffset = new DMKPositionOffset();

	[SerializeField]
	public DMKDeathBulletEmitter deathEmitter = null;

	[SerializeField]
	public DMKEmitterModifier emitterModifier = null;

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
			return (overallLength != 0 && (_currentFrame - startFrame) >= overallLength);
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
				if(_currentCooldown > 0) {
					--_currentCooldown;
				}
				if(_currentCooldown == 0) {
					for(int i=0; i<this.simulationCount; ++i) {
						this.DMKShoot(currentFrame);
						if(i != 0)
							_internalFrame += 1;
					}
					
					_currentCooldown = (int)Mathf.Clamp(this.emissionCooldown.Update((float)_currentFrame / 60f), 0, 9999);
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

	public virtual void DMKInit() {
		_currentCooldown = _currentFrame = _currentInterval = _prevFrame = _internalFrame = 0;
		if(deathEmitter != null)
			deathEmitter.DMKInit();
		if(emitterModifier != null)
			emitterModifier.DMKInit();
	}

	public virtual void DMKShoot(int frame) {

	}

	public virtual string DMKName() {
		return "DMKBulletEmitter";
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

		if(emitter.emitterModifier != null) {
			this.emitterModifier = (DMKEmitterModifier)ScriptableObject.CreateInstance(emitter.emitterModifier.GetType());
			this.emitterModifier.CopyFrom(emitter.emitterModifier);
		}
		else
			this.emitterModifier = null;

		this.bulletInfo.CopyFrom(emitter.bulletInfo);
	}

	public DMKBulletInfo CreateBullet(Vector3 position, float direction, float speedMultiplier) {
		if(parentController.CanAddBullet()) {
			GameObject bulletObj = new GameObject();
			DMKBulletInfo bullet = bulletObj.AddComponent<DMKBulletInfo>();

			bullet.bulletInfo.CopyFrom(this.bulletInfo, speedMultiplier);
			bullet.bulletInfo.direction = direction * Mathf.Deg2Rad;

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

	public void ShootBullet(Vector3 position, float direction, float speedMultiplier = 1f) {
		if(emitterModifier != null) {
			emitterModifier.OnShootBullet(position, direction, speedMultiplier);
		} 
		else
			this.CreateBullet(position, direction, speedMultiplier);
	}
	
	public void ShootBulletTo(Vector3 position, GameObject target, float speedMultiplier = 1f) {
		Vector3 targetPos = target.transform.position;
		Vector3 dis = targetPos - position ;
		float angle = (float)(Math.Atan2(dis.y, dis.x) * Mathf.Rad2Deg);
		this.ShootBullet(position, angle, speedMultiplier);
	}

	public void AddModifier(DMKEmitterModifier modifier) {
		DMKEmitterModifier m = this.emitterModifier;
		if(m == null)
			this.emitterModifier = modifier;
		else {
			while(m.next != null)
				m = m.next;
			m.next = modifier;
		}
		modifier.parentEmitter = this;
	}

	public void RemoveModifier(DMKEmitterModifier modifier) {
		DMKEmitterModifier m = this.emitterModifier;
		DMKEmitterModifier p = null;
		while(m != null) {
			if(m == modifier) {
				if(p == null) {
					this.emitterModifier = m.next;
				}
				else {
					p.next = m.next;
				}
				break;
			}
			p = m;
			m = m.next;
		}
	}


#region editor
	
	public bool editorExpanded;
	public bool editorEnabled = true;
	public bool editorBulletInfoExpanded = true;
	public bool editorEmitterInfoExpanded = true;
	public bool editorModifierExpanded = true;
	
	public virtual void OnEditorGUI() {
	}
	
#endregion


};
