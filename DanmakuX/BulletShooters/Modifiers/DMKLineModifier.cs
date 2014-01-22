using UnityEditor;
using UnityEngine;
using System;

namespace danmakux {

	[Serializable]
	public class DMKLineModifier: DMKShooterModifier {

		public int count = 1;
		public float speedAttenuation = 1;

		public override void OnShootBullet(DMKBulletShooterController parentController, Vector3 pos, float direction, float speedMultiplier) {
			float sm = speedMultiplier;
			for(int i=0; i<count; ++i) {
				this.DoShootBullet(parentController, pos, direction, sm);
				sm *= speedAttenuation;
			}
		}
		
		public override void CopyFrom(DMKShooterModifier rhs) {
			if(rhs.GetType() == typeof(DMKLineModifier)) {
				DMKLineModifier lm = rhs as DMKLineModifier;
				this.count = lm.count;
				this.speedAttenuation = lm.speedAttenuation;
			}

			base.CopyFrom(rhs);
		}
		
		public override string DMKName() {
			return "Line Modifier";
		}

		public override void OnEditorGUI(bool showHelp) {
			base.OnEditorGUI(showHelp);

			this.count = EditorGUILayout.IntField("Count", this.count);
			this.speedAttenuation = EditorGUILayout.FloatField("Speed Attenuation", this.speedAttenuation);
		}
	};

}

