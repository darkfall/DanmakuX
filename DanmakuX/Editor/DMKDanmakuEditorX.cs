using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

class DMKDanmakuEditorX: EditorWindow {

	public static int LeftPaneWidth = 154;
	public static int InspectorWidth = 260;

	public static int ActionBarWidth = 60;
	public static int ActionBarHeight = 26;
	
	public static int DanmakuListWindowId = 999;
	public static int DanmakuListWindowWidth = 240;
	public static int DanmakuListWindowHeight = 120;

	public static int ShooterGraphWindowWidth = 100;
	public static int ShooterGraphWindowHeight = 52;
	public static int ShooterModifierGraphWindowWidth = 100;
	public static int ShooterModifierGraphWindowHeight = 32;

	public static int ShooterModifierWindowIdStartIndex = 100;
	
	DMKController selectedController;

	[SerializeField]
	DMKDanmaku selectedDanmaku;

	DMKBulletShooterController 	copiedShooter = null;
	DMKShooterModifier			copiedModifier = null;

	UnityEngine.Object selectedGraphObject = null;

	Vector2 inspectorScrollPosition = new Vector2(0, 0);
	Vector2 danmakuListScrollPosition = new Vector2(0, 0);
	Vector2 shooterGraphScrollPosition = new Vector2(0, 0);

	bool creatingLink = false;
	Type linkSourceType;
	Type linkTargetType;
	Rect linkStartPos;
	
	public static void Create() {
		DMKDanmakuEditorX editor = (DMKDanmakuEditorX)EditorWindow.GetWindow<DMKDanmakuEditorX>("DanmakuX", true);
		editor.Init();
	}
	
	public void Init() {
		this.minSize = new Vector2(InspectorWidth + DanmakuListWindowWidth + 20,
		                           DanmakuListWindowHeight + ActionBarHeight + 200);
	}
	
	public void OnFocus() {
		if(selectedController != null &&
		   selectedController.danmakus.IndexOf(selectedDanmaku) == -1)
			selectedDanmaku = null;
		this.Repaint();
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
			this.ShooterGraphGUI();
			this.DanmakuListGUI();
			this.ActionBarGUI();
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

			selectedGraphObject = null;
			if(selectedController != null && selectedController.danmakus.Count > 0) {
				selectedDanmaku = selectedController.danmakus[0];
			}
			
			this.Repaint();
		} catch {
			selectedController = null;
			selectedGraphObject = null;
		}
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
		GUILayout.BeginArea(new Rect((this.position.width - ActionBarWidth - InspectorWidth) / 2, 
		                             this.position.height - ActionBarHeight - 16, 
		                             ActionBarWidth, 
		                             ActionBarHeight), 
		                    (GUIStyle)"box");
		GUILayout.BeginHorizontal();
		{
			if(selectedController.currentAttackIndex != -1) {
				if(GUILayout.Button(EditorGUIUtility.FindTexture( "d_PlayButton On" ),
				                    "label",
									GUILayout.Width(24))) {
					selectedController.StartDanmaku(-1);
					selectedController.paused = false;
				}
			} else {
				if(GUILayout.Button(EditorGUIUtility.FindTexture( "d_PlayButton" ),
				                    "label",
				                    GUILayout.Width(24))) {
					selectedController.StartDanmaku(selectedDanmaku);
				}
			}

			if(selectedController.paused) {
				if(GUILayout.Button(EditorGUIUtility.FindTexture( "d_PauseButton on" ),
				                    "label",
				                    GUILayout.Width(24))) {
					selectedController.paused = false;
				}
			} else {
				if(GUILayout.Button(EditorGUIUtility.FindTexture( "d_PauseButton" ),
				                    "label",
				                    GUILayout.Width(24))) {
					selectedController.paused = true;
				}
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

	void OnDanmakuListWindow() {
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
					selectedGraphObject = null;
				}
			}
		}

