using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

class DMKDanmakuEditor: EditorWindow {

	public static int PreviewTextureWidth = 48;
	public static int PreviewTextureHeight = 48;
	public static int LeftPaneWidth = 154;

	public int 			  asSelectedIndex = 0;
	public DMKDanmaku	  asSelectedStyle;

	public DMKController selectedController;
	// used to display names
	public List<string> danmakuNames;

	public int 		 	emitterIdx;
	Vector2			 	emitterScrollPosition;

	public static void Create() {
		DMKDanmakuEditor editor = (DMKDanmakuEditor)EditorWindow.GetWindow<DMKDanmakuEditor>("DanmakuX", true);
		editor.Init();
	}

	public void Init() {
		danmakuNames = new List<string>();
		emitterScrollPosition = new Vector2(0, 0);

		this.OnSelectionChange();
	}

	public void OnFocus() {
		this.OnSelectionChange();
	}

	public void OnSelectionChange() {
		try {
			GameObject selectedObject = Selection.activeObject as GameObject;
			danmakuNames.Clear();
			if(selectedObject != null) {
				selectedController = selectedObject.GetComponent<DMKController>();
				if(selectedController != null) {
					if(selectedController.danmakus == null)
						selectedController.danmakus = new List<DMKDanmaku>();
					foreach(DMKDanmaku style in selectedController.danmakus) {
						danmakuNames.Add(style.name);
					}
				}
			} else
				selectedController = null;

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
		DMKGUIUtility.Init();

		GUILayout.BeginArea(new Rect(0, 0, LeftPaneWidth, this.position.height),
		                    "",
		                    DMKGUIUtility.boxNoBackground);
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
					danmakuNames.Add(style.name);

					asSelectedIndex = selectedController.danmakus.Count - 1;
					asSelectedStyle = style;
				}
				EditorUtility.SetDirty(this.selectedController.gameObject);
			}
			if(GUILayout.Button("-", "label", GUILayout.Width(30))) {
				if(asSelectedIndex >= 0 && selectedController != null) {
					danmakuNames.RemoveAt(asSelectedIndex);
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

		GUILayout.Space (2);

		{
			if(danmakuNames.Count > 0) {
				asSelectedIndex = DMKGUIUtility.MakeSimpleList(asSelectedIndex, danmakuNames.ToArray());
				//asSelectedIndex = GUILayout.SelectionGrid(asSelectedIndex, danmakuNames.ToArray(), 1);
				if(asSelectedIndex >= 0 && selectedController != null && selectedController.danmakus.Count > 0) {
					asSelectedStyle = selectedController.danmakus[asSelectedIndex];
				}
			}
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

#region emitter tools menu

	DMKBulletEmitter _selectedEmitter = null;
	DMKBulletEmitter _copiedEmitter = null;

	void OnMenuCopyClicked() {
		_copiedEmitter = _selectedEmitter;
	}

	void OnMenuPasteClicked() {
		if(_copiedEmitter != null &&
		   _copiedEmitter != _selectedEmitter) {
			_selectedEmitter.CopyFrom(_copiedEmitter);
		}
	}

	void OnMenuRemoveClicked() {
		if(EditorUtility.DisplayDialog("Remove Emitter", "Are you sure you want to remove this emitter?", "Yes", "No")) {
			if(_selectedEmitter != null)
				asSelectedStyle.emitters.Remove(_selectedEmitter);
//			if(_selectedEmitter.deathParentEmitter != null) {
//				_selectedEmitter.deathParentEmitter.deathSubEmitter = null;
//				_selectedEmitter.deathParentEmitter.editorDeathSubEmitterIndex = -1;
//			}
			_selectedEmitter = null;
		}
	}

	void DisplayEmitterToolsMenu() {
		GenericMenu menu = new GenericMenu();
		
		menu.AddItem(new GUIContent("Copy"), false, OnMenuCopyClicked);
		if(_copiedEmitter != null &&
		   _copiedEmitter != _selectedEmitter)
			menu.AddItem(new GUIContent("Paste"), false, OnMenuPasteClicked);
		else
			menu.AddDisabledItem(new GUIContent("Paste"));
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Remove"), false, OnMenuRemoveClicked);
		menu.ShowAsContext();
	}

#endregion

#region new emitter menu

	void OnAddEmitterClicked(object userData) {
		string emitterTypeName = userData as string;

		DMKBulletEmitter emitter = ScriptableObject.CreateInstance(emitterTypeName) as DMKBulletEmitter;
		emitter.editorExpanded = true;
		emitter.parentController = selectedController;
		emitter.gameObject = selectedController.transform.gameObject;
		emitter.identifier = emitterTypeName;
		
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
	
	void DisplayNewEmitterMenu() {
		GenericMenu menu = new GenericMenu();
		
		foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach(Type type in asm.GetTypes()) {
				if(type.BaseType == typeof(DMKBulletEmitter)) {
					menu.AddItem(new GUIContent(type.ToString()), false, OnAddEmitterClicked, type.ToString());
				}
			}
		}
		
		menu.ShowAsContext();
	}


#endregion

	string[] BuildSubEmitterList(DMKBulletEmitter currentEmitter) {
		List<string> emitters = new List<string>(asSelectedStyle.emitters.Count);
		emitters.Add("None");
		foreach(DMKBulletEmitter emitter in asSelectedStyle.emitters) {
			if(emitter != currentEmitter)
				emitters.Add (emitter.identifier);
		}
		return emitters.ToArray();
	}

	void RightPanelGUI() {
		GUILayout.BeginArea(new Rect(LeftPaneWidth, 0, this.position.width - LeftPaneWidth, this.position.height),
		                    "");

		if(selectedController.danmakus.Count == 0) {
			EditorGUILayout.HelpBox("No Danmakus Available", MessageType.Info);
			GUILayout.EndArea();
			return;
		}

		if(asSelectedStyle != null && asSelectedIndex >= 0 && asSelectedIndex < selectedController.danmakus.Count) {
			emitterScrollPosition = GUILayout.BeginScrollView(emitterScrollPosition);
			GUILayout.BeginVertical("box");
			asSelectedStyle.name = EditorGUILayout.TextField("Name", asSelectedStyle.name);
			danmakuNames[asSelectedIndex] = asSelectedStyle.name;

			selectedController.maxBulletCount = EditorGUILayout.IntField("Max Num Bullets", selectedController.maxBulletCount);
			GUILayout.EndVertical();
			GUILayout.BeginVertical("box");
			GUILayout.BeginHorizontal();
			{

				GUILayout.Label(asSelectedStyle.emitters.Count.ToString() + " Emitters");
				if(GUILayout.Button("+", "label", GUILayout.Width(16))) {
					this.DisplayNewEmitterMenu();
				}
			}
			GUILayout.EndHorizontal();
			foreach(DMKBulletEmitter emitter in asSelectedStyle.emitters) {
				GUILayout.BeginVertical("box");
				{
					EditorGUILayout.BeginHorizontal();
					emitter.editorEnabled = EditorGUILayout.Toggle(emitter.editorEnabled, GUILayout.Width(12));
					emitter.enabled = emitter.editorEnabled;

					emitter.editorExpanded = EditorGUILayout.Foldout(emitter.editorExpanded, emitter.DMKName());
					/*if(!emitter.editorExpanded) {
						GUILayout.Label("Identifier", GUILayout.Width(54));
						emitter.identifier = EditorGUILayout.TextField(emitter.identifier, GUILayout.MaxWidth(120));
					}*/
					if(GUILayout.Button("Options", "label", GUILayout.Width(48))) {
						_selectedEmitter = emitter;
						this.DisplayEmitterToolsMenu();
					}

					EditorGUILayout.EndHorizontal();
				}

				if(emitter.editorExpanded) {
					EditorGUILayout.BeginVertical();
					{
						//emitter.identifier = EditorGUILayout.TextField("Identifier", emitter.identifier);
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

						GUILayout.BeginHorizontal();
						{
							GUILayout.Label("Position Offset");
							emitter.usePositionOffCurve = DMKGUIUtility.MakeCurveToggle(emitter.usePositionOffCurve);
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						{
							GUILayout.Space (32);
							GUILayout.BeginVertical();
							
							if(emitter.usePositionOffCurve) {
								emitter.positionOffsetX	= EditorGUILayout.CurveField("X", emitter.positionOffsetX);
								emitter.positionOffsetY	= EditorGUILayout.CurveField("Y", emitter.positionOffsetY);
							} else {
								emitter.positionOffset = EditorGUILayout.Vector2Field("", emitter.positionOffset);
							}
							GUILayout.EndVertical();
						}
						GUILayout.EndHorizontal();

						/*
						emitter.editorDeathSubEmitterIndex = EditorGUILayout.Popup("Death SubEmitter", emitter.editorDeathSubEmitterIndex, BuildSubEmitterList(emitter));
						if(emitter.editorDeathSubEmitterIndex > 0) {
							if(emitter.deathSubEmitter != null)
								emitter.deathSubEmitter.deathParentEmitter = null;
							emitter.deathSubEmitter = asSelectedStyle.emitters[emitter.editorDeathSubEmitterIndex];
							emitter.deathSubEmitter.deathParentEmitter = emitter;
						} else
							emitter.editorDeathSubEmitterIndex = -1;
						if(emitter.deathParentEmitter != null)
							EditorGUILayout.LabelField("Parent Emitter", emitter.deathParentEmitter.identifier);
						*/
					}
					EditorGUILayout.Separator();
					EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical("box");
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.BeginVertical();

						string bulletInfoStr = "Bullet Info";
						if(!emitter.editorBulletInfoExpanded) {
							bulletInfoStr = String.Format("Bullet Info (Speed = {0}, Accel = {1}, Lifetime = {2})", 
							                              emitter.bulletInfo.speed.value,
							                              emitter.bulletInfo.accel.value,
							                              emitter.bulletInfo.maxLifetime);
						}
						emitter.editorBulletInfoExpanded = EditorGUILayout.Foldout(emitter.editorBulletInfoExpanded, bulletInfoStr);

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
								
							emitter.bulletInfo.damage = EditorGUILayout.IntField("Damage", emitter.bulletInfo.damage);
							emitter.bulletInfo.maxLifetime = EditorGUILayout.IntField("Lifetime (Frame)", emitter.bulletInfo.maxLifetime);
						}
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
					
					EditorGUILayout.BeginVertical("box");
					string emitterStr = "Emitter ";
					if(!emitter.editorEmitterInfoExpanded)
						emitterStr += emitter.DMKSummary();
					emitter.editorEmitterInfoExpanded = EditorGUILayout.Foldout(emitter.editorEmitterInfoExpanded, emitterStr);
					if(emitter.editorEmitterInfoExpanded) {
						emitter.OnEditorGUI();
					}
					EditorGUILayout.EndVertical();
				}
				GUILayout.EndVertical();
			}

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