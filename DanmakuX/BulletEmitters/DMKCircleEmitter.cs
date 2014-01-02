using System;
using UnityEngine;
using UnityEditor;

public class DMKCircleEmitter: DMKBulletEmitter {
	public int 	  bulletCount;
	public float  radius = 0f;
	public float  angleRange = 360f;
	public float  startAngle = 0f;
	public float  accel1 = 0f;
	public float  accel2 = 0f;
	public bool   trackTarget = false;
	public GameObject targetObject;

	float _acceleration;
	float _currentAngle;

	public override void DMKInit() {
		_currentAngle = startAngle;
		_acceleration = accel1;

		base.DMKInit();
	}

	public override void DMKShoot(int frame) {
		float start = _currentAngle;
		if(trackTarget && targetObject != null) {
			start = DMKUtil.GetDgrBetweenObjects(this.gameObject, targetObject);
		}
		_currentAngle += _acceleration;
		_acceleration += accel2;
		if(_currentAngle > 360)
			_currentAngle -= 360;

		for(int i=0; i<bulletCount; ++i) {
			float angle = angleRange / bulletCount * i + start;
			Vector3 diff = Vector3.zero;
			if(radius != 0f) {
				diff = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
				                   Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
				                   0);
			}
			this.ShootBullet(this.gameObject.transform.position + diff,
			                 angle);
		}
	}

	public override string DMKName() {
		return "Circle Emitter";
	}

	#region editor

	public override void OnEditorGUI() {
		base.OnEditorGUI();

		this.bulletCount = EditorGUILayout.IntField("Bullet Count", this.bulletCount);
		this.radius 	 = EditorGUILayout.FloatField("Emission Radius", this.radius);
		this.accel1		 = EditorGUILayout.FloatField("Acceleration 1", this.accel1);
		this.accel2 	 = EditorGUILayout.FloatField("Acceleration 2", this.accel2);

		this.trackTarget = EditorGUILayout.Toggle("Facing Target", this.trackTarget);
		if(!this.trackTarget) {
			this.startAngle = EditorGUILayout.FloatField("Start Angle", this.startAngle);
		} else {
			this.targetObject = (GameObject)EditorGUILayout.ObjectField("Target", this.targetObject, typeof(GameObject), true);
		}
		this.angleRange  = EditorGUILayout.FloatField("Angular Range", this.angleRange);
	}

	#endregion

};
