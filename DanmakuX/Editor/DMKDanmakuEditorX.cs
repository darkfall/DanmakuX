using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

class DMKDanmakuEditorX: EditorWindow {
	
	public static int PreviewTextureWidth = 48;
	public static int PreviewTextureHeight = 48;

	public static int LeftPaneWidth = 154;
	public static int InspectorWidth = 300;
	public static int ActionBarHeight = 24;
	
	public static int DanmakuListWindowId = 999;
	public static int DanmakuListWindowWidth = 240;
	public static int DanmakuListWindowHeight = 120;

	public static int ShooterGraphWindowWidth = 120;
	public static int ShooterModifierGraphWindowWidth = 100;
	public static int ShooterGraphWindowHeight = 40;
	public static int ShooterModifierGraphWindowHeight = 32;

	public static int ShooterModifierWindowIdStartIndex = 100;
	
	DMKController selectedController;

	DMKDanmaku	 			 	selectedDanmaku;
	DMKBulletShooterController 	selectedShooter = null;
	DMKBulletShooterController 	copiedShooter = null;
	DMKShooterModifier			selectedModifier = null;

	Vector2 inspectorScrollPosition = new Vector2(0, 0);
	Vector2 danmakuListScrollPosition = new Vector2(0, 0);
	Vector2 shooterGraphScrollPosition = new Vector2(0, 0);

	Dictionary<int, DMKShooterModifier> modifierDict = new Dictionary<int, DMKShooterModifier>();
	
	public static void Create() {
		DMKDanmakuEditorX editor = (DMKDanmakuEditorX)EditorWindow.GetWindow<DMKDanmakuEditorX>("DanmakuX", true);
		editor.Init();
	}
	
	public void Init() {
		this.minSize = new Vector2(InspectorWidth + DanmakuListWindowWidth + 20,
		                           DanmakuListWindowHeight + ActionBarHeight + 200);
	}
	
	public void OnFocus() {
	}

	void UnavailableGUI() {
		GUILayout.BeginHorizontal("box");
		EditorGUILayout.HelpBox("Add DMKController to start", MessageType.Info);
		GUILayout.EndHorizontal();
	}

	void OnGUI() {
		EditorGUIUtility.labelWidth = 100;
		if(selectedController != null) {
			this.BeginWindows();
			this.DanmakuListGUI();
			this.ActionBarGUI();
			this.ShooterGraphGUI();
			this.InspectorGUI();
			this.EndWindows();
		} else {
			this.UnavailableGUI();
		}
	}
	
	public void OnSelectionChange() {
		try {
			GameObject selectedObject = Selection.activeObject as GameObject;
			if(selectedObject != null) {
				selectedController = selectedObject.GetComponent<DMKController>();
				if(selectedController != null) {
					if(selectedController.danmakus == null)
						selectedController.danmakus = new List<DMKDanmaku>();
				}
			} else
				selectedController = null;

			selectedDanmaku = null;
			if(selectedController != null && selectedController.danmakus.Count > 0) {
				selectedDanmaku = selectedController.danmakus[0];
			}
			
			this.Repaint();
		} catch {
			selectedController = null;
			selectedDanmaku = null;
		}
	}

	string[] MakeDanmakuPopup() {
		string[] danmakuNames = {};
		foreach(DMKDanmaku danmaku in selectedController.danmakus) {
			ArrayUtility.Add(ref danmakuNames, danmaku.name);
		}
		ArrayUtility.Add(ref danmakuNames, "----------");
		ArrayUtility.Add(ref danmakuNames, "New Danmaku");
		return danmakuNames;
	}

	void CreateNewDanmaku() {
		DMKDanmaku danmaku = new DMKDanmaku();
		danmaku.name = "New Danmaku";
		danmaku.shooters = new List<DMKBulletShooterController>();
		
		selectedController.danmakus.Add(danmaku);

		selectedDanmaku = danmaku;
		selectedDanmaku.parentController = selectedController;
	}

