using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DMKController: MonoBehaviour {

	[SerializeField]
	public List<DMKDanmaku> danmakus;
	public int currentAttackIndex = -1;
	
	public int maxBulletCount;
	[SerializeField]
	public List<DMKBullet> bulletContainer = new List<DMKBullet>();

	public bool paused = false;

	public bool playing {
		get { return currentAttackIndex != -1 && !paused; }
	}

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
	}

	public void Start() {
		if(Application.isEditor)
			_prevTime = EditorApplication.timeSinceStartup;
		else
			_prevTime = Time.timeSinceLevelLoad;
		this.StartDanmaku(-1);
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

	public void StartDanmaku(DMKDanmaku danmaku) {
		this.StartDanmaku(this.danmakus.IndexOf(danmaku));
	}

	public void StartDanmaku(int index) {
		if(currentAttackIndex != -1) {
			if(currentAttackIndex < this.danmakus.Count) {
				this.danmakus[currentAttackIndex].Stop();
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
			try {
				foreach(DMKBullet bullet in this.bulletContainer) {
					DestroyImmediate(bullet.gameObject);
				}
			} catch {
				
			}
			this.bulletContainer.Clear();
		} else {
			this.danmakus[currentAttackIndex].Play(this);
		}
		paused = false;
		_currentFrame = 0;
	}

	void DMKUpdate() {
		if(!paused && enabled && currentAttackIndex != -1) {
			Rect cameraRect = DMKSettings.instance.GetCameraRect();
		
			List<DMKBullet> diedBullets = new List<DMKBullet>();
			// to do, put bullet update in another thread
			foreach(DMKBullet bullet in this.bulletContainer) {
				DMKBulletInfo info = bullet.bulletInfo;
				if(!info.died) {
					Vector3 prevPos = bullet.gameObject.transform.position;
					float dist = info.speed.get() * DMKSettings.instance.unitPerPixel;
					bullet.gameObject.transform.position = new Vector3(prevPos.x + (float)(dist * Mathf.Cos (info.direction)),
					                                                   prevPos.y + (float)(dist * Mathf.Sin (info.direction)), 
					                                                   prevPos.z);

					float currentTime = (float)(info.lifeFrame) / 60;
					info.speed.Update(currentTime);
					info.speed.value += info.accel.Update(currentTime);

					float ang = info.angularAccel.Update(currentTime);
					if(ang != 0f) {
						info.direction += ang * Mathf.Deg2Rad;
						bullet.gameObject.transform.rotation = Quaternion.AngleAxis(info.direction * Mathf.Rad2Deg + 90, Vector3.forward);
					}

					if(info.useScaleCurve) {
						bullet.gameObject.transform.localScale = new Vector3(info.scaleCurveX.Evaluate(currentTime), 
						                                                     info.scaleCurveY.Evaluate(currentTime), 
						                                                     1f);
					}

					if((info.maxLifetime != 0 && info.maxLifetime <= info.lifeFrame) ||
					   !cameraRect.Contains(bullet.transform.position)) {
						info.died = true;
						diedBullets.Add(bullet);

					}

					info.lifeFrame++;
				} else {
					diedBullets.Add(bullet);
				}
			}
			foreach(DMKBullet bullet in diedBullets) {
				DestroyImmediate(bullet.gameObject);
			}
			this.bulletContainer.RemoveAll(
				b => {
					return b.bulletInfo.died;
				}
			);

			this.danmakus[currentAttackIndex].Update();
		}
	}

};
