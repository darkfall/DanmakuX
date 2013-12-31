using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DMKDanmaku {
	[SerializeField]
	public string name;

	[SerializeField]
	public List<DMKBulletEmitter> emitters;
};

[Serializable]
public class DMKController: MonoBehaviour {

	[SerializeField]
	public List<DMKDanmaku> danmakus;
	public int currentAttackIndex = -1;
	
	public int maxBulletCount;
	public List<DMKBulletInfo> bulletContainer = new List<DMKBulletInfo>();

	public bool paused = false;

	// that's for STG!
	// we want a 60fps update because every value in DMK is related to frame
	double _prevTime;
	double _targetInterval;
	bool   _needInternalTimer;
	int    _currentFrame;

	public void Awake() {
		_needInternalTimer = DMKSettings.CheckNeedInternalTimer();
		_targetInterval = 1.0f / DMKSettings.targetFPS;

		if(Application.isEditor)
			_prevTime = EditorApplication.timeSinceStartup;
		else
			_prevTime = Time.timeSinceLevelLoad;

		_currentFrame = 0;
	}

	public void Start() {
		if(Application.isEditor)
			_prevTime = EditorApplication.timeSinceStartup;
		else
			_prevTime = Time.timeSinceLevelLoad;

		_currentFrame = 0;
		this.StartAttack(-1);
	}

	public void Update() {
		double currentTime;
		if(Application.isEditor)
			currentTime = EditorApplication.timeSinceStartup;
		else
			currentTime = Time.timeSinceLevelLoad;
		if(_needInternalTimer || Application.isEditor) {
			double delta = currentTime - _prevTime;
			if(delta < _targetInterval) {
				return;
			}
			double realDelta;
			if(_prevTime < currentTime - _targetInterval &&
			   _prevTime > currentTime - (_targetInterval * 2)) {
				realDelta = _targetInterval;
				_prevTime += _targetInterval;
			} else {
				realDelta = delta;
				_prevTime = currentTime;
			}

			this.DMKUpdate();
		} else {
			this.DMKUpdate();
		}
	}

	public bool CanAddBullet() {
		return maxBulletCount == 0 || bulletContainer.Count < maxBulletCount;
	}

	public void StartAttack(int index) {
		_targetInterval = 1.0f / DMKSettings.targetFPS;

		if(currentAttackIndex != -1) {
			foreach(DMKBulletEmitter emitter in this.danmakus[currentAttackIndex].emitters) {
				emitter.enabled = false;
				emitter.parentController = this;
			}
		}

		if(index < danmakus.Count)
			currentAttackIndex = index;
		else {
			currentAttackIndex = -1;
			Debug.LogError("DMKController::StartAttack: invalid index " + index.ToString());
		}
		if(index == -1) {
			foreach(DMKBulletInfo bullet in this.bulletContainer) {
				try {
					DestroyImmediate(bullet.gameObject);
				} catch {

				}
			}
			this.bulletContainer.Clear();
		} else {
			foreach(DMKBulletEmitter emitter in this.danmakus[currentAttackIndex].emitters) {
				if(Application.isEditor)
					emitter.enabled = true && emitter.editorEnabled;
				else
					emitter.enabled = true;
			}
		}
		_currentFrame = 0;
		paused = false;
	}

	void DMKUpdate() {
		if(!paused && enabled && currentAttackIndex != -1) {
			Vector3 pos = Camera.main.transform.position;
			float   orthoSize = Camera.main.orthographicSize;
			Rect  	cameraRect = new Rect(pos.x - orthoSize* Camera.main.aspect,
		                              	  pos.y - orthoSize,
		                              	  orthoSize * 2 * Camera.main.aspect,
		                              	  orthoSize * 2);
		
			List<DMKBulletInfo> diedBullets = new List<DMKBulletInfo>();
			foreach(DMKBulletInfo bullet in this.bulletContainer) {
				DMKBulletInfoInternal info = bullet.bulletInfo;
				if(!info.died) {
					Vector3 prevPos = bullet.gameObject.transform.position;
					float dist = info.speed * DMKSettings.unitPerPixel;
					if(dist == 0)
						dist = 1;
					bullet.gameObject.transform.position = new Vector3(prevPos.x + (float)(dist * Mathf.Cos (info.direction)),
					                                                   prevPos.y + (float)(dist * Mathf.Sin (info.direction)), 
					                                                   prevPos.z);

					float currentTime = (float)(_currentFrame - info.startFrame) / 60;
					if(info.useSpeedCurve) 
						info.speed = info.speedCurve.Evaluate(currentTime);
					if(info.useAccelCurve)
						info.accel = info.accelCurve.Evaluate(currentTime);
					info.speed += info.accel;

					if(info.useAngularAccelCurve)
						info.angularAccel = info.angularAccelCurve.Evaluate(currentTime) * Mathf.Deg2Rad;
					
					if(info.angularAccel != 0f) {
						info.direction += info.angularAccel;
						bullet.gameObject.transform.rotation = Quaternion.AngleAxis(info.direction * Mathf.Rad2Deg + 90, Vector3.forward);
					}

					if(info.useScaleCurve) {
						bullet.gameObject.transform.localScale = new Vector3(info.scaleCurveX.Evaluate(currentTime), 
						                                                     info.scaleCurveY.Evaluate(currentTime), 
						                                                     1f);
					}

					if(!cameraRect.Contains(bullet.transform.position)) {
						info.died = true;
						diedBullets.Add(bullet);
					}
				} else {
					diedBullets.Add(bullet);
				}
			}
			foreach(DMKBulletInfo bullet in diedBullets) {
				DestroyImmediate(bullet.gameObject);
			}
			this.bulletContainer.RemoveAll(
				b => {
					return b.bulletInfo.died;
				}
			);

			foreach(DMKBulletEmitter emitter in this.danmakus[currentAttackIndex].emitters) {
				emitter.DMKUpdateFrame(_currentFrame);
			}

			_currentFrame += 1;
		}
	}

};