		GUILayout.EndScrollView();
	}

	void DanmakuListGUI() {
		GUIStyle s = new GUIStyle(GUI.skin.window);
		s.onNormal.background = s.normal.background;
	
		GUILayout.BeginArea(new Rect(0, 0, DanmakuListWindowWidth, DanmakuListWindowHeight), s);
		this.OnDanmakuListWindow();

		GUILayout.EndArea();
	}

	void OnShooterWindow(DMKBulletShooterController shooter) {
		if ((Event.current.button == 0 || Event.current.button == 1)) {
			if(Event.current.type == EventType.MouseDown) {
				selectedGraphObject = shooter;
				Event.current.Use();

			} else if(Event.current.type == EventType.MouseUp) {
				if(Event.current.button == 1)
					this.DisplayShooterToolsMenu();
				Event.current.Use();
			}
		}

		GUI.Label(new Rect(2, 0, ShooterGraphWindowWidth, 16), shooter.DMKName());

		Sprite sprite = shooter.bulletInfo.bulletSprite;
		if(sprite != null) {
			Texture2D tex = sprite.texture;
			if(tex != null) {
				int width = (int)(Mathf.Clamp(sprite.rect.width, 0, ShooterGraphWindowHeight - 16));
				int height = (int)(Mathf.Clamp(sprite.rect.height, 0, ShooterGraphWindowHeight - 16));
				DMKGUIUtility.DrawTextureWithTexCoordsAndColor(new Rect(Mathf.Clamp((ShooterGraphWindowWidth - width)/2, 0, ShooterGraphWindowWidth/2),
				                                                        16 + Mathf.Clamp((ShooterGraphWindowHeight - 16 - height)/2, 0, ShooterGraphWindowHeight/2),
				                                                        width,
				                                                        height),
				                                               tex,
				                                               new Rect((float)sprite.rect.x / tex.width,
				         												(float)sprite.rect.y / tex.height,
				         												(float)sprite.rect.width / tex.width,
				         												(float)sprite.rect.height / tex.height),
				                                               shooter.bulletInfo.bulletColor);
			}
		}
	}

	void OnShooterModifierWindow(DMKShooterModifier modifier) {
		if ((Event.current.button == 0 || Event.current.button == 1)) {
			if(Event.current.type == EventType.MouseDown) {
				if(!creatingLink)
					selectedGraphObject = modifier;
				Event.current.Use();
			} else if(Event.current.type == EventType.MouseUp) {
				if(Event.current.button == 0 && creatingLink) {
					if(linkSourceType == typeof(DMKBulletShooterController))
						(selectedGraphObject as DMKBulletShooterController).shooter.modifier = modifier;
					else if(linkSourceType == typeof(DMKShooterModifier) &&
					        !HasModifierLoop(modifier, selectedGraphObject as DMKShooterModifier)) { 
						(selectedGraphObject as DMKShooterModifier).next = modifier;
					}
					creatingLink = false;
				} else if(Event.current.button == 1) {
					if(creatingLink)
						creatingLink = false;
					else
						this.DisplayModifierToolsMenu();
				}
				Event.current.Use();
			}

		}

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

	bool HasModifierLoop(DMKShooterModifier src, DMKShooterModifier target) {
		DMKShooterModifier m = src.next;
		while(m != null) {
			if(m == target)
				return true;
			m = m.next;
		}
		return false;
	}

	void DrawVerticalBezier(Vector3 start, Vector3 end, bool drawArrow) {
		float tangentOff = (end.y - start.y) / 2;
		if(end.y < start.y) {
			//tangentOff = -tangentOff;
			if(drawArrow)
				end.y += 8;
		} else {
			if(drawArrow)
				end.y -= 8;
		}
		Handles.DrawBezier(start,
		                   end ,
		                   start + Vector3.up * tangentOff,
		                   end - Vector3.up * tangentOff,
		                   Color.yellow,
		                   null,
		                   3);
		if(drawArrow) {
			if(end.y > start.y)
				DMKGUIUtility.DrawTextureAt(Resources.LoadAssetAtPath<Texture2D>("Assets/Scripts/DanmakuX/Editor/Resources/Icons/arrow_down.png"),
			                            	new Rect(end.x - 10, 
			         								 end.y - 8,
		    	     								 20, 20),
			    	                        Color.yellow);
			else 
				DMKGUIUtility.DrawTextureAt(Resources.LoadAssetAtPath<Texture2D>("Assets/Scripts/DanmakuX/Editor/Resources/Icons/arrow_up.png"),
				                            new Rect(end.x - 10, 
				         							 end.y - 12,
				         							 20, 20),
				                            Color.yellow);
		}
	}

	void ShooterGraphGUI() {

		if(selectedController.danmakus.Count == 0) {
			return;
		}

		if(selectedDanmaku != null) {
			Rect graphWindowRect = new Rect(0, 0, this.position.width - InspectorWidth, this.position.height);
			Rect nodeWindowRect = new Rect(0, 0, ShooterGraphWindowWidth, ShooterGraphWindowHeight);
			int widthRequired = selectedDanmaku.shooters.Count * (ShooterGraphWindowWidth + 40) - 40;

			// to do, scroll height required?
			shooterGraphScrollPosition =  GUI.BeginScrollView(graphWindowRect,
			                    							  shooterGraphScrollPosition,
			                                                  new Rect(0, 0, widthRequired + 40, this.position.height - 24));
			GUI.Box(new Rect(shooterGraphScrollPosition.x, 
			                 shooterGraphScrollPosition.y, 
			                 this.position.width - InspectorWidth, 
			                 this.position.height),
			        "",
			        (GUIStyle)"flow background");
		//	shooterGraphScrollPosition =  GUILayout.BeginScrollView(shooterGraphScrollPosition, (GUIStyle)"flow background");

			int center = (int) (this.position.height - DanmakuListWindowHeight) / 2;
			nodeWindowRect.y = 40 + DanmakuListWindowHeight;
			nodeWindowRect.x = Mathf.Clamp((this.position.width - InspectorWidth) / 2 - widthRequired / 2, 20, 9999);

			for(int i=0; i<selectedDanmaku.shooters.Count; ++i) {
				DMKBulletShooterController shooter = selectedDanmaku.shooters[i];

				GUILayout.BeginArea(nodeWindowRect, selectedGraphObject == shooter ? (GUIStyle)"flow node 0 on" : (GUIStyle)"flow node 0");
				this.OnShooterWindow(shooter);
				GUILayout.EndArea();

				shooter.editorWindowRect = nodeWindowRect;

				if(shooter.shooter.modifier != null) {
					DMKShooterModifier modifier = shooter.shooter.modifier;
					this.DrawVerticalBezier(new Vector3(nodeWindowRect.x + nodeWindowRect.width / 2,
					                            		nodeWindowRect.y + nodeWindowRect.height),
					                		new Vector3(modifier.editorWindowRect.x + modifier.editorWindowRect.width / 2,
					            						modifier.editorWindowRect.y),
					                		true);
				}
				nodeWindowRect.x += (ShooterGraphWindowWidth + 40);
			}

			nodeWindowRect = new Rect((this.position.width - InspectorWidth) / 2 - ShooterModifierGraphWindowWidth/2,
			                      		40 + ShooterGraphWindowHeight + 52 + DanmakuListWindowHeight,
			                   			ShooterModifierGraphWindowWidth,
			                   			ShooterModifierGraphWindowHeight);
			
			foreach(DMKShooterModifier modifier in selectedDanmaku.modifiers) {
				GUIStyle modifierStyle = (GUIStyle)"flow node 1";
				if(selectedGraphObject == modifier ||
				   (creatingLink && 
				 	linkTargetType == typeof(DMKShooterModifier) &&
				 	nodeWindowRect.Contains(Event.current.mousePosition) &&
				 	!HasModifierLoop(modifier, selectedGraphObject as DMKShooterModifier))) {
					modifierStyle = (GUIStyle)"flow node 1 on";
				}

				GUILayout.BeginArea(nodeWindowRect, modifierStyle);
				this.OnShooterModifierWindow(modifier);
				GUILayout.EndArea();
				
				if(modifier.next != null) {
					DMKShooterModifier next = modifier.next;
					if(next.editorWindowRect.y > modifier.editorWindowRect.y)
						this.DrawVerticalBezier(new Vector3(nodeWindowRect.x + nodeWindowRect.width / 2, 
						                            		nodeWindowRect.y + nodeWindowRect.height),
						                        new Vector3(next.editorWindowRect.x + next.editorWindowRect.width / 2,
						            						next.editorWindowRect.y),
						                		true);
					else {
						this.DrawVerticalBezier(new Vector3(nodeWindowRect.x + nodeWindowRect.width / 2, 
						                                    nodeWindowRect.y),
						                        new Vector3(next.editorWindowRect.x + next.editorWindowRect.width / 2,
						            						next.editorWindowRect.y + next.editorWindowRect.height),
						                        true);
					}
				}
				modifier.editorWindowRect = nodeWindowRect;

				nodeWindowRect.y += ShooterModifierGraphWindowHeight + 40;
			}

			if(creatingLink) {
				this.DrawVerticalBezier(new Vector3(linkStartPos.x + linkStartPos.width / 2,
				                            		linkStartPos.y + linkStartPos.height),
				                		Event.current.mousePosition,
				                		false);

				this.Repaint();
			}

		//	GUILayout.EndArea();
			GUI.EndScrollView();

			if ((Event.current.button == 0 || Event.current.button == 1) && 
			    (Event.current.type == EventType.mouseUp) &&
			    graphWindowRect.Contains(Event.current.mousePosition)) {

				if(Event.current.button == 1 && !creatingLink)
					this.DisplayShooterGraphMenu();
				else
					selectedGraphObject = null;

				creatingLink = false;

				this.Repaint();
			}              
		}
	}

	void InspectorGUI() {

		GUILayout.BeginArea(new Rect(this.position.width - DMKDanmakuEditorX.InspectorWidth, 0, DMKDanmakuEditorX.InspectorWidth, this.position.height),
		                    (GUIStyle)"hostview");
		
		EditorGUILayout.BeginVertical();
		inspectorScrollPosition = GUILayout.BeginScrollView(inspectorScrollPosition);

		if(selectedGraphObject != null) {
			if(typeof(DMKBulletShooterController).IsAssignableFrom(selectedGraphObject.GetType()))
				this.ShooterGUI(selectedGraphObject as DMKBulletShooterController);
			else if(typeof(DMKShooterModifier).IsAssignableFrom(selectedGraphObject.GetType()))
				(selectedGraphObject as DMKShooterModifier).OnEditorGUI();
		}
		else {
			GUILayout.Label("No node selected");
		}


		GUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
		
		GUILayout.EndArea();


	}

	void ShooterGUILowerPart_BulletInfo(DMKBulletShooterController shooter) {
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
	
	void ShooterGUILowerPart_Shooter(DMKBulletShooterController shooter) {
		Rect rr = GUILayoutUtility.GetLastRect();
		GUI.Box (new Rect(0, rr.y + rr.height, rr.width, 2),
		         "");

		EditorGUILayout.BeginVertical();
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
				GUILayout.Space (20);
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
		selectedDanmaku.AddModifier(modifier);
		// to do
		//selectedShooter.shooter.AddModifier(modifier);
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

	void OnShooterMenuCopyClicked() {
		copiedShooter = selectedGraphObject as DMKBulletShooterController;
	}
	
	void OnShooterMenuPasteClicked() {
		if(copiedShooter != null &&
		   copiedShooter != selectedGraphObject) {
			(selectedGraphObject as DMKBulletShooterController).CopyFrom(copiedShooter);
		}
	}
	
	void OnShooterMenuRemoveClicked() {
		if(EditorUtility.DisplayDialog("Remove Shooter", "Are you sure you want to remove this Shooter?", "Yes", "No")) {
			if(selectedGraphObject != null)
				selectedDanmaku.shooters.Remove(selectedGraphObject as DMKBulletShooterController);
			selectedGraphObject = null;
		}
	}

	void OnShooterMenuCreateLinkClicked() {
		DMKBulletShooterController shooterController = selectedGraphObject as DMKBulletShooterController;

		creatingLink = true;
		linkTargetType = typeof(DMKShooterModifier);
		linkSourceType = typeof(DMKBulletShooterController);
		linkStartPos = shooterController.editorWindowRect;
		shooterController.shooter.modifier = null;
	}

	void DisplayShooterToolsMenu() {
		GenericMenu menu = new GenericMenu();

		DMKBulletShooterController shooterController = selectedGraphObject as DMKBulletShooterController;
		if(!shooterController)
			return;

		menu.AddItem(new GUIContent("Link Modifier"), false, OnShooterMenuCreateLinkClicked);
		menu.AddSeparator("");

		menu.AddItem(new GUIContent("Copy"), false, OnShooterMenuCopyClicked);
		if(copiedShooter != null &&
		   copiedShooter != selectedGraphObject)
			menu.AddItem(new GUIContent("Paste"), false, OnShooterMenuPasteClicked);
		else
			menu.AddDisabledItem(new GUIContent("Paste"));
		menu.AddSeparator("");

	
		menu.AddItem(new GUIContent("Remove"), false, OnShooterMenuRemoveClicked);
		menu.ShowAsContext();
	}
	
	#endregion

	#region modifier tools menu

	void OnModifierMenuCopyClicked() {
		copiedModifier = selectedGraphObject as DMKShooterModifier;
	}
	
	void OnModifierMenuPasteClicked() {
		if(copiedModifier != null &&
		   copiedModifier != selectedGraphObject) {
			(selectedGraphObject as DMKShooterModifier).CopyFrom(copiedModifier);
		}
	}
	
	void OnModifierMenuRemoveClicked() {
		if(selectedGraphObject != null)
			selectedDanmaku.RemoveModifier(selectedGraphObject as DMKShooterModifier);
		selectedGraphObject = null;
	}
	
	void OnModifierMenuCreateLinkClicked() {
		DMKShooterModifier modifier = selectedGraphObject as DMKShooterModifier;

		creatingLink = true;
		linkSourceType = linkTargetType = typeof(DMKShooterModifier);
		linkStartPos = modifier.editorWindowRect;
		modifier.next = null;
	}

	void DisplayModifierToolsMenu() {
		GenericMenu menu = new GenericMenu();
		
		DMKShooterModifier modifier = selectedGraphObject as DMKShooterModifier;
		if(!modifier)
			return;

		menu.AddItem(new GUIContent("Link Modifier"), false, OnModifierMenuCreateLinkClicked);
		menu.AddSeparator("");
		
		menu.AddItem(new GUIContent("Copy"), false, OnModifierMenuCopyClicked);
		if(copiedModifier != null &&
		   copiedModifier != selectedGraphObject)
			menu.AddItem(new GUIContent("Paste"), false, OnModifierMenuPasteClicked);
		else
			menu.AddDisabledItem(new GUIContent("Paste"));
		menu.AddSeparator("");

		menu.AddItem(new GUIContent("Remove"), false, OnModifierMenuRemoveClicked);
		menu.ShowAsContext();
	}

	#endregion

	#region shooter graph menu

	void DisplayShooterGraphMenu() {
		GenericMenu menu = new GenericMenu();

		foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach(Type type in asm.GetTypes()) {
				if(type.BaseType == typeof(DMKBulletShooter)) {
					menu.AddItem(new GUIContent("New Shooter/" + type.ToString()), false, OnAddShooterClicked, type.ToString());
				}
			}
		}

		foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
			foreach(Type type in asm.GetTypes()) {
				if(type.BaseType == typeof(DMKShooterModifier)) {
					menu.AddItem(new GUIContent("New Modifier/" + type.ToString()), false, OnAddModifierClicked, type.ToString());
				}
			}
		}
		menu.ShowAsContext();
	}
	
	#endregion


};