using UnityEditor;
using UnityEngine;
using System;

[Serializable]
public class DMKShooterModifier: DMKNode {
	public DMKShooterModifier next = null;
	
	public virtual void DMKInit() {
		if(next != null)
			next.DMKInit();
	}

	public virtual void OnShootBullet(DMKBulletShooterController parentShooter, Vector3 pos, float direction, float speedMultiplier) {
		this.DoShootBullet(parentShooter, pos, direction, speedMultiplier);
	}
	
	public virtual void CopyFrom(DMKShooterModifier rhs) {
		this.next = rhs.next;
	}

	public virtual string DMKName() {
		return "DMKEmitterModifier";
	}

	public void DoShootBullet(DMKBulletShooterController parentShooter, Vector3 pos, float direction, float speedMultiplier) {
		if(next != null)
			next.OnShootBullet(parentShooter, pos, direction, speedMultiplier);
		else
			parentShooter.CreateBullet(pos, direction, speedMultiplier);
	}


};
