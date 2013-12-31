using System;
using UnityEngine;

class DMKTargetEmitter: DMKBulletEmitter {
	public override void DMKShoot(int frame) {
		this.ShootBulletTo(this.gameObject.transform.position,
		                   null);
	}

	public override string DMKName() {
		return "Target Emitter";
	}

};
