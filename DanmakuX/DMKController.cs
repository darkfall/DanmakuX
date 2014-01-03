using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DMKDanmaku {
	public string name;

	[SerializeField]
	public List<DMKBulletEmitter> emitters;

	public override string ToString() {
		return name;
	}

#region editor

	public bool editorExpanded = true;

#endregion

};

[Serializable]
public class DMKController: MonoBehaviour {

	[SerializeField]
	public List<DMKDanmaku> danmakus;
	public int currentAttackIndex = -1;
	
	public int maxBulletCount;
	[SerializeField]
	public List<DMKBulletInfo> bulletContainer = new List<DMKBulletInfo>();

	public bool paused = false;

	// that's for STG!
	// we want a 60fps update because every value in DMK is related to frame
	double _prevTime;
	bool   _needInternalTimer;
	int    _currentFrame;

	public void Awake() {
		_needInternalTimer = DMKSettings.instance.CheckNeedInternalTimer();

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
			if(delta < DMKSettings.instance.frameInterval) {
				return;
			}
			double realDelta;
			if(_prevTime < currentTime - DMKSettings.instance.frameInterval &&
			   _prevTime > currentTime - (DMKSettings.instance.frameInterval * 2)) {
				realDelta = DMKSettings.instance.frameInterval;
				_prevTime += DMKSettings.instance.frameInterval;
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
		if(currentAttackIndex != -1) {
			if(currentAttackIndex < this.danmakus.Count) {
				foreach(DMKBulletEmitter emitter in this.danmakus[currentAttackIndex].emitters) {
					emitter.enabled = false;
					emitter.parentController = this;
				}
			} else
				currentAttackIndex = -1;
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
//				if(emitter.deathParentEmitter == null) {
					if(Application.isEditor)
						emitter.enabled = true && emitter.editorEnabled;
					else
						emitter.enabled = true;
		//		}
				// otherwise will be initiated by parent emitter
			}
		}
		_currentFrame = 0;
		paused = false;
	}

	void DMKUpdate() {
		if(!paused && enabled && currentAttackIndex != -1) {
			Rect cameraRect = DMKSettings.instance.GetCameraRect();
		
			List<DMKBulletInfo> diedBullets = new List<DMKBulletInfo>();
			foreach(DMKBulletInfo bullet in this.bulletContainer) {
				DMKBulletInfoInternal info = bullet.bulletInfo;
				if(!info.died) {
					Vector3 prevPos = bullet.gameObject.transform.position;
					float dist = info.speed.value * DMKSettings.instance.unitPerPixel;
					bullet.gameObject.transform.position = new Vector3(prevPos.x + (float)(dist * Mathf.Cos (info.direction)),
					                                                   prevPos.y + (float)(dist * Mathf.Sin (info.direction)), 
					                                                   prevPos.z);

					int frame = _currentFrame - info.startFrame;
					float currentTime = (float)(frame) / 60;
					info.speed.Update(currentTime);
					info.accel.Update(currentTime);
					info.speed.value += info.accel.value;
					info.angularAccel.Update(currentTime);
					info.angularAccel.value *= Mathf.Deg2Rad;
					
					if(info.angularAccel.value != 0f) {
						info.direction += info.angularAccel.value;
						bullet.gameObject.transform.rotation = Quaternion.AngleAxis(info.direction * Mathf.Rad2Deg + 90, Vector3.forward);
					}

					if(info.useScaleCurve) {
						bullet.gameObject.transform.localScale = new Vector3(info.scaleCurveX.Evaluate(currentTime), 
						                                                     info.scaleCurveY.Evaluate(currentTime), 
						                                                     1f);
					}

					if((info.maxLifetime != 0 && info.maxLifetime <= frame) ||
					   !cameraRect.Contains(bullet.transform.position)) {
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
