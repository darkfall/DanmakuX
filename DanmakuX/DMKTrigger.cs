using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace danmakux {

	public enum DMKTriggerType {
		Once,
		Repeat,
		Death
	};

	public enum DMKTriggerTarget {
		Parent,
		Bullet
	};

	public class DMKTriggerInfo {
		public int 				currentFrame;
		public DMKTrigger 		parent;
		public WeakReference 	trackingObject;
		public object 			userData;
		
		public DMKTriggerInfo(UnityEngine.Object obj, DMKTrigger p) {
			currentFrame = 0;
			trackingObject = new WeakReference(obj);
			parent = p;
		}
		
		public void UpdateFrame(int f) {
			currentFrame ++;
			if(currentFrame >= parent.triggerFrame) {
				if(parent.triggerType == DMKTriggerType.Repeat)
					this.currentFrame = 0;
				parent.OnTrigger(this);
			}
		}
	};

	[Serializable]
	public class DMKTrigger: DMKNode {

		public int 				triggerFrame = 10;
		public DMKTriggerType 	triggerType;
		public DMKTriggerTarget	triggerTarget;

		public DMKNode parent;

		public List<DMKTriggerInfo> triggers = new List<DMKTriggerInfo>();

		public void DMKInit() {
			if(this.triggerTarget == DMKTriggerTarget.Parent) {
				this.AddNewTriggerObject(this.parent);
			}
		}

		public void DMKUpdateFrame(int currentFrame) {
			foreach(DMKTriggerInfo info in triggers)
				info.UpdateFrame(currentFrame);

			triggers.RemoveAll(o => {
				return (this.triggerType != DMKTriggerType.Repeat && !this.IsTriggerAlive(o)) || 
						!o.trackingObject.IsAlive;
			});
		}
		
		public void AddNewTriggerObject(UnityEngine.Object obj) {
			DMKTriggerInfo trigger = new DMKTriggerInfo(obj, this);
			triggers.Add (trigger);
			this.OnAddNewTrigger(trigger);
		}


		public virtual bool IsTriggerAlive(DMKTriggerInfo info) {
			return info.currentFrame > this.triggerFrame;
		}

		public virtual void OnTrigger(DMKTriggerInfo trigger) {

		}

		public virtual bool RequireShooter() {
			return false;
		}

		public virtual DMKBulletShooterController LinkedShooter() {
			return null;
		}

		public virtual void OnLinkedWith(DMKBulletShooterController shooter) {

		}

		public virtual void OnAddNewTrigger(DMKTriggerInfo trigger) {

		}

		public virtual void CopyFrom(DMKTrigger trigger)
		{
			this.triggerFrame = trigger.triggerFrame;
			this.triggerType = trigger.triggerType;
			this.triggerTarget = trigger.triggerTarget;
		}

		public override void OnEditorGUI(bool showHelp) {
			this.triggerFrame = EditorGUILayout.IntField("Trigger Frame", this.triggerFrame);
			this.triggerType = (DMKTriggerType) EditorGUILayout.EnumPopup("Trigger Type", this.triggerType);
			this.triggerTarget = (DMKTriggerTarget) EditorGUILayout.EnumPopup("Trigger Target", this.triggerTarget);
		}

		public virtual string DMKName() {
			return "Trigger";
		}

		#region editor

		#endregion

	};
		
}
