using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace danmakux {

	public enum DMKDanmakuPlayMode {
		Sequence,
		Randomized,
		All,
	};

	[Serializable]
	public class DMKDanmaku: ScriptableObject {
		//public string name;

		public DMKDanmakuPlayMode playMode = DMKDanmakuPlayMode.All;
		public int 	  			  playInterval = 0;

		public int currentFrame;

		[SerializeField]
		public DMKController parentController;
		
		[SerializeField]
		public List<DMKBulletShooterController> shooters = new List<DMKBulletShooterController>();
		
		[SerializeField]
		public List<DMKShooterModifier> modifiers = new List<DMKShooterModifier>();

		[SerializeField]
		public List<DMKTrigger> triggers = new List<DMKTrigger>();

		[SerializeField]
		List<DMKBulletShooterController> _availableShooters;

		[SerializeField]
		List<DMKBulletShooterController> _currentShooters;

		[SerializeField]
		int _currentShooterIndex;
		int _currentInterval;

		GameObject bulletContainer;	
		
		public override string ToString() {
			return name;
		}

		public void UpdateShooters() {
			_availableShooters = this.shooters.FindAll(o => {
				return o.editorEnabled;
			}).OrderBy(o => o.groupId).ToList();

			ResetCurrentShooter();
		}

		 void ResetCurrentShooter() {
			switch(playMode) {
			case DMKDanmakuPlayMode.All:
				foreach(DMKBulletShooterController shooter in _availableShooters) {
					shooter.enabled = true;
				}
				break;
				
			case DMKDanmakuPlayMode.Randomized:
				_currentShooterIndex = UnityEngine.Random.Range(0, _availableShooters.Count);
				break;
				
			case DMKDanmakuPlayMode.Sequence:
				_currentShooterIndex = 0;
				break;
			}

			if(playMode != DMKDanmakuPlayMode.All) {
				_currentShooters = new List<DMKBulletShooterController>();
				foreach(DMKBulletShooterController shooter in _availableShooters) {
					if(shooter.groupId == _availableShooters[_currentShooterIndex].groupId) {
						_currentShooters.Add (shooter);
						shooter.enabled = true;
					}
				}
			} else
				_currentShooters = _availableShooters;
		}
		
		public void Play(DMKController controller) {
			parentController = controller;
			currentFrame = 0;

			if(bulletContainer != null && DMKSettings.instance.bulletStorePolicy == DMKBulletStorePolicy.ByController)
				DestroyImmediate(bulletContainer);
			bulletContainer = DMKSettings.instance.GetBulletContainer(this.parentController);
			foreach(DMKBulletShooterController shooterController in shooters) {
				shooterController.parentController = parentController;
				shooterController.bulletContainer = bulletContainer;

				DMKSubBulletShooterController subController = shooterController.subController;
				while(subController != null) {
					subController.internalController.parentController = parentController;
					subController.internalController.bulletContainer = bulletContainer;
					subController = subController.internalController.subController;
				}
			}
			foreach(DMKShooterModifier modifier in modifiers) {
				modifier.DMKInit();
			}
			foreach(DMKTrigger trigger in triggers)
				trigger.DMKInit();
			
			this.UpdateShooters();
		}
		
		public void Stop() {
			
			if(bulletContainer != null && DMKSettings.instance.bulletStorePolicy == DMKBulletStorePolicy.ByController)
				DestroyImmediate(bulletContainer);

			if(_availableShooters != null) {
				foreach(DMKBulletShooterController shooter in _availableShooters) {
					shooter.DMKClear();
				}
			}
		}
		
		public void Update() {
			foreach(DMKTrigger trigger in this.triggers)
				trigger.DMKUpdateFrame(currentFrame);

			if(playMode == DMKDanmakuPlayMode.All) {
				foreach(DMKBulletShooterController shooter in _currentShooters) {
					shooter.DMKUpdateFrame(currentFrame);
				}
				currentFrame += 1;
			} else {
				if(_currentInterval == 0) {
					foreach(DMKBulletShooterController shooter in _currentShooters) {
						shooter.DMKUpdateFrame(currentFrame);
					}
					bool ended = _currentShooters.All(s => s.Ended);
					if(ended) {
						if(playMode == DMKDanmakuPlayMode.Randomized)
							_currentShooterIndex = UnityEngine.Random.Range(0, _availableShooters.Count);
						else {
							while(_availableShooters[_currentShooterIndex].groupId == _currentShooters[0].groupId) {
								++_currentShooterIndex;
								if(_currentShooterIndex >= _currentShooters.Count)
									break;
							}
							if(_currentShooterIndex >= _availableShooters.Count)
								_currentShooterIndex = 0;
						}

						foreach(DMKBulletShooterController shooter in _currentShooters)
							shooter.enabled = false;

						_currentShooters = new List<DMKBulletShooterController>();
						foreach(DMKBulletShooterController shooter in _availableShooters) {
							if(shooter.groupId == _availableShooters[_currentShooterIndex].groupId) {
								_currentShooters.Add (shooter);
								shooter.enabled = true;
							}
						}
						
						_currentInterval = playInterval;

						currentFrame = 0;
					}
					currentFrame += 1;
				} else {
					--_currentInterval;
				}
			}

		}

		
		public void AddModifier (DMKShooterModifier modifier)
		{
			this.modifiers.Add(modifier);
		}
		
		public void RemoveModifier (DMKShooterModifier modifier)
		{
			this.modifiers.Remove(modifier);
			foreach(DMKShooterModifier m in this.modifiers) {
				if(m.next == modifier) {
					m.next = null;
				}
			}
			foreach(DMKBulletShooterController shooterController in this.shooters) {
				if(shooterController.shooter.modifier == modifier) {
					shooterController.shooter.modifier = null;
				}
				DMKSubBulletShooterController subController = shooterController.subController;
				if(subController != null) {
					while(subController != null) {
						if(subController.internalController.shooter.modifier == modifier)
							subController.internalController.shooter.modifier = null;
						subController = subController.internalController.subController;
					}
				}
			}
		}
		
		public void CopyFrom(DMKDanmaku danmaku) {
			this.playMode = danmaku.playMode;
			this.playInterval = danmaku.playInterval;
			this.parentController = danmaku.parentController;

			this.shooters.Clear();
			foreach(DMKBulletShooterController shooterController in danmaku.shooters) {
				DMKBulletShooterController newController = (DMKBulletShooterController)ScriptableObject.CreateInstance<DMKBulletShooterController>();
				newController.CopyFrom(shooterController);
				this.shooters.Add (newController);
			}

			this.modifiers.Clear();
			foreach(DMKShooterModifier modifier in danmaku.modifiers) {
				DMKShooterModifier newModifier = (DMKShooterModifier)ScriptableObject.CreateInstance(modifier.GetType());
				newModifier.CopyFrom(modifier);
				this.modifiers.Add (newModifier);
			}

			this.triggers.Clear();
			foreach(DMKTrigger trigger in danmaku.triggers) {
				DMKTrigger newTrigger = (DMKTrigger)ScriptableObject.CreateInstance(trigger.GetType());
				newTrigger.CopyFrom(trigger);
				this.triggers.Add (newTrigger);
			}
		}

		
		#region editor
		
		public bool editorExpanded = true;
		
		#endregion
		
	};
		
}

