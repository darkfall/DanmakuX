using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

class DMKDanmakuEditor: EditorWindow {

	public static int PreviewTextureWidth = 48;
	public static int PreviewTextureHeight = 48;
	public static int LeftPaneWidth = 154;
	public static int RightPaneColumnWidth = 300;

	public int 			  selectedDanmakuIndex = 0;
	public DMKDanmaku	  selectedDanmaku;

	public DMKController selectedController;
	// used to display names
	public List<string> danmakuNames;

	Vector2			 	shooterScrollPosition;

	public static void Create() {
		DMKDanmakuEditor editor = (DMKDanmakuEditor)EditorWindow.GetWindow<DMKDanmakuEditor>("DanmakuX", true);
		editor.Init();
	}

	public void Init() {
		danmakuNames = new List<string>();
		shooterScrollPosition = new Vector2(0, 0);

		this.OnSelectionChange();
	}

	public void OnFocus() {
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

			selectedDanmakuIndex = -1;
			selectedDanmaku = null;
			if(selectedController != null && selectedController.danmakus.Count > 0) {
				selectedDanmakuIndex = 0;
				selectedDanmaku = selectedController.danmakus[0];
			}
			
			this.Repaint();
		} catch {
			selectedController = null;
			selectedDanmakuIndex = -1;
			selectedDanmaku = null;
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
					style.shooters = new List<DMKBulletShooterController>();

					selectedController.danmakus.Add(style);
					danmakuNames.Add(style.name);

					selectedDanmakuIndex = selectedController.danmakus.Count - 1;
					selectedDanmaku = style;
					selectedDanmaku.parentController = selectedController;
				}
				EditorUtility.SetDirty(this.selectedController.gameObject);
			}
			if(GUILayout.Button("-", "label", GUILayout.Width(30))) {
				if(selectedDanmakuIndex >= 0 && selectedController != null) {
					danmakuNames.RemoveAt(selectedDanmakuIndex);
					selectedController.danmakus.RemoveAt(selectedDanmakuIndex);

					if(selectedDanmakuIndex >= selectedController.danmakus.Count)
						selectedDanmakuIndex = selectedController.danmakus.Count - 1;
					if(selectedController.danmakus.Count == 0)
						selectedDanmakuIndex = -1;
					if(selectedDanmakuIndex != -1)
						selectedDanmaku = selectedController.danmakus[selectedDanmakuIndex];
					else
						selectedDanmaku = null;

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
				selectedDanmakuIndex = DMKGUIUtility.MakeSimpleList(selectedDanmakuIndex, danmakuNames.ToArray());
				//selectedDanmakuIndex = GUILayout.SelectionGrid(selectedDanmakuIndex, danmakuNames.ToArray(), 1);
				if(selectedDanmakuIndex >= 0 && selectedController != null && selectedController.danmakus.Count > 0) {
					selectedDanmaku = selectedController.danmakus[selectedDanmakuIndex];
				}
			}
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

#region shooter tools menu

	DMKBulletShooterController _selectedShooter = null;
	DMKBulletShooterController _copiedShooter = null;

	void OnMenuCopyClicked() {
		_copiedShooter = _selectedShooter;
	}

	void OnMenuPasteClicked() {
		if(_copiedShooter != null &&
		   _copiedShooter != _selectedShooter) {
			_selectedShooter.CopyFrom(_copiedShooter);
		}
	}

	void OnMenuRemoveClicked() {
		if(EditorUtility.DisplayDialog("Remove Shooter", "Are you sure you want to remove this Shooter?", "Yes", "No")) {
			if(_selectedShooter != null)
				selectedDanmaku.shooters.Remove(_selectedShooter);
			_selectedShooter = null;
		}
	}

	void OnAddDeathShooterClicked(object userData) {
		string typeName = userData as String;

		DMKDeathBulletShooterController shooterController = new DMKDeathBulletShooterController();
		shooterController.shooter = ScriptableObject.CreateInstance(typeName) as DMKBulletShooter;
		shooterController.shooter.parentController = shooterController;
		if(shooterController != null) {
			_selectedShooter.deathController = shooterController;
			shooterController.bulletContainer = _selectedShooter.bulletContainer;
			shooterController.parentController = _selectedShooter.parentController;
			shooterController.tag = _selectedShooter.tag;

		}
		EditorUtility.SetDirty(this.selectedController.gameObject);
	}

	void OnRemoveDeathShooterClicked() {
		_selectedShooter.deathController = null;
	}

	void DisplayShooterToolsMenu() {
		GenericMenu menu = new GenericMenu();
		
		menu.AddItem(new GUIContent("Copy"), false, OnMenuCopyClicked);
		if(_copiedShooter != null &&
		   _copiedShooter != _selectedShooter)
			menu.AddItem(new GUIContent("Paste"), false, OnMenuPasteClicked);
		else
			menu.AddDisabledItem(new GUIContent("Paste"));
		menu.AddSeparator("");

		if(_selectedShooter.deathController == null) {
			foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(Type type in asm.GetTypes()) {
					if(type.BaseType == typeof(DMKBulletShooter)) {
						menu.AddItem(new GUIContent("[DeathShooter] Add" + type.ToString()), false, OnAddDeathShooterClicked, type.ToString());
					}
				}
			}
		} else {
			menu.AddItem(new GUIContent("Remove Death Shooter"), false, OnRemoveDeathShooterClicked);
		}

		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Remove"), false, OnMenuRemoveClicked);
		menu.ShowAsContext();
	}

