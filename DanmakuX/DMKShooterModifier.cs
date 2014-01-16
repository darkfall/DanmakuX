using UnityEditor;
using UnityEngine;
using System;

[Serializable]
public class DMKShooterModifier: ScriptableObject {
	public DMKShooterModifier 				next = null;
	public DMKBulletShooterController		parentShooter = null;
	
	public virtual void DMKInit() {
		if(next != null)
			next.DMKInit();
	}

	public virtual void OnShootBullet(Vector3 pos, float direction, float speedMultiplier) {
		this.DoShootBullet(pos, direction, speedMultiplier);
	}
	
	public virtual void CopyFrom(DMKShooterModifier rhs) {
		if(rhs.next != null) {
			this.next = (DMKShooterModifier)ScriptableObject.CreateInstance(rhs.GetType());
			this.next.parentShooter = this.parentShooter;
			this.next.CopyFrom(rhs);
		} else
			this.next = null;
	}

	public virtual string DMKName() {
		return "DMKEmitterModifier";
	}
	
	public virtual void OnEditorGUI(bool showHelp = false) {
		
	}

	public void DoShootBullet(Vector3 pos, float direction, float speedMultiplier) {
		if(next != null)
			next.OnShootBullet(pos, direction, speedMultiplier);
		else
			parentShooter.CreateBullet(pos, direction, speedMultiplier);
	}

	#region editor

	public Rect editorWindowRect;

	#endregion

};