	void ActionBarGUI() {
		GUILayout.BeginArea(new Rect(0, this.position.height - ActionBarHeight, this.position.width, ActionBarHeight), GUI.skin.box);
		GUILayout.BeginHorizontal();
		{
			if(GUILayout.Button("New Shooter", GUILayout.Width(100))) {
				this.DisplayNewShooterMenu();
			}
			if(GUILayout.Button("New Modifier", GUILayout.Width(100))) {

			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	void DanmakuGUI(DMKDanmaku danmaku) {
		danmaku.name = EditorGUILayout.TextField("Name", danmaku.name);
		selectedController.maxBulletCount = EditorGUILayout.IntField("Max Num Bullets", selectedController.maxBulletCount);

		EditorGUI.BeginChangeCheck();
		danmaku.playMode = (DMKDanmakuPlayMode)EditorGUILayout.EnumPopup("Play Mode", danmaku.playMode);
		if(EditorGUI.EndChangeCheck()) 
			danmaku.UpdateCurrentShooter();

		if(danmaku.playMode != DMKDanmakuPlayMode.All) {
			danmaku.playInterval = (int)Mathf.Clamp(EditorGUILayout.IntField("Interval", danmaku.playInterval), 0, 999999);
		}
	}

	void OnDanmakuListWindow(int id) {
		GUILayout.BeginHorizontal();

		GUILayout.EndHorizontal();

		GUI.Label(new Rect(2, 0, 60, 16),
		          "Danmakus");
		if(GUI.Button(new Rect(DanmakuListWindowWidth - 20, 
		                       0,
		                       24,
		                       16),
		              "+",
		              "label")) {
			this.CreateNewDanmaku();
		}

		danmakuListScrollPosition = GUILayout.BeginScrollView(danmakuListScrollPosition);
		DMKDanmaku danmakuToRemove = null;
		for(int i=0; i<selectedController.danmakus.Count; ++i) {
			DMKDanmaku danmaku = selectedController.danmakus[i];

			GUILayout.BeginVertical();

			{
				GUILayout.BeginHorizontal();
				GUI.SetNextControlName("danmaku_" + i.ToString());

				if(selectedDanmaku == danmaku) {
					GUIStyle s = new GUIStyle(EditorStyles.foldout);
					s.normal.textColor = s.onNormal.textColor = s.onFocused.textColor;
					
					danmaku.editorExpanded = EditorGUILayout.Foldout(danmaku.editorExpanded, danmaku.name, s);

				} else {
					danmaku.editorExpanded = EditorGUILayout.Foldout(danmaku.editorExpanded, danmaku.name);
				}

				if(GUILayout.Button("-", "label", GUILayout.Width(12))) {
					danmakuToRemove = danmaku;
				}

				GUILayout.EndHorizontal();
			}
			if(danmaku.editorExpanded) {
				this.DanmakuGUI(danmaku);
			}

			GUILayout.EndVertical();
		}
		if(danmakuToRemove != null) {
			if(selectedDanmaku == danmakuToRemove) {
				selectedDanmaku = null;
			}
			selectedController.danmakus.Remove(danmakuToRemove);
			this.Repaint();
			return;
		}

		for(int i=0; i<selectedController.danmakus.Count; ++i) {
			DMKDanmaku danmaku = selectedController.danmakus[i];

			if(GUI.GetNameOfFocusedControl() == "danmaku_" + i.ToString()) {
				if(selectedDanmaku != danmaku) {
					selectedDanmaku = danmaku;
					selectedShooter = null;
				}
			}
		}

		GUILayout.EndScrollView();
	}

	void DanmakuListGUI() {
		GUIStyle s = new GUIStyle(GUI.skin.window);
		s.onNormal.background = s.normal.background;
	
		GUI.Window(DanmakuListWindowId, 
		           new Rect(0, 0, DanmakuListWindowWidth, DanmakuListWindowHeight), 
		           OnDanmakuListWindow, 
		           "",
		           s);
	}

	void OnShooterWindow(int id) {
		if ((Event.current.button == 0) && (Event.current.type == EventType.MouseDown)) {
			GUI.FocusWindow(id);
			selectedShooter = selectedDanmaku.shooters[id];
			selectedModifier = null;

			Event.current.Use();
		}

		DMKBulletShooterController shooter = selectedDanmaku.shooters[id];
		GUI.Label(new Rect(2, 0, ShooterGraphWindowWidth - 30, 16), shooter.DMKName());

		if(GUI.Button(new Rect(ShooterGraphWindowWidth - 18, 0, 16, 16), 
		              Resources.LoadAssetAtPath<Texture2D>("Assets/Scripts/DanmakuX/Editor/Resources/Icons/Settings.png"), 
		              GUI.skin.label)) {
			this.DisplayShooterToolsMenu();
		}

		Sprite sprite = shooter.bulletInfo.bulletSprite;
		if(sprite != null) {
			Texture2D tex = sprite.texture;
			if(tex != null) {
				Rect sr = new Rect(Mathf.Clamp((ShooterGraphWindowWidth - sprite.rect.width)/2, 0, ShooterGraphWindowWidth/2),
				                   16 + Mathf.Clamp((ShooterGraphWindowHeight - 16 - sprite.rect.height)/2, 0, ShooterGraphWindowHeight/2),
				                   sprite.rect.width,
				                   sprite.rect.height);
				
				Rect texR = new Rect((float)sprite.rect.x / tex.width,
				                     (float)sprite.rect.y / tex.height,
				                     (float)sprite.rect.width / tex.width,
				                     (float)sprite.rect.height / tex.height);
				Color c = GUI.color;
				GUI.color = shooter.bulletInfo.bulletColor;
				GUI.DrawTextureWithTexCoords(sr, 
				                             tex, 
				                             texR,
				                             true);
				GUI.color = c;
			}
		}
	
		GUI.DragWindow();
	}

	void OnShooterModifierWindow(int id) {
		if ((Event.current.button == 0) && (Event.current.type == EventType.MouseDown)) {
			GUI.FocusWindow(id);
			selectedModifier = modifierDict[id];
			selectedShooter = null;

			Event.current.Use();
		}

		DMKShooterModifier modifier = modifierDict[id];
		GUIStyle s = new GUIStyle(GUI.skin.label);
		s.fontSize = 12;
		s.normal.textColor = new Color(255, 255, 255, 1);
		s.alignment = TextAnchor.MiddleCenter;
		GUI.Label(new Rect(0, 0, ShooterModifierGraphWindowWidth, ShooterModifierGraphWindowHeight),
		          modifier.DMKName(),
		          s);

	}

	Rect ClampEditorWindowRect(Rect r) {
		if(r.x < 0)
			r.x = 0;
		else if(r.x + r.width > this.position.width - InspectorWidth)
			r.x = this.position.width - InspectorWidth - r.width;

		if(r.y < 0)
			r.y = 0;
		else if(r.y + r.height > this.position.height - ActionBarHeight)
			r.y = this.position.height - r.height - ActionBarHeight;
		return r;
	}

	void ShooterGraphGUI() {
		GUILayout.BeginArea(new Rect(0, 0, this.position.width - InspectorWidth, this.position.height - ActionBarHeight));

		if(selectedController.danmakus.Count == 0) {
			EditorGUILayout.HelpBox("No Danmakus Available", MessageType.Info);
			GUILayout.EndArea();
			return;
		}

		if(selectedDanmaku != null) {
			Rect windowRect = new Rect(0, 0, ShooterGraphWindowWidth, ShooterGraphWindowHeight);
			int heightRequired = selectedDanmaku.shooters.Count * (ShooterGraphWindowHeight + 40) - 40;

			// to do, scroll width required?
			shooterGraphScrollPosition =  GUI.BeginScrollView(new Rect(0, DanmakuListWindowHeight, this.position.width - InspectorWidth, this.position.height - DanmakuListWindowHeight),
			                    							  shooterGraphScrollPosition,
			                                                  new Rect(0, 0, this.position.width - InspectorWidth, heightRequired));

			int center = (int) (this.position.height - DanmakuListWindowHeight) / 2;
			windowRect.y = center - (heightRequired - (ShooterGraphWindowHeight + 40)) / 2;
			windowRect.x = 40;

			for(int i=0; i<selectedDanmaku.shooters.Count; ++i) {
				DMKBulletShooterController shooter = selectedDanmaku.shooters[i];
				/*
				shooter.editorWindowRect = ClampEditorWindowRect(GUI.Window(i, 
					                                                        shooter.editorWindowRect, OnShooterWindow, 
				                                                            "", 
				                                                            selectedShooter == shooter ? (GUIStyle)"flow node 0 on" : (GUIStyle)"flow node 0"));
				*/
				GUI.Window(i, 
				           windowRect, 
				           OnShooterWindow, 
				           "", 
				           selectedShooter == shooter ? (GUIStyle)"flow node 0 on" : (GUIStyle)"flow node 0");

				DMKShooterModifier modifier = shooter.shooter.modifier;
				int modifierIdx = 0;
				while(modifier != null) {
					Rect mr = new Rect(windowRect.x + ShooterGraphWindowWidth + 80,
					                   windowRect.y,
					                   ShooterModifierGraphWindowWidth,
					                   ShooterModifierGraphWindowHeight);
					mr.y += (ShooterGraphWindowHeight - ShooterModifierGraphWindowHeight) / 2;

					int id = i * 10 + ShooterModifierWindowIdStartIndex + modifierIdx;
					GUI.Window(id, 
					           mr, 
					           OnShooterModifierWindow, 
					           "", 
					           selectedModifier == modifier ? (GUIStyle)"flow node 1 on" : (GUIStyle)"flow node 1");
					modifierDict[id] = modifier;


					Vector3 start = new Vector3(windowRect.x + windowRect.width,
					                            windowRect.y + windowRect.height / 2 - DanmakuListWindowHeight);
					Vector3 end = new Vector3(mr.x,
					                          mr.y + mr.height / 2 - DanmakuListWindowHeight);
					
					Handles.DrawBezier(start,
					                   end ,
					                   start + Vector3.right * 20,
					                   end - Vector3.right * 20,
					                   Color.yellow,
					                   null,
					                   3);

					mr.x += (ShooterModifierGraphWindowWidth + 80);
					modifier = modifier.next;
					modifierIdx += 1;
				}

				windowRect.y += (ShooterGraphWindowHeight + 40);
			}

			GUI.EndScrollView();

			if ((Event.current.button == 0) && (Event.current.type == EventType.mouseDown)) {
				GUI.FocusWindow(-1);
				selectedShooter = null;
				selectedModifier = null;
				this.Repaint();
			}
		}
		/*
		if(selectedDanmaku.shooters.Count >= 2) {
			for(int i=0;i<selectedDanmaku.shooters.Count; ++i) {
				if(i == selectedDanmaku.shooters.Count - 1)
					continue;

				DMKBulletShooterController shooter1 = selectedDanmaku.shooters[i];
				DMKBulletShooterController shooter2 = selectedDanmaku.shooters[i+1];
				Vector3 start = new Vector3(shooter1.editorWindowRect.x + shooter1.editorWindowRect.width / 2,
				                            shooter1.editorWindowRect.y + shooter1.editorWindowRect.height);
				Vector3 end = new Vector3(shooter2.editorWindowRect.x + shooter2.editorWindowRect.width / 2,
				                          shooter2.editorWindowRect.y);
				
				Handles.DrawBezier(start,
				                   end ,
				                   start + Vector3.up * 20,
				                   end - Vector3.up * 20,
				                   Color.green,
				                   null,
				                   3);
			}
		}
		  */                 
		GUILayout.EndArea();
	}

	void InspectorGUI() {

		if(selectedShooter != null || selectedModifier != null) {
			GUILayout.BeginArea(new Rect(this.position.width - DMKDanmakuEditorX.InspectorWidth, 0, DMKDanmakuEditorX.InspectorWidth, this.position.height - ActionBarHeight));
			
			EditorGUILayout.BeginVertical("box");
			inspectorScrollPosition = GUILayout.BeginScrollView(inspectorScrollPosition);

			if(selectedShooter != null)
				this.ShooterGUI(selectedShooter);
			else if(selectedModifier != null)
				selectedModifier.OnEditorGUI();

			GUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			
			GUILayout.EndArea();
		}
		
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

	
	void ShooterGUILowerPart(DMKBulletShooterController shooter) {
		ShooterGUILowerPart_BulletInfo(shooter);
		ShooterGUILowerPart_Shooter(shooter);
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
		selectedShooter.shooter.AddModifier(modifier);
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

	
	#region shooter tools menu

	void OnMenuCopyClicked() {
		copiedShooter = selectedShooter;
	}
	
	void OnMenuPasteClicked() {
		if(copiedShooter != null &&
		   copiedShooter != selectedShooter) {
			selectedShooter.CopyFrom(copiedShooter);
		}
	}
	
	void OnMenuRemoveClicked() {
		if(EditorUtility.DisplayDialog("Remove Shooter", "Are you sure you want to remove this Shooter?", "Yes", "No")) {
			if(selectedShooter != null)
				selectedDanmaku.shooters.Remove(selectedShooter);
			selectedShooter = null;
		}
	}
	
	void OnAddDeathShooterClicked(object userData) {
		string typeName = userData as String;
		
		DMKDeathBulletShooterController shooterController = new DMKDeathBulletShooterController();
		shooterController.shooter = ScriptableObject.CreateInstance(typeName) as DMKBulletShooter;
		shooterController.shooter.parentController = shooterController;
		if(shooterController != null) {
			selectedShooter.deathController = shooterController;
			shooterController.bulletContainer = selectedShooter.bulletContainer;
			shooterController.parentController = selectedShooter.parentController;
			shooterController.tag = selectedShooter.tag;
			
		}
		EditorUtility.SetDirty(this.selectedController.gameObject);
	}
	
	void OnRemoveDeathShooterClicked() {
		selectedShooter.deathController = null;
	}
	
	void DisplayShooterToolsMenu() {
		GenericMenu menu = new GenericMenu();
		
		menu.AddItem(new GUIContent("Copy"), false, OnMenuCopyClicked);
		if(copiedShooter != null &&
		   copiedShooter != selectedShooter)
			menu.AddItem(new GUIContent("Paste"), false, OnMenuPasteClicked);
		else
			menu.AddDisabledItem(new GUIContent("Paste"));
		menu.AddSeparator("");
		/*
		if(selectedShooter.deathController == null) {
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
		*/
		menu.AddItem(new GUIContent("Remove"), false, OnMenuRemoveClicked);
		menu.ShowAsContext();
	}
	
	#endregion


};