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

	[SerializeField]
	public DMKController parentController;
	
	[SerializeField]
	public List<DMKBulletEmitter> emitters;

	public int currentFrame;

	List<DMKBulletEmitter> _availableEmitters;
	DMKBulletEmitter	   _currentEmitter;
	int				 	   _currentEmitterIndex;
	int 				   _currentInterval;
	
	public override string ToString() {
		return name;
	}
	
	public void Play(DMKController controller) {
		parentController = controller;
		currentFrame = 0;

		_availableEmitters = this.emitters.FindAll(o => {
			return o.editorEnabled;
		});
		foreach(DMKBulletEmitter emitter in _availableEmitters) {
			emitter.parentController = parentController;
		}
		
		switch(playMode) {
		case DMKDanmakuPlayMode.All:
			foreach(DMKBulletEmitter emitter in _availableEmitters) {
				emitter.enabled = true;
			}
			break;
			
		case DMKDanmakuPlayMode.Randomized:
			_currentEmitterIndex = UnityEngine.Random.Range(0, _availableEmitters.Count);
			break;
			
		case DMKDanmakuPlayMode.Sequence:
			_currentEmitterIndex = 0;
			break;
		}
		
		if(playMode != DMKDanmakuPlayMode.All) {
			_currentEmitter = _availableEmitters[_currentEmitterIndex];
			_currentEmitter.enabled = true;
		}
	}
	
	public void Stop() {
		foreach(DMKBulletEmitter emitter in _availableEmitters) {
			emitter.enabled = false;
		}
	}
	
	public void Update() {
		if(playMode == DMKDanmakuPlayMode.All) {
			foreach(DMKBulletEmitter emitter in _availableEmitters) {
				emitter.DMKUpdateFrame(currentFrame);
			}
		} else {
			if(_currentInterval == 0) {
				_currentEmitter.DMKUpdateFrame(currentFrame);
				if(_currentEmitter.Ended) {
					if(playMode == DMKDanmakuPlayMode.Randomized)
						_currentEmitterIndex = UnityEngine.Random.Range(0, _availableEmitters.Count);
					else {
						++_currentEmitterIndex;
						if(_currentEmitterIndex >= _availableEmitters.Count)
							_currentEmitterIndex = 0;
					}
					_currentEmitter.enabled = false;
					_currentInterval = playInterval;
					_currentEmitter = _availableEmitters[_currentEmitterIndex];
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

