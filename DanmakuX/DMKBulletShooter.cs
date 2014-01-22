using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace danmakux {

	[Serializable]
	public class DMKBulletShooter: ScriptableObject {

		[SerializeField]
		public DMKShooterModifier modifier = null;

		public DMKBulletShooterController parentController = null;

		public void DMKInit() {	
			if (modifier != null)
				modifier.DMKInit ();
			this.OnInit();
		}

		public void ShootBullet (Vector3 position, float direction, float speedMultiplier = 1f)
		{
			if (modifier != null && modifier.editorEnabled) {
				modifier.OnShootBullet (parentController, position, direction, speedMultiplier);
			} else
				parentController.CreateBullet (position, direction, speedMultiplier);
		}
		
		public void ShootBulletTo (Vector3 position, GameObject target, float speedMultiplier = 1f)
		{
			Vector3 targetPos = target.transform.position;
			Vector3 dis = targetPos - position;
			float angle = (float)(Math.Atan2 (dis.y, dis.x) * Mathf.Rad2Deg);
			this.ShootBullet (position, angle, speedMultiplier);
		} 

		public virtual string DMKName () {
			return "DMKBulletShooter";
		}

		public virtual string DMKSummary() {
			return "";
		}

		public virtual void CopyFrom(DMKBulletShooter shooter) {
			//this.modifier = shooter.modifier;
		}

		public virtual void OnShoot(int frame) {

		}

		public virtual void OnInit() {

		}

		public virtual void OnEditorGUI() {

		}

	};

		
}