#endregion

#region new shooter menu

	void OnAddShooterClicked(object userData) {
		string shooterTypeName = userData as string;

		DMKBulletShooterController shooterController = new DMKBulletShooterController();
		shooterController.shooter = ScriptableObject.CreateInstance(shooterTypeName) as DMKBulletShooter;

		shooterController.editorExpanded = true;
		shooterController.parentController = selectedController;
		shooterController.gameObject = selectedController.transform.gameObject;
		shooterController.identifier = shooterTypeName;
		
		if(selectedDanmaku.shooters.Count > 0) {
			shooterController.bulletContainer = selectedDanmaku.shooters[0].bulletContainer;
			shooterController.tag = selectedDanmaku.shooters[0].tag;
		}
		selectedDanmaku.shooters.Add( shooterController );
		
		if(selectedController.currentAttackIndex != -1) {
			// playing
			shooterController.enabled = true;
		}
		EditorUtility.SetDirty(this.selectedController.gameObject);
	}
	
	void DisplayNewShooterMenu() {
		GenericMenu menu = new GenericMenu();
		
		foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach(Type type in asm.GetTypes()) {
				if(type.BaseType == typeof(DMKBulletShooter)) {
					menu.AddItem(new GUIContent(type.ToString()), false, OnAddShooterClicked, type.ToString());
				}
			}
		}
		
		menu.ShowAsContext();
	}


#endregion

#region modifier menu

	void OnAddModifierClicked(object userData) {
		string modifierTypeName = userData as string;
		
		DMKShooterModifier modifier = ScriptableObject.CreateInstance(modifierTypeName) as DMKShooterModifier;
		_selectedShooter.shooter.AddModifier(modifier);
	}

	void DisplayNewModifierMenu() {
		GenericMenu menu = new GenericMenu();
		
		foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach(Type type in asm.GetTypes()) {
				if(type.BaseType == typeof(DMKShooterModifier)) {
					menu.AddItem(new GUIContent(type.ToString()), false, OnAddModifierClicked, type.ToString());
				}
			}
		}
		
		menu.ShowAsContext();
	}

