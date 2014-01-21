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
public class DMKDanmaku: ScriptableObject {
	public string name;

	public DMKDanmakuPlayMode playMode = DMKDanmakuPlayMode.All;
	public int 	  			  playInterval = 0;

	public int currentFrame;

	[SerializeField]
	public DMKController parentController;
	
	[SerializeField]
	public List<DMKBulletShooterController> shooters;
	
	[SerializeField]
	public List<DMKShooterModifier> modifiers = new List<DMKShooterModifier>();

	[SerializeField]
	public List<DMKTrigger> triggers = new List<DMKTrigger>();

	[SerializeField]
	List<DMKBulletShooterController> _availableShooters;

	[SerializeField]
	DMKBulletShooterController _currentShooter;

	[SerializeField]
	int _currentShooterIndex;
	int _currentInterval;
	
	public override string ToString() {
		return name;
	}

	public void UpdateShooters() {
		_availableShooters = this.shooters.FindAll(o => {
			return o.editorEnabled;
		});
	}

	public void UpdateCurrentShooter() {
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
	}
	
	public void Play(DMKController controller) {
		parentController = controller;
		currentFrame = 0;

		this.UpdateShooters();

		foreach(DMKBulletShooterController emitter in _availableShooters) {
			emitter.parentController = parentController;
		}
		foreach(DMKShooterModifier modifier in modifiers) {
			modifier.DMKInit();
		}
		foreach(DMKTrigger trigger in triggers)
			trigger.DMKInit();

		this.UpdateCurrentShooter();

		if(playMode != DMKDanmakuPlayMode.All) {
			_currentShooter = _availableShooters[_currentShooterIndex];
			_currentShooter.enabled = true;
		}
	}
	
	public void Stop() {
		if(_availableShooters != null)
			foreach(DMKBulletShooterController emitter in _availableShooters) {
				emitter.enabled = false;
			}
	}
	
	public void Update() {
		foreach(DMKTrigger trigger in this.triggers)
			trigger.DMKUpdateFrame(currentFrame);

		if(playMode == DMKDanmakuPlayMode.All) {
			foreach(DMKBulletShooterController shooter in _availableShooters) {
				if(shooter.enabled)
					shooter.DMKUpdateFrame(currentFrame);
			}
			currentFrame += 1;
		} else {
			if(_currentInterval == 0) {
				_currentShooter.DMKUpdateFrame(currentFrame);
				if(_currentShooter.Ended) {
					if(playMode == DMKDanmakuPlayMode.Randomized)
						_currentShooterIndex = UnityEngine.Random.Range(0, _availableShooters.Count);
					else {
						++_currentShooterIndex;
						if(_currentShooterIndex >= _availableShooters.Count)
							_currentShooterIndex = 0;
					}
					_currentShooter.enabled = false;
					_currentInterval = playInterval;
					_currentShooter = _availableShooters[_currentShooterIndex];
					_currentShooter.enabled = true;

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
	}
	
	public static void Copy(DMKDanmaku danmaku) {

	}

	
	#region editor
	
	public bool editorExpanded = true;
	
	#endregion
	
};

