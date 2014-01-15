using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public enum DMKDanmakuPlayMode {
	Sequence,
	Randomized,
	All,
};

[Serializable]
public class DMKDanmaku {
	public string name;

	public DMKDanmakuPlayMode playMode = DMKDanmakuPlayMode.All;
	public int 	  			  playInterval = 0;

	public int currentFrame;

	[SerializeField]
	public DMKController parentController;
	
	[SerializeField]
	public List<DMKBulletShooterController> shooters;


	[SerializeField]
	List<DMKBulletShooterController> _availableShooters;

	[SerializeField]
	DMKBulletShooterController _currentEmitter;

	[SerializeField]
	int _currentEmitterIndex;
	int _currentInterval;
	
	public override string ToString() {
		return name;
	}

	public void UpdateShooters() {
		_availableShooters = this.shooters.FindAll(o => {
			return o.editorEnabled;
		});
	}
	
	public void Play(DMKController controller) {
		parentController = controller;
		currentFrame = 0;

		this.UpdateShooters();

		foreach(DMKBulletShooterController emitter in _availableShooters) {
			emitter.parentController = parentController;
		}
		
		switch(playMode) {
		case DMKDanmakuPlayMode.All:
			foreach(DMKBulletShooterController emitter in _availableShooters) {
				emitter.enabled = true;
			}
			break;
			
		case DMKDanmakuPlayMode.Randomized:
			_currentEmitterIndex = UnityEngine.Random.Range(0, _availableShooters.Count);
			break;
			
		case DMKDanmakuPlayMode.Sequence:
			_currentEmitterIndex = 0;
			break;
		}
		
		if(playMode != DMKDanmakuPlayMode.All) {
			_currentEmitter = _availableShooters[_currentEmitterIndex];
			_currentEmitter.enabled = true;
		}
	}
	
	public void Stop() {
		if(_availableShooters != null)
			foreach(DMKBulletShooterController emitter in _availableShooters) {
				emitter.enabled = false;
			}
	}
	
	public void Update() {
		if(playMode == DMKDanmakuPlayMode.All) {
			foreach(DMKBulletShooterController emitter in _availableShooters) {
				emitter.DMKUpdateFrame(currentFrame);
			}
			currentFrame += 1;
		} else {
			if(_currentInterval == 0) {
				_currentEmitter.DMKUpdateFrame(currentFrame);
				if(_currentEmitter.Ended) {
					if(playMode == DMKDanmakuPlayMode.Randomized)
						_currentEmitterIndex = UnityEngine.Random.Range(0, _availableShooters.Count);
					else {
						++_currentEmitterIndex;
						if(_currentEmitterIndex >= _availableShooters.Count)
							_currentEmitterIndex = 0;
					}
					_currentEmitter.enabled = false;
					_currentInterval = playInterval;
					_currentEmitter = _availableShooters[_currentEmitterIndex];
					_currentEmitter.enabled = true;

					currentFrame = 0;
				}
				currentFrame += 1;
			} else {
				--_currentInterval;
			}
		}

	}
	
	#region editor
	
	public bool editorExpanded = true;
	
	#endregion
	
};