#endregion

	void RightPanelGUI() {
		GUILayout.BeginArea(new Rect(LeftPaneWidth, 0, this.position.width - LeftPaneWidth, this.position.height),
		                    "");

		if(selectedController.danmakus.Count == 0) {
			EditorGUILayout.HelpBox("No Danmakus Available", MessageType.Info);
			GUILayout.EndArea();
			return;
		}

		if(selectedDanmaku != null && selectedDanmakuIndex >= 0 && selectedDanmakuIndex < selectedController.danmakus.Count) {
			shooterScrollPosition = GUILayout.BeginScrollView(shooterScrollPosition);

			{
				GUILayout.BeginVertical("box");
				selectedDanmaku.name = EditorGUILayout.TextField("Name", selectedDanmaku.name);
				danmakuNames[selectedDanmakuIndex] = selectedDanmaku.name;

				selectedController.maxBulletCount = EditorGUILayout.IntField("Max Num Bullets", selectedController.maxBulletCount);

				selectedDanmaku.playMode = (DMKDanmakuPlayMode)EditorGUILayout.EnumPopup("Play Mode", selectedDanmaku.playMode);
				if(selectedDanmaku.playMode != DMKDanmakuPlayMode.All) {
					selectedDanmaku.playInterval = (int)Mathf.Clamp(EditorGUILayout.IntField("Interval", selectedDanmaku.playInterval), 0, 999999);
				}

				GUILayout.EndVertical();
				GUILayout.BeginVertical("box");
			}
			{
				GUILayout.BeginHorizontal();
				{

					GUILayout.Label(selectedDanmaku.shooters.Count.ToString() + " Shooters");
					if(GUILayout.Button("+", "label", GUILayout.Width(16))) {
						this.DisplayNewShooterMenu();
					}
				}
				GUILayout.EndHorizontal();
			}
			foreach(DMKBulletShooterController shooter in selectedDanmaku.shooters) {
				GUILayout.BeginVertical("box");
				{
					EditorGUILayout.BeginHorizontal();
					shooter.editorEnabled = EditorGUILayout.Toggle(shooter.editorEnabled, GUILayout.Width(12));
					shooter.enabled = shooter.editorEnabled;
					selectedDanmaku.UpdateShooters();

					string shooterInfoStr = shooter.DMKName();
					if(!shooter.editorExpanded) {
						shooterInfoStr = String.Format("{0} (Start = {1}, Overall Length = {2})", 
						                               shooter.DMKName(),
						                               shooter.startFrame,
						                               shooter.overallLength == 0 ? "INF" : shooter.overallLength.ToString());
					}
					shooter.editorExpanded = EditorGUILayout.Foldout(shooter.editorExpanded, shooterInfoStr);
				
					if(GUILayout.Button("Options", "label", GUILayout.Width(48))) {
						_selectedShooter = shooter;

						this.DisplayShooterToolsMenu();
					}

					EditorGUILayout.EndHorizontal();
				}

				if(shooter.editorExpanded) {
					if(shooter.deathController == null) {
						this.ShooterGUI(shooter);
					} else {
						EditorGUILayout.BeginHorizontal();
						
						EditorGUILayout.BeginVertical(GUILayout.MaxWidth(RightPaneColumnWidth));
						this.ShooterGUI(shooter);
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical("box");
						shooter.deathController.editorExpanded = EditorGUILayout.Foldout(shooter.deathController.editorExpanded, "Death Shooter");
						if(shooter.deathController.editorExpanded)
							this.DeathShooterGUI(shooter.deathController);
						EditorGUILayout.EndVertical();

						EditorGUILayout.EndHorizontal();
					}
				}
				GUILayout.EndVertical();
			}
			
			GUILayout.EndVertical();
			
			GUILayout.EndScrollView();
		}
		
		GUILayout.EndArea();
	}

	void ShooterGUILowerPart_BulletInfo(DMKBulletShooterController shooter) {
		EditorGUILayout.BeginVertical("box");
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			
			string bulletInfoStr = "Bullet Info";
			if(!shooter.editorBulletInfoExpanded) {
				bulletInfoStr = String.Format("Bullet Info (Speed = {0}, Accel = {1}, Lifetime = {2})", 
				                              shooter.bulletInfo.speed.value,
				                              shooter.bulletInfo.accel.value,
				                              shooter.bulletInfo.maxLifetime);
			}
			shooter.editorBulletInfoExpanded = EditorGUILayout.Foldout(shooter.editorBulletInfoExpanded, bulletInfoStr);
			
			if(shooter.editorBulletInfoExpanded) {
				EditorGUILayout.BeginHorizontal();
				
				{
					EditorGUILayout.BeginVertical();
					shooter.bulletInfo.bulletSprite = EditorGUILayout.ObjectField("Sprite", shooter.bulletInfo.bulletSprite, typeof(Sprite), false) as Sprite;
					shooter.bulletInfo.bulletColor  = EditorGUILayout.ColorField("Color", shooter.bulletInfo.bulletColor);
					EditorGUILayout.EndVertical();
				}
				
				GUILayoutOption[] options = {GUILayout.Width(PreviewTextureWidth), GUILayout.Height(PreviewTextureHeight)};
				GUILayout.Label("", "textarea", options);
				
				{
					// here's the trick
					// the begin/endvertical will create a rect on the right side of the panel
					// and with getLastRect we can get that rect
					// thus we can get the position we need to draw the preview texture
					Sprite sprite = shooter.bulletInfo.bulletSprite;
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
							GUI.color = shooter.bulletInfo.bulletColor;
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
		EditorGUILayout.EndVertical();
	}

	void ShooterGUILowerPart_Shooter(DMKBulletShooterController shooter) {
		EditorGUILayout.BeginVertical("box");
		string shooterStr = "Shooter ";
		if(!shooter.editorShooterInfoExpanded)
			shooterStr += shooter.DMKSummary();
		shooter.editorShooterInfoExpanded = EditorGUILayout.Foldout(shooter.editorShooterInfoExpanded, shooterStr);
		if(shooter.editorShooterInfoExpanded) {
			shooter.OnEditorGUI();
		}
		EditorGUILayout.EndVertical();
	}

	void ShooterGUILowerPart_Modifier(DMKBulletShooterController shooter) {
		EditorGUILayout.BeginVertical("box");

		EditorGUILayout.BeginHorizontal();
		shooter.editorModifierExpanded = EditorGUILayout.Foldout(shooter.editorModifierExpanded, "Modifiers");
		if(GUILayout.Button("+", "label", GUILayout.Width(16))) {
			_selectedShooter = shooter;
			this.DisplayNewModifierMenu();
		}
		EditorGUILayout.EndHorizontal();

		DMKShooterModifier modifier = shooter.shooter.modifier;
		while(modifier != null) {
			EditorGUILayout.BeginVertical("box");

			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(modifier.DMKName());
				if(GUILayout.Button("-", "label", GUILayout.Width(16))) {
					shooter.shooter.RemoveModifier(modifier);
				}
				EditorGUILayout.EndHorizontal();
			}

			modifier.OnEditorGUI();
			EditorGUILayout.EndVertical();

			modifier = modifier.next;
		}

		EditorGUILayout.EndVertical();
	}

	void ShooterGUILowerPart(DMKBulletShooterController shooter) {
		ShooterGUILowerPart_BulletInfo(shooter);
		ShooterGUILowerPart_Shooter(shooter);
		ShooterGUILowerPart_Modifier(shooter);
	}

	void DeathShooterGUI(DMKDeathBulletShooterController shooter) {
		EditorGUILayout.BeginVertical();
		{
			EditorGUI.BeginChangeCheck();

			shooter.lifeFrame = EditorGUILayout.IntField("Lifetime (Frame)", shooter.lifeFrame);

			DMKGUIUtility.MakeCurveControl(ref shooter.emissionCooldown, "Emission CD");
			shooter.emissionLength = (int)Mathf.Clamp(EditorGUILayout.IntField("Emission Length", shooter.emissionLength), 0, 999999);
			shooter.interval = (int)Mathf.Clamp(EditorGUILayout.IntField("Emission Interval", shooter.interval), 0, 999999);
			shooter.startFrame = (int)Mathf.Clamp(EditorGUILayout.IntField("Start Frame", shooter.startFrame), 0, 999999);
			shooter.overallLength = (int)Mathf.Clamp(EditorGUILayout.IntField("Overall Length", shooter.overallLength), 0, 999999);
			if(EditorGUI.EndChangeCheck()) {
				shooter.DMKInit();
			}

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Position Offset");
				shooter.positionOffset.type = (DMKPositionOffsetType)EditorGUILayout.EnumPopup(shooter.positionOffset.type);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space (32);
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

	void ShooterGUI(DMKBulletShooterController shooter) {
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
				Debug.Log("test");
				shooter.DMKInit();
			}

			shooter.simulationCount = (int)Mathf.Clamp(EditorGUILayout.IntField("Simulation Count", shooter.simulationCount), 1, 999999);

			
			EditorGUILayout.Space();
			shooter.tag = EditorGUILayout.TextField("Tag", shooter.tag);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Position Offset");
				shooter.positionOffset.type = (DMKPositionOffsetType)EditorGUILayout.EnumPopup(shooter.positionOffset.type);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space (32);
				GUILayout.BeginVertical();
				
				if(shooter.positionOffset.type != DMKPositionOffsetType.Absolute)
					shooter.gameObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", shooter.gameObject, typeof(GameObject), true);

				shooter.positionOffset.OnEditorGUI(false);
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();

		}
		EditorGUILayout.Separator();
		EditorGUILayout.EndVertical();

		ShooterGUILowerPart(shooter);
	}

	void UnavailableGUI() {
		GUILayout.BeginHorizontal("box");
		EditorGUILayout.HelpBox("Add DMKController to start", MessageType.Info);
		GUILayout.EndHorizontal();
	}


};