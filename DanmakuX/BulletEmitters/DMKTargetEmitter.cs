using System;
using UnityEngine;

class DMKTargetEmitter: DMKBulletEmitter {
	public override void DMKShoot(int frame) {
		this.ShootBulletTo(new Vector3(0, 0, 0),
		                   null);
	}

	public override string DMKName() {
		return "Target Emitter";
	}

};
