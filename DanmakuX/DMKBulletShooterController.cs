using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[Serializable]
public class DMKBulletShooterController: ScriptableObject {
	public DMKController parentController = null;
	public DMKBulletInfo bulletInfo = new DMKBulletInfo ();

	public GameObject	bulletContainer;
	public string 		tag;
	public string 		identifier;
	public int 			simulationCount = 1;
	public bool			followParentDirection = false;

	public DMKCurveProperty emissionCooldown = new DMKCurveProperty (30);
	public int emissionLength = 0;
	public int interval = 0;
	public int startFrame = 0;
	public int overallLength = 0;

	public GameObject gameObject;

	[SerializeField]
	public DMKPositionOffset positionOffset = new DMKPositionOffset ();

	[SerializeField]
	public DMKBulletShooter shooter = new DMKBulletShooter();

	[SerializeField]
	public DMKSubBulletShooterController subController = null;


	public List<DMKBulletInfo> bullets;

	bool  _enabled;
	public bool	enabled { 
		get { return _enabled; }
		set { 
			if (_enabled != value)
				DMKInit ();
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
	
	public void Start ()
	{
		_currentCooldown = 0;
		_currentInterval = 0;
		_prevFrame = 0;
		_internalFrame = 0;
	}
	
	public void Update ()
	{
		
	}
	
	public override string ToString ()
	{
		return this.identifier;
	}
	
	public virtual void DMKUpdateFrame (int currentFrame)
	{
		_currentFrame = currentFrame;
		
		if (!enabled || 
		    currentFrame < startFrame)
			return;
		if(this.Ended) {
			this.enabled = false;
			return;
		}
		
		if (_currentInterval != 0) {
			--_currentInterval;
			if (_currentInterval == 0)
				_prevFrame = currentFrame;
		} else {
			if (this.emissionLength == 0 || currentFrame - _prevFrame < this.emissionLength) {
				if (_currentCooldown > 0) {
					--_currentCooldown;
				}
				if (_currentCooldown == 0) {
					for (int i=0; i<this.simulationCount; ++i) {
						shooter.OnShoot(currentFrame);
						if (i == 0)
							_internalFrame += 1;
					}
					
					_currentCooldown = (int)Mathf.Clamp (this.emissionCooldown.Update ((float)_currentFrame / 60f), 0, 9999);
				}
			} else {
				_prevFrame = currentFrame;
				_currentInterval = interval;
				if (interval == 0)
					enabled = false;
			}
		}

		if(subController != null)
			subController.DMKUpdateFrame(currentFrame);
	}
	
	public virtual void DMKInit ()
	{
		_currentCooldown = _currentFrame = _currentInterval = _prevFrame = _internalFrame = 0;

		shooter.DMKInit();
		shooter.parentController = this;

		if(subController)
			subController.DMKInit();
	}

	public string DMKName() {
		return shooter.DMKName();
	}

	public string DMKSummary() {
		return shooter.DMKSummary();
	}

	public virtual void CopyFrom (DMKBulletShooterController controller)
	{
		this.interval = controller.interval;
		this.emissionCooldown = controller.emissionCooldown;
		this.emissionLength = controller.emissionLength;
		this.positionOffset = controller.positionOffset;
		this.bulletContainer = controller.bulletContainer;
		this.gameObject = controller.gameObject;
		this.tag = controller.tag;

		if (controller.shooter != null) {
			this.shooter = (DMKBulletShooter)ScriptableObject.CreateInstance (controller.shooter.GetType ());
			this.shooter.parentController = this;
			this.shooter.CopyFrom (controller.shooter);
		} else
			this.shooter = null;
		
		this.bulletInfo.CopyFrom (controller.bulletInfo);
	}
	
	public void CreateBullet (Vector3 position, float direction, float speedMultiplier)
	{
		if (parentController.CanAddBullet ()) {
			GameObject bulletObj = new GameObject ();
			DMKBullet bullet = bulletObj.AddComponent<DMKBullet> ();
			
			bullet.bulletInfo.CopyFrom (this.bulletInfo, speedMultiplier);
			if(followParentDirection && this.gameObject) {
				direction += this.gameObject.transform.rotation.eulerAngles.z - 90;
			}

			bullet.bulletInfo.direction = direction * Mathf.Deg2Rad;
			SpriteRenderer renderer = bulletObj.AddComponent<SpriteRenderer> ();
			renderer.sprite = this.bulletInfo.bulletSprite;
			renderer.color = this.bulletInfo.bulletColor;
			renderer.sortingOrder = DMKSettings.instance.sortingOrder;
			renderer.sortingLayerID = DMKSettings.instance.sortingLayerIndex;
			
			if (bulletContainer != null)
				bullet.transform.parent = bulletContainer.transform;
			
			bullet.transform.localScale = new Vector3 (this.bulletInfo.scale.x, this.bulletInfo.scale.y, 1);
			bullet.transform.rotation = Quaternion.AngleAxis (direction + 90, Vector3.forward);
			
			float ctime = (_currentFrame + _internalFrame) * DMKSettings.instance.frameInterval;
			Vector2 offset = this.positionOffset.Evaluate (ctime);
			position.x += offset.x;
			position.y += offset.y;
			if (positionOffset.type != DMKPositionOffsetType.Absolute && this.gameObject != null)
				position += this.gameObject.transform.position;
			bullet.transform.position = position;
			
			bullet.tag = this.tag;
			bullet.parentEmitter = this;
			
			parentController.bulletContainer.Add (bullet);

			if(subController != null)
				subController.OnShootBullet(bullet);
		}
	}

	#region editor
	
	public bool editorExpanded;
	public bool editorEnabled = true;
	public bool editorBulletInfoExpanded = true;
	public bool editorShooterInfoExpanded = true;
	public bool editorModifierExpanded = true;
	public Rect editorWindowRect;

	public void OnEditorGUI (bool showHelp = false)
	{
		this.shooter.OnEditorGUI();
	}
	
	#endregion
	
	
};