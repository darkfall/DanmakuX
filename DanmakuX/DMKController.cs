using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace danmakux {


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
		int    _maxBulletCount;

		double GetTimeSinceStartup() {
#if UNITY_EDITOR
			return EditorApplication.timeSinceStartup;
#else
			return Time.timeSinceLevelLoad;
#endif
		}

		public void Awake() {
			_needInternalTimer = DMKSettings.instance.CheckNeedInternalTimer();

			_prevTime = GetTimeSinceStartup();
		}

		public void Start() {
			_prevTime = GetTimeSinceStartup();
			this.StartDanmaku(-1);
		}

		public void Update() {
			double currentTime;
			currentTime = GetTimeSinceStartup();

			if(_needInternalTimer || Application.isEditor) {
				double delta = currentTime - _prevTime;
				if(delta < DMKSettings.instance.frameInterval) {
					return;
				}
				if(_prevTime < currentTime - DMKSettings.instance.frameInterval &&
				   _prevTime > currentTime - (DMKSettings.instance.frameInterval * 2)) {
					_prevTime += DMKSettings.instance.frameInterval;
				} else {
					_prevTime = currentTime;
				}

				this.DMKUpdate();
			} else {
				this.DMKUpdate();
			}
		}

		public bool CanAddBullet() {
			return _maxBulletCount == 0 || (bulletContainer.Count < _maxBulletCount);
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
				return;
			}

			foreach(DMKBullet bullet in this.bulletContainer) {
				if(bullet != null)
					DestroyImmediate(bullet.gameObject);
			}
			this.bulletContainer.Clear();

			if(currentAttackIndex != -1)
				this.danmakus[currentAttackIndex].Play(this);

			paused = false;

			_maxBulletCount = maxBulletCount;
			if(_maxBulletCount == 0) {
				_maxBulletCount = DMKSettings.instance.MaxNumBullets;
			}
		}

		public void RestartCurrentDanmaku() {
			if(this.currentAttackIndex != -1)
				this.StartDanmaku(this.currentAttackIndex);
		}

		void DMKUpdate() {
			if(!paused && enabled && currentAttackIndex != -1) {
				Rect cameraRect = DMKSettings.instance.GetCameraRect();
			
				List<DMKBullet> diedBullets = new List<DMKBullet>();
				// to do, put bullet update in another thread
				foreach(DMKBullet bullet in this.bulletContainer) {
					if(bullet == null) {
						diedBullets.Add (bullet);
						continue;
					}
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
					if(bullet != null)
						DestroyImmediate(bullet.gameObject);
				}
				this.bulletContainer.RemoveAll(
					b => {
						return b == null || b.bulletInfo.died;
					}
				);

				this.danmakus[currentAttackIndex].Update();
			}
		}

	};

}