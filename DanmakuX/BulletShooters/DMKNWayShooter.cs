using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace danmakux {

	public class DMKNWayShooter: DMKBulletShooter {
		public int 	  bulletCount;
		public DMKCurveProperty  radius = new DMKCurveProperty(0f);
		public DMKCurveProperty  angleRange = new DMKCurveProperty(360f);
		public DMKCurveProperty  startAngle = new DMKCurveProperty(0f);
		public float  accel1 = 0f;
		public float  accel2 = 0f;
		public bool   trackTarget = false;
		public GameObject targetObject;

		float _acceleration;
		float _currentAngle;

		public override string DMKName() {
			return "N-Way Shooter";
		}
		
		public override string DMKSummary() {
			return String.Format("(Count = {0}, Radius = {1})", this.bulletCount, this.radius);
		}
		
		public override void OnInit() {
			_currentAngle = startAngle.Update(0, true);
			_acceleration = accel1;
		}

		public override void OnShoot(int frame) {
			float start = _currentAngle;
			float t = (float)frame / 60f;

			if(this.startAngle.type != DMKCurvePropertyType.Constant) {
				_currentAngle = this.startAngle.Update(t, true);
			} else {
				_currentAngle += _acceleration;
				_acceleration += accel2;
				if(_currentAngle > 360)
					_currentAngle -= 360;
			}

			if(trackTarget && targetObject != null) {
				start = DMKUtil.GetDgrBetweenObjects(this.parentController.gameObject, targetObject);
			}
			for(int i=0; i<bulletCount; ++i) {
				float angle = angleRange.Update(t) / bulletCount * i + start;
				Vector3 diff = Vector3.zero;
				if(radius.Update(t) != 0f) {
					diff = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius.get (),
					                   Mathf.Sin(angle * Mathf.Deg2Rad) * radius.get (),
					                   0);
				}
				this.ShootBullet(diff,
				                 angle);
			}
		}

		public override void CopyFrom(DMKBulletShooter shooter)
		{
			if(shooter.GetType() == typeof(DMKNWayShooter)) {
				DMKNWayShooter cs = shooter as DMKNWayShooter;
				this.accel1 = cs.accel1;
				this.accel2 = cs.accel2;
				this.angleRange = DMKCurveProperty.Copy(cs.angleRange);
				this.bulletCount = cs.bulletCount;
				this.radius = DMKCurveProperty.Copy(cs.radius);
				this.startAngle = DMKCurveProperty.Copy(cs.startAngle);
				this.targetObject = cs.targetObject;
				this.trackTarget = cs.trackTarget;
			}
			base.CopyFrom (shooter);
		}

		#region editor
#if UNITY_EDITOR
		public override void OnEditorGUI() {
			base.OnEditorGUI();

			this.bulletCount = EditorGUILayout.IntField("Way Count", this.bulletCount);
			if(this.bulletCount < 0)
				this.bulletCount = 0;

			EditorGUI.BeginChangeCheck();
			this.trackTarget = EditorGUILayout.Toggle("Facing Target", this.trackTarget);
			if(!this.trackTarget) {
				DMKGUIUtility.MakeCurveControl(ref this.startAngle, "Start Angle");
				//this.startAngle = EditorGUILayout.FloatField("Start Angle", this.startAngle);
			} else {
				this.targetObject = (GameObject)EditorGUILayout.ObjectField("Target", this.targetObject, typeof(GameObject), true);
			}
			DMKGUIUtility.MakeCurveControl(ref this.angleRange, "Angular Range");
			if(EditorGUI.EndChangeCheck()) {
				this.startAngle.Update(0);
				this.angleRange.Update(0);
				this._currentAngle = this.startAngle.get();
			}

			DMKGUIUtility.MakeCurveControl(ref this.radius, "Emission Radius");

			if(this.startAngle.type == DMKCurvePropertyType.Constant) {
				EditorGUI.BeginChangeCheck();
				this.accel1 = EditorGUILayout.FloatField("Acceleration 1", this.accel1);
				if(EditorGUI.EndChangeCheck())
					this._acceleration = this.accel1;
				
				this.accel2 = EditorGUILayout.FloatField("Acceleration 2", this.accel2);
				this._acceleration = this.accel1;
			}

		}
#endif
		#endregion

	};
	
}
