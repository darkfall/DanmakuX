using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

class DMKDanmakuEditor: EditorWindow {

	public static int PreviewTextureWidth = 48;
	public static int PreviewTextureHeight = 48;

	public int 			  asSelectedIndex = 0;
	public DMKDanmaku	  asSelectedStyle;

	public DMKController selectedController;
	// used to display names
	public List<string> availableAttackStyleNames;
	
	public List<string> bulletEmitterNames;
	public int 		 	emitterIdx;
	Vector2			 	emitterScrollPosition;

	public static void Create() {
		DMKDanmakuEditor editor = (DMKDanmakuEditor)EditorWindow.GetWindow<DMKDanmakuEditor>("DanmakuX", true);
		editor.Init();
	}

	public void Init() {
		availableAttackStyleNames = new List<string>();
		emitterScrollPosition = new Vector2(0, 0);

		this.OnSelectionChange();
	}

	public void OnSelectionChange() {
		try {
			GameObject selectedObject = Selection.activeObject as GameObject;
			availableAttackStyleNames.Clear();
			if(selectedObject != null) {
				selectedController = selectedObject.GetComponent<DMKController>();
				if(selectedController != null) {
					if(selectedController.danmakus == null)
						selectedController.danmakus = new List<DMKDanmaku>();
					foreach(DMKDanmaku style in selectedController.danmakus) {
						availableAttackStyleNames.Add(style.name);
					}
				}
			} else
				selectedController = null;

			bulletEmitterNames = new List<string>();
			
			foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(Type type in asm.GetTypes()) {
					if(type.BaseType == typeof(DMKBulletEmitter)) {
						bulletEmitterNames.Add(type.ToString());
					}
				}
			}
			
			asSelectedIndex = -1;
			asSelectedStyle = null;
			if(selectedController != null && selectedController.danmakus.Count > 0) {
				asSelectedIndex = 0;
				asSelectedStyle = selectedController.danmakus[0];
			}
			
			this.Repaint();
		} catch {
			selectedController = null;
			asSelectedIndex = -1;
			asSelectedStyle = null;
		}

	}

	void LeftPanelGUI() {
		GUILayout.BeginArea(new Rect(0, 0, 160, this.position.height),
		                    "",
		                    "box");
		GUILayout.BeginVertical(GUILayout.Width(150));

		{
			GUILayout.BeginHorizontal("box");
			if(GUILayout.Button("+", "label", GUILayout.Width(30))) {
				if(Selection.activeObject != null && selectedController == null) {
					selectedController = ((GameObject)Selection.activeObject).AddComponent<DMKController>();
					if(selectedController.danmakus == null)
						selectedController.danmakus = new List<DMKDanmaku>();
				}
				if(selectedController != null) {
					DMKDanmaku style = new DMKDanmaku();
					style.name = "New Danmaku";
					style.emitters = new List<DMKBulletEmitter>();

					selectedController.danmakus.Add(style);
					availableAttackStyleNames.Add(style.name);

					asSelectedIndex = selectedController.danmakus.Count - 1;
					asSelectedStyle = style;
				}
				EditorUtility.SetDirty(this.selectedController.gameObject);
			}
			if(GUILayout.Button("-", "label", GUILayout.Width(30))) {
				if(asSelectedIndex >= 0 && selectedController != null) {
					availableAttackStyleNames.RemoveAt(asSelectedIndex);
					selectedController.danmakus.RemoveAt(asSelectedIndex);

					if(asSelectedIndex >= selectedController.danmakus.Count)
						asSelectedIndex = selectedController.danmakus.Count - 1;
					if(selectedController.danmakus.Count == 0)
						asSelectedIndex = -1;
					if(asSelectedIndex != -1)
						asSelectedStyle = selectedController.danmakus[asSelectedIndex];
					else
						asSelectedStyle = null;

					this.Repaint();
					EditorUtility.SetDirty(this.selectedController.gameObject);
				}
			}
			GUILayout.Label("Danmaku");
			GUILayout.EndHorizontal();
		}

		GUILayout.Space (6);

		{
			GUILayout.BeginHorizontal("box");
			asSelectedIndex = GUILayout.SelectionGrid(asSelectedIndex, availableAttackStyleNames.ToArray(), 1);
			if(asSelectedIndex >= 0 && selectedController != null && selectedController.danmakus.Count > 0) {
				asSelectedStyle = selectedController.danmakus[asSelectedIndex];
			}
			GUILayout.EndHorizontal();
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	void CreateOptionalCurveField(ref bool useCurve, ref float fVal, ref AnimationCurve cVal, string label) {
		EditorGUILayout.BeginHorizontal();
		if(useCurve) {
			cVal = EditorGUILayout.CurveField(label, cVal);
		} else {
			fVal = EditorGUILayout.FloatField(label, fVal);
		}
		GUILayout.Label("Curve", GUILayout.Width(40));
		useCurve = EditorGUILayout.Toggle("", useCurve, GUILayout.Width(16));
		EditorGUILayout.EndHorizontal();
	}

#region emitter tools menu

	int _copiedEmitterIndex = -1;

	void OnMenuCopyClicked() {

	}

	void OnMenuPasteClicked() {

	}

	void OnMenuRemoveClicked() {

	}

	void DisplayEmitterToolsMenu() {
		GenericMenu menu = new GenericMenu();
		
		menu.AddItem(new GUIContent("Copy"), false, OnMenuCopyClicked);
		if(_copiedEmitterIndex != -1)
			menu.AddItem(new GUIContent("Paste"), false, OnMenuPasteClicked);
		else
			menu.AddDisabledItem(new GUIContent("Paste"));
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Remove"), false, OnMenuRemoveClicked);
		menu.ShowAsContext();
	}
#endregion

	void RightPanelGUI() {
		GUILayout.BeginArea(new Rect(160, 0, this.position.width - 160, this.position.height),
		                    "",
		                    "box");

		if(asSelectedStyle != null && asSelectedIndex < selectedController.danmakus.Count) {
			emitterScrollPosition = GUILayout.BeginScrollView(emitterScrollPosition);

			GUILayout.BeginVertical("box");
			asSelectedStyle.name = EditorGUILayout.TextField("Name", asSelectedStyle.name);
			availableAttackStyleNames[asSelectedIndex] = asSelectedStyle.name;

			selectedController.maxBulletCount = EditorGUILayout.IntField("Max Num Bullets", selectedController.maxBulletCount);
			GUILayout.EndVertical();

			GUILayout.BeginVertical("box");
			GUILayout.Label(asSelectedStyle.emitters.Count.ToString() + " Emitters");

			foreach(DMKBulletEmitter emitter in asSelectedStyle.emitters) {
				GUILayout.BeginVertical("box");
				{
					EditorGUILayout.BeginHorizontal();
					emitter.editorEnabled = EditorGUILayout.Toggle(emitter.editorEnabled, GUILayout.Width(12));
					emitter.enabled = emitter.editorEnabled;

					emitter.editorExpanded = EditorGUILayout.Foldout(emitter.editorExpanded, emitter.DMKName());

					if(GUILayout.Button("Options", "label", GUILayout.Width(48))) {
						this.DisplayEmitterToolsMenu();
					}

					EditorGUILayout.EndHorizontal();
				}

				if(emitter.editorExpanded) {
					EditorGUILayout.BeginVertical();
					{
						emitter.bulletContainer = (GameObject)EditorGUILayout.ObjectField("Bullet Container", emitter.bulletContainer, typeof(GameObject), true);
						emitter.cooldown 	= EditorGUILayout.IntField("Cooldown", emitter.cooldown);
						if(emitter.cooldown < 0)
							emitter.cooldown = 0;
						emitter.length		= EditorGUILayout.IntField("Length", emitter.length);
						if(emitter.length < 0)
							emitter.length = 0;
						emitter.interval	= EditorGUILayout.IntField("Interval", emitter.interval);
						if(emitter.interval < 0)
							emitter.interval = 0;
						EditorGUILayout.Space();
						emitter.tag  	 	= EditorGUILayout.TextField("Tag", emitter.tag);
						emitter.gameObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", emitter.gameObject, typeof(GameObject), true);
						emitter.positionOffset = EditorGUILayout.Vector3Field("Position Offset", emitter.positionOffset);
					}
					EditorGUILayout.Separator();
					EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical("box");
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.BeginVertical();
						
						emitter.editorBulletInfoExpanded = EditorGUILayout.Foldout(emitter.editorBulletInfoExpanded, "Bullet Info");

						if(emitter.editorBulletInfoExpanded) {
							EditorGUILayout.BeginHorizontal();

							{
								EditorGUILayout.BeginVertical();
								emitter.bulletInfo.bulletSprite = EditorGUILayout.ObjectField("Sprite", emitter.bulletInfo.bulletSprite, typeof(Sprite), false) as Sprite;
								emitter.bulletInfo.bulletColor  = EditorGUILayout.ColorField("Color", emitter.bulletInfo.bulletColor);
								EditorGUILayout.EndVertical();
							}

							GUILayoutOption[] options = {GUILayout.Width(PreviewTextureWidth), GUILayout.Height(PreviewTextureHeight)};
							GUILayout.Label("", "textarea", options);
							
							{
								// here's the trick
								// the begin/endvertical will create a rect on the right side of the panel
								// and with getLastRect we can get that rect
								// thus we can get the position we need to draw the preview texture
								Sprite sprite = emitter.bulletInfo.bulletSprite;
								if(sprite != null) {
									Texture2D tex = sprite.texture;
									if(tex != null) {
										Rect r = GUILayoutUtility.GetLastRect();
										
										r.x += Mathf.Clamp((PreviewTextureWidth - sprite.rect.width)/2, 0, PreviewTextureWidth);
										r.y += Mathf.Clamp((PreviewTextureHeight - sprite.rect.height)/2, 0, PreviewTextureHeight);
										r.width = sprite.rect.width;
										r.height = sprite.rect.height;
										
										Rect texR = new Rect((float)sprite.rect.x / tex.width,
										                     (float)sprite.rect.y / tex.height,
										                     (float)sprite.rect.width / tex.width,
										                     (float)sprite.rect.height / tex.height);
										Color c = GUI.color;
										GUI.color = emitter.bulletInfo.bulletColor;
										GUI.DrawTextureWithTexCoords(r, 
										                             tex, 
										                             texR,
										                             true);
										GUI.color = c;
									}
								}
							}
							
							EditorGUILayout.EndHorizontal();
							
							EditorGUILayout.Separator();
							
							DMKBulletInfoInternal bulletInfo = emitter.bulletInfo;
							CreateOptionalCurveField(ref bulletInfo.useSpeedCurve, 
							                         ref bulletInfo.speed,
							                         ref bulletInfo.speedCurve, 
							                         "Speed");
							CreateOptionalCurveField(ref bulletInfo.useAccelCurve, 
							                         ref bulletInfo.accel,
							                         ref bulletInfo.accelCurve, 
							                         "Acceleration");
							CreateOptionalCurveField(ref bulletInfo.useAngularAccelCurve, 
							                         ref bulletInfo.angularAccel,
							                         ref bulletInfo.angularAccelCurve, 
							                         "Angular Accel");
							
							GUILayout.BeginHorizontal();
								GUILayout.Label("Scale");
								GUILayout.Label("Curve", GUILayout.Width(40));
								bulletInfo.useScaleCurve = EditorGUILayout.Toggle("", bulletInfo.useScaleCurve, GUILayout.Width(16));
							GUILayout.EndHorizontal();
							
							GUILayout.BeginHorizontal();
								GUILayout.Space (32);
								GUILayout.BeginVertical();
								
								if(bulletInfo.useScaleCurve) {
									bulletInfo.scaleCurveX = EditorGUILayout.CurveField("Scale X", bulletInfo.scaleCurveX);
									bulletInfo.scaleCurveY = EditorGUILayout.CurveField("Scale Y", bulletInfo.scaleCurveY);
								} else {
									bulletInfo.scale = EditorGUILayout.Vector2Field("", bulletInfo.scale);
								}
								GUILayout.EndVertical();
							GUILayout.EndHorizontal();
								
							emitter.bulletInfo.damage = EditorGUILayout.IntField("Damage", emitter.bulletInfo.damage);
						}
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.BeginVertical("box");
					emitter.editorEmitterInfoExpanded = EditorGUILayout.Foldout(emitter.editorEmitterInfoExpanded, "Emitter");
					if(emitter.editorEmitterInfoExpanded) {
						emitter.OnEditorGUI();
					}
					EditorGUILayout.EndVertical();
				}
				GUILayout.EndVertical();
			}

			GUILayout.BeginHorizontal("box");
			GUILayout.Label("New Emitter", GUILayout.Width(80));
			emitterIdx = EditorGUILayout.Popup("", emitterIdx, bulletEmitterNames.ToArray());
			
			if(GUILayout.Button("+", "label", GUILayout.Width(12))) {
				DMKBulletEmitter emitter = ScriptableObject.CreateInstance(bulletEmitterNames[emitterIdx]) as DMKBulletEmitter;
				emitter.editorExpanded = true;
				emitter.parentController = selectedController;
				emitter.gameObject = selectedController.transform.gameObject;
				if(asSelectedStyle.emitters.Count > 0) {
					emitter.bulletContainer = asSelectedStyle.emitters[0].bulletContainer;
					emitter.tag = asSelectedStyle.emitters[0].tag;
				}
				asSelectedStyle.emitters.Add( emitter );

				if(selectedController.currentAttackIndex != -1) {
					// playing
					emitter.enabled = true;
				}

				EditorUtility.SetDirty(this.selectedController.gameObject);
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			
			GUILayout.EndScrollView();
		}

		GUILayout.EndArea();

	}

	void UnavailableGUI() {
		GUILayout.BeginHorizontal("box");
		EditorGUILayout.HelpBox("Add DMKController to start", MessageType.Info);
		GUILayout.EndHorizontal();
	}

	void OnGUI() {
		EditorGUIUtility.labelWidth = 100;
		if(selectedController != null) {
			this.LeftPanelGUI();
			this.RightPanelGUI(); 

		} else {
			this.UnavailableGUI();
		}
	}

};