using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
		if (modifier != null) {
			modifier.OnShootBullet (position, direction, speedMultiplier);
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
	
	public void AddModifier (DMKShooterModifier modifier)
	{
		DMKShooterModifier m = this.modifier;
		if (m == null)
			this.modifier = modifier;
		else {
			while (m.next != null)
				m = m.next;
			m.next = modifier;
		}
		modifier.parentShooter = this.parentController;
	}
	
	public void RemoveModifier (DMKShooterModifier modifier)
	{
		DMKShooterModifier m = this.modifier;
		DMKShooterModifier p = null;
		while (m != null) {
			if (m == modifier) {
				if (p == null) {
					this.modifier = m.next;
				} else {
					p.next = m.next;
				}
				break;
			}
			p = m;
			m = m.next;
		}
	}

	public virtual string DMKName () {
		return "DMKBulletShooter";
	}

	public virtual string DMKSummary() {
		return "";
	}

	public virtual void CopyFrom(DMKBulletShooter shooter) {
		if(shooter.modifier != null) {
			this.modifier = (DMKShooterModifier)ScriptableObject.CreateInstance(shooter.modifier.GetType());
			this.modifier.parentShooter = this.parentController;
			this.modifier.CopyFrom(shooter.modifier);
		} else
			this.modifier = null;
	}

	public virtual void OnShoot(int frame) {

	}

	public virtual void OnInit() {

	}

	public virtual void OnEditorGUI() {

	}

};

