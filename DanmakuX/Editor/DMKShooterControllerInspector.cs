using UnityEngine;
using UnityEditor;
using System;

class DMKShooterControllerInspector {

	public static void OnEditorGUI(DMKBulletShooterController shooter) {
		EditorGUILayout.BeginVertical();
		{
			//shooter.identifier = EditorGUILayout.TextField("Identifier", shooter.identifier);
			shooter.bulletContainer = (GameObject)EditorGUILayout.ObjectField("Bullet Container", shooter.bulletContainer, typeof(GameObject), true);
			
			EditorGUI.BeginChangeCheck();
			DMKGUIUtility.MakeCurveControl(ref shooter.emissionCooldown, "Emission CD");
			shooter.emissionLength = (int)Mathf.Clamp(EditorGUILayout.IntField("Emission Length", shooter.emissionLength), 0, 999999);
			shooter.interval = (int)Mathf.Clamp(EditorGUILayout.IntField("Emission Interval", shooter.interval), 0, 999999);
			shooter.startFrame = (int)Mathf.Clamp(EditorGUILayout.IntField("Start Frame", shooter.startFrame), 0, 999999);
			shooter.overallLength = (int)Mathf.Clamp(EditorGUILayout.IntField("Overall Length", shooter.overallLength), 0, 999999);
			if(EditorGUI.EndChangeCheck()) {
				shooter.DMKInit();
			}
			
			shooter.simulationCount = (int)Mathf.Clamp(EditorGUILayout.IntField("Simulation Count", shooter.simulationCount), 1, 999999);
			
			EditorGUILayout.Space();
			shooter.tag = EditorGUILayout.TextField("Tag", shooter.tag);

			
			if(shooter.positionOffset.type != DMKPositionOffsetType.Absolute)
				shooter.gameObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", shooter.gameObject, typeof(GameObject), true);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Position Offset");
				shooter.positionOffset.type = (DMKPositionOffsetType)EditorGUILayout.EnumPopup(shooter.positionOffset.type);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space (20);
				GUILayout.BeginVertical();

				shooter.positionOffset.OnEditorGUI(false);
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			
		}
		EditorGUILayout.Separator();
		EditorGUILayout.EndVertical();
		
		ShooterGUILowerPart(shooter);
	}

	static void ShooterGUILowerPart_BulletInfo(DMKBulletShooterController shooter) {
		Rect rr = GUILayoutUtility.GetLastRect();
		GUI.Box (new Rect(0, rr.y + rr.height, rr.width, 2),
		         "");
		EditorGUILayout.BeginVertical("");
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			
			string bulletInfoStr = "Bullet";
			if(!shooter.editorBulletInfoExpanded) {
				bulletInfoStr = String.Format("Bullet Info (Speed = {0}, Accel = {1})", 
				                              shooter.bulletInfo.speed.value,
				                              shooter.bulletInfo.accel.value,
				                              shooter.bulletInfo.maxLifetime);
			}
			shooter.editorBulletInfoExpanded = EditorGUILayout.Foldout(shooter.editorBulletInfoExpanded, bulletInfoStr);
			
			if(shooter.editorBulletInfoExpanded) {
				{
					EditorGUILayout.BeginVertical();
					shooter.bulletInfo.bulletSprite = EditorGUILayout.ObjectField("Sprite", shooter.bulletInfo.bulletSprite, typeof(Sprite), false) as Sprite;
					shooter.bulletInfo.bulletColor  = EditorGUILayout.ColorField("Color", shooter.bulletInfo.bulletColor);
					EditorGUILayout.EndVertical();
				}
				
				EditorGUILayout.Separator();
				
				DMKBulletInfo bulletInfo = shooter.bulletInfo;
				DMKGUIUtility.MakeCurveControl(ref bulletInfo.speed, "Speed");
				DMKGUIUtility.MakeCurveControl(ref bulletInfo.accel, "Acceleration");
				DMKGUIUtility.MakeCurveControl(ref bulletInfo.angularAccel, "Angular Accel");
				
				GUILayout.BeginHorizontal();
				{
					GUILayout.Label("Scale");
					bulletInfo.useScaleCurve = DMKGUIUtility.MakeCurveToggle(bulletInfo.useScaleCurve);
				}
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				{
					GUILayout.Space (32);
					GUILayout.BeginVertical();
					
					if(bulletInfo.useScaleCurve) {
						bulletInfo.scaleCurveX = EditorGUILayout.CurveField("Scale X", bulletInfo.scaleCurveX);
						bulletInfo.scaleCurveY = EditorGUILayout.CurveField("Scale Y", bulletInfo.scaleCurveY);
					} else {
						bulletInfo.scale = EditorGUILayout.Vector2Field("", bulletInfo.scale);
					}
					GUILayout.EndVertical();
				}
				GUILayout.EndHorizontal();
				
				shooter.bulletInfo.damage = EditorGUILayout.IntField("Damage", shooter.bulletInfo.damage);
				shooter.bulletInfo.maxLifetime = EditorGUILayout.IntField("Lifetime (Frame)", shooter.bulletInfo.maxLifetime);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.Space ();
		EditorGUILayout.EndVertical();
	}
	
	static void ShooterGUILowerPart_Shooter(DMKBulletShooterController shooter) {
		Rect rr = GUILayoutUtility.GetLastRect();
		GUI.Box (new Rect(0, rr.y + rr.height, rr.width, 2),
		         "");
		
		EditorGUILayout.BeginVertical();
		string shooterStr = "Shooter ";
		if(!shooter.editorShooterInfoExpanded)
			shooterStr += shooter.DMKSummary();
		shooter.editorShooterInfoExpanded = EditorGUILayout.Foldout(shooter.editorShooterInfoExpanded, shooterStr);
		if(shooter.editorShooterInfoExpanded) {
			shooter.followParentDirection = EditorGUILayout.Toggle("Parent Direction", shooter.followParentDirection);
			shooter.OnEditorGUI();
		}
		EditorGUILayout.EndVertical();
	}
	
	
	static void ShooterGUILowerPart(DMKBulletShooterController shooter) {
		ShooterGUILowerPart_BulletInfo(shooter);
		ShooterGUILowerPart_Shooter(shooter);
	}

};