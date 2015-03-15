using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace danmakux {

	class DMKDanmakuEditorX: EditorWindow {

		public static int LeftPaneWidth = 154;
		public static int InspectorWidth = 260;

		public static int ActionBarWidth = 60;
		public static int ActionBarHeight = 26;
		
		public static int DanmakuListWindowId = 999;
		public static int DanmakuListWindowWidth = 240;
		public static int DanmakuListWindowHeight = 120;

		public static int ShooterNodeWindowWidth = 100;
		public static int ShooterNodeWindowHeight = 52;
		public static int ModifierNodeWindowWidth = 100;
		public static int ModifierNodeWindowHeight = 32;
		public static int TriggerNodeWindowWidth = 100;
		public static int TriggerNodeWindowHeight = 32;

		DMKController selectedController;

		[SerializeField]
		DMKDanmaku selectedDanmaku;

		DMKBulletShooterController 	copiedShooter = null;
		DMKShooterModifier			copiedModifier = null;
		DMKDanmaku					copiedDanmaku = null;

		UnityEngine.Object selectedGraphObject = null;

		Vector2 inspectorScrollPosition = new Vector2(0, 0);
		Vector2 danmakuListScrollPosition = new Vector2(0, 0);
		Vector2 shooterGraphScrollPosition = new Vector2(0, 0);

		bool creatingLink = false;
		Type linkSourceType;
		Rect linkStartPos;

		float shooterGraphHeight = 0;

		public static DMKDanmakuEditorX instance = null;
		public static bool IsOpen { 
			get { return instance != null; } 
		}
		
		public static void Create() {
			instance = (DMKDanmakuEditorX)EditorWindow.GetWindow<DMKDanmakuEditorX>("DanmakuX", true);
			instance.Init();
		}
		
		public void Init() {
			this.minSize = new Vector2(InspectorWidth + DanmakuListWindowWidth + 20,
			                           DanmakuListWindowHeight + ActionBarHeight + 200);

			shooterGraphHeight = this.position.height;
		}
		
		public void OnFocus() {
			if(selectedController != null &&
			   selectedController.danmakus.IndexOf(selectedDanmaku) == -1) {
				selectedDanmaku = null;
				this.Repaint();
			}
			else if(selectedController == null)
				this.OnSelectionChange();
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

		DMKDanmaku CreateNewDanmaku() {
			DMKDanmaku danmaku = (DMKDanmaku)ScriptableObject.CreateInstance<DMKDanmaku>();
			danmaku.name = "New Danmaku";
			danmaku.shooters = new List<DMKBulletShooterController>();
			
			selectedController.danmakus.Add(danmaku);

			selectedDanmaku = danmaku;
			selectedDanmaku.parentController = selectedController;
			return danmaku;
		}

		void ActionBarGUI() {
			GUILayout.BeginArea(new Rect(40, 
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
			if(EditorGUI.EndChangeCheck()) {
				if(selectedController.currentAttackIndex != -1)
					selectedController.StartDanmaku(selectedController.currentAttackIndex);
			}

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
			              new GUIContent(Resources.LoadAssetAtPath<Texture2D>("Assets/Scripts/DanmakuX/Editor/Resources/Icons/settings.png")),
			              "label")) {
				
				this.ShowDanmakuOptionMenu(selectedDanmaku);

			//	this.CreateNewDanmaku();
			}

			danmakuListScrollPosition = EditorGUILayout.BeginScrollView(danmakuListScrollPosition);
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
					
					GUILayout.EndHorizontal();

				}
				if(danmaku.editorExpanded) {
					this.DanmakuGUI(danmaku);
				}

				GUILayout.EndVertical();
			}
			
			EditorGUILayout.EndScrollView();

			if(Event.current.type == EventType.Repaint) {
				for(int i=0; i<selectedController.danmakus.Count; ++i) {
					DMKDanmaku danmaku = selectedController.danmakus[i];
					
					if(GUI.GetNameOfFocusedControl() == "danmaku_" + i.ToString()) {
					//	Debug.Log(GUI.GetNameOfFocusedControl());
						if(selectedDanmaku != danmaku) {
							selectedDanmaku = danmaku;
							selectedGraphObject = null;
							
						//	this.Repaint();
						}
					}
				}

			}

		}

		void DanmakuListGUI() {
			GUIStyle s = new GUIStyle(GUI.skin.window);
			s.onNormal.background = s.normal.background;
		
			GUILayout.BeginArea(new Rect(0, 0, DanmakuListWindowWidth, DanmakuListWindowHeight), s);
			this.OnDanmakuListWindow();
			GUILayout.EndArea();
		}

		void DrawShooterInfo(DMKBulletShooterController shooter, Rect windowRect, bool isSubShooter) {
			GUI.skin.label.wordWrap = true;

			EditorGUI.BeginChangeCheck();
			shooter.editorEnabled = EditorGUI.Toggle(new Rect(0, windowRect.height / 2, 16, 16), shooter.editorEnabled);
			if(EditorGUI.EndChangeCheck()) {
				selectedDanmaku.UpdateShooters();
			}

			GUI.Label(new Rect(2, 0, windowRect.width, 16), shooter.DMKName());
			GUI.skin.label.wordWrap = false;

			Sprite sprite = shooter.bulletInfo.bulletSprite;
			if(sprite != null) {
				Texture2D tex = sprite.texture;
				if(tex != null) {
					int width = (int)(Mathf.Clamp(sprite.rect.width, 0, windowRect.height - 16));
					int height = (int)(Mathf.Clamp(sprite.rect.height, 0, windowRect.height - 16));
					DMKGUIUtility.DrawTextureWithTexCoordsAndColor(new Rect(Mathf.Clamp((windowRect.width - width)/2, 0, windowRect.width/2),
					                                                        16 + Mathf.Clamp((windowRect.height - 16 - height)/2, 0, windowRect.height/2),
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

		void OnShooterWindow(DMKBulletShooterController shooter, Rect windowRect, bool isSubShooter = false) {
			if(shooter == null)
				return;
			
			DrawShooterInfo(shooter, windowRect, isSubShooter);

			if ((Event.current.button == 0 || Event.current.button == 1)) {
				if(Event.current.type == EventType.MouseDown) {
					if(!creatingLink) {
						selectedGraphObject = shooter;
						GUI.FocusControl("");
					}
					Event.current.Use();

				} else if(Event.current.type == EventType.MouseUp) {
					if(Event.current.button == 1) {
						if(!creatingLink)
							this.DisplayShooterToolsMenu(isSubShooter);
						else
							creatingLink = false;
					}
					Event.current.Use();
				}
			}

		}

		void OnShooterModifierWindow(DMKShooterModifier modifier) {
			GUIStyle s = new GUIStyle(GUI.skin.label);
			s.fontSize = 12;
			s.normal.textColor = new Color(255, 255, 255, 1);
			s.alignment = TextAnchor.MiddleCenter;
			GUI.Label(new Rect(6, 0, ModifierNodeWindowWidth, ModifierNodeWindowHeight),
			          modifier.DMKName(),
			          s);

			modifier.editorEnabled = EditorGUI.Toggle(new Rect(0, ModifierNodeWindowHeight / 2 - 8, 16, 16), modifier.editorEnabled);

			
			if ((Event.current.button == 0 || Event.current.button == 1)) {
				if(Event.current.type == EventType.MouseDown) {
					if(!creatingLink) {
						selectedGraphObject = modifier;
						GUI.FocusControl("");
					}
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
						if(creatingLink) {
							creatingLink = false;
						}
						else
							this.DisplayModifierToolsMenu();
					}
					Event.current.Use();
				}
				
			}

		}

		void OnTriggerWindow(DMKTrigger trigger) {
			if ((Event.current.button == 0 || Event.current.button == 1)) {
				if(Event.current.type == EventType.MouseDown) {
					if(!creatingLink) {
						selectedGraphObject = trigger;
						GUI.FocusControl("");
					}
					Event.current.Use();
				} else if(Event.current.type == EventType.MouseUp) {
					if(Event.current.button == 0 && creatingLink) {
						creatingLink = false;
					} else if(Event.current.button == 1) {
						if(creatingLink)
							creatingLink = false;
						else
							this.DisplayTriggerToolsMenu();
					}
					Event.current.Use();
				}
			}
			
			GUIStyle s = new GUIStyle(GUI.skin.label);
			s.fontSize = 12;
			s.normal.textColor = new Color(255, 255, 255, 1);
			s.alignment = TextAnchor.MiddleCenter;
			s.wordWrap = true;
			GUI.Label(new Rect(0, 0, TriggerNodeWindowWidth, TriggerNodeWindowHeight),
			          trigger.DMKName(),
			          s);
			s.wordWrap = false;
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

		float DrawSubControllerNode(DMKSubBulletShooterController controller, Rect windowRect, bool isSecondaryLevel = false) {
			if(controller.internalController == null)
				return windowRect.y + windowRect.height;
			
			Rect nodeWindowRect;
			if(isSecondaryLevel)
				nodeWindowRect = new Rect(windowRect.x,
				                          windowRect.y + 40 + ShooterNodeWindowHeight,
				                          windowRect.width,
			                              windowRect.height);
			else
				nodeWindowRect = new Rect(windowRect.x + 5,
				                          windowRect.y + 40 + ShooterNodeWindowHeight,
				                          windowRect.width - 10,
				                          windowRect.height - 10);

					
			this.DrawVerticalBezier(new Vector3(windowRect.x + windowRect.width / 2,
			                                    windowRect.y + windowRect.height),
			                        new Vector3(nodeWindowRect.x + nodeWindowRect.width / 2,
			            						nodeWindowRect.y),
			                        true);


			GUIStyle shooterStyle = (GUIStyle)"flow node 0";
			if(selectedGraphObject == controller.internalController) {
				shooterStyle = (GUIStyle)"flow node 0 on";
			}

			GUILayout.BeginArea(nodeWindowRect, shooterStyle);
			this.OnShooterWindow(controller.internalController, nodeWindowRect, true);
			GUILayout.EndArea();
			
			controller.editorWindowRect = controller.internalController.editorWindowRect = nodeWindowRect;
			
			if(controller.internalController.shooter.modifier != null) {
				DMKShooterModifier modifier = controller.internalController.shooter.modifier;
				this.DrawVerticalBezier(new Vector3(nodeWindowRect.x + nodeWindowRect.width / 2,
				                                    nodeWindowRect.y + nodeWindowRect.height),
				                        new Vector3(modifier.editorWindowRect.x + modifier.editorWindowRect.width / 2,
				            						modifier.editorWindowRect.y),
				                        true);
			}

			DMKSubBulletShooterController next = controller.internalController.subController;
			while(next != null) {
				nodeWindowRect.y = this.DrawSubControllerNode(next, nodeWindowRect, true);

				next = next.internalController.subController;
			}
			return nodeWindowRect.y;
		}

		void ShooterGraphGUI() {

			if(selectedDanmaku != null) {
				Rect graphWindowRect = new Rect(0, 0, this.position.width - InspectorWidth, this.position.height);
				Rect nodeWindowRect = new Rect(0, 0, TriggerNodeWindowWidth, TriggerNodeWindowHeight);
				
				int triggerWidthRequired = selectedDanmaku.triggers.Count * (TriggerNodeWindowWidth + 40) - 40;
				int shooterWidthRequired = selectedDanmaku.shooters.Count * (ShooterNodeWindowWidth + 40) - 40;
				int widthRequired = Mathf.Max (triggerWidthRequired, shooterWidthRequired);

				shooterGraphScrollPosition =  GUI.BeginScrollView(graphWindowRect,
				                    							  shooterGraphScrollPosition,
				                                                  new Rect(0, 0, widthRequired + 40, shooterGraphHeight));
				GUI.Box(new Rect(shooterGraphScrollPosition.x, 
				                 shooterGraphScrollPosition.y, 
				                 this.position.width - InspectorWidth, 
				                 this.position.height),
				        "",
				        (GUIStyle)"flow background");
			//	shooterGraphScrollPosition =  GUILayout.BeginScrollView(shooterGraphScrollPosition, (GUIStyle)"flow background");

				float startY = nodeWindowRect.y + DanmakuListWindowHeight + 40;
				if(shooterGraphHeight < this.position.height) {
					startY = this.position.height / 2 - (shooterGraphHeight - DanmakuListWindowHeight) / 2;
				}
				nodeWindowRect = new Rect(Mathf.Clamp((this.position.width - InspectorWidth) / 2 - shooterWidthRequired / 2, 20, 9999),
				                          startY,
				                          ShooterNodeWindowWidth,
				                          ShooterNodeWindowHeight);

				float bottomY = nodeWindowRect.y;
				foreach(DMKBulletShooterController controller in selectedDanmaku.shooters) {
					GUIStyle shooterStyle = (GUIStyle)"flow node 0";
					if(selectedGraphObject == controller) {
						shooterStyle = (GUIStyle)"flow node 0 on";
					}

					GUILayout.BeginArea(nodeWindowRect, shooterStyle);
					this.OnShooterWindow(controller, nodeWindowRect);
					GUILayout.EndArea();

					controller.editorWindowRect = nodeWindowRect;

					if(controller.shooter == null)
						continue;

					if(controller.shooter.modifier != null) {
						DMKShooterModifier modifier = controller.shooter.modifier;
						this.DrawVerticalBezier(new Vector3(nodeWindowRect.x + nodeWindowRect.width / 2,
						                            		nodeWindowRect.y + nodeWindowRect.height),
						                		new Vector3(modifier.editorWindowRect.x + modifier.editorWindowRect.width / 2,
						            						modifier.editorWindowRect.y),
						                		true);
					}

					if(controller.subController != null) {
						bottomY = Mathf.Max (this.DrawSubControllerNode(controller.subController, nodeWindowRect), bottomY);
					} else {
						bottomY = Mathf.Max (nodeWindowRect.y, bottomY);
					}

					nodeWindowRect.x += (ShooterNodeWindowWidth + 40);
				}

				nodeWindowRect = new Rect(Mathf.Max (widthRequired, this.position.width - InspectorWidth)/ 2 - ModifierNodeWindowWidth / 2,
				                      	  bottomY + 52 + ShooterNodeWindowHeight,
				                   		  ModifierNodeWindowWidth,
				                   		  ModifierNodeWindowHeight);
				
				foreach(DMKShooterModifier modifier in selectedDanmaku.modifiers) {
					GUIStyle modifierStyle = (GUIStyle)"flow node 1";
					if(selectedGraphObject == modifier ||
					   (creatingLink && 
					 	(linkSourceType == typeof(DMKBulletShooterController) || linkSourceType == typeof(DMKShooterModifier)) &&
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

					nodeWindowRect.y += ModifierNodeWindowHeight + 40;
				}

				shooterGraphHeight = nodeWindowRect.y;

				if(creatingLink) {
					Vector3 end = Event.current.mousePosition;
					Vector3 start;
					if(end.y > linkStartPos.y + linkStartPos.height / 2) {
						start = new Vector3(linkStartPos.x + linkStartPos.width / 2,
						                    linkStartPos.y + linkStartPos.height);
					} else {
						start = new Vector3(linkStartPos.x + linkStartPos.width / 2,
						                    linkStartPos.y);
					}
					this.DrawVerticalBezier(start,
					                		Event.current.mousePosition,
					                		false);

					this.Repaint();
				}

			//	GUILayout.EndArea();
				GUI.EndScrollView();

				if ((Event.current.button == 0 || Event.current.button == 1)
				     && graphWindowRect.Contains(Event.current.mousePosition)) {

					if((Event.current.type == EventType.mouseUp)) {
						if(creatingLink) {
							if(linkSourceType == typeof(DMKBulletShooterController))
								(selectedGraphObject as DMKBulletShooterController).shooter.modifier = null;
							else if(linkSourceType == typeof(DMKShooterModifier)) { 
								(selectedGraphObject as DMKShooterModifier).next = null;
							}
							
							creatingLink = false;
						}
						else {
							if(Event.current.button == 1 && !creatingLink)
								this.DisplayShooterGraphMenu();
							else
								selectedGraphObject = null;
						}
					}


					this.Repaint();
				}              
			} else {
				GUI.Box(new Rect(0, 
				                 0, 
				                 this.position.width - InspectorWidth, 
				                 this.position.height),
				        "",
				        (GUIStyle)"flow background");
			}
		}

		void InspectorGUI() {

			GUILayout.BeginArea(new Rect(this.position.width - DMKDanmakuEditorX.InspectorWidth, 0, DMKDanmakuEditorX.InspectorWidth, this.position.height),
			                    (GUIStyle)"hostview");
			
			EditorGUILayout.BeginVertical();
			inspectorScrollPosition = GUILayout.BeginScrollView(inspectorScrollPosition);

			if(selectedGraphObject != null) {
				if(typeof(DMKBulletShooterController).IsAssignableFrom(selectedGraphObject.GetType()))
					DMKShooterControllerInspector.OnEditorGUI(selectedGraphObject as DMKBulletShooterController);
				else if(typeof(DMKShooterModifier).IsAssignableFrom(selectedGraphObject.GetType())) {
					(selectedGraphObject as DMKShooterModifier).OnEditorGUI(false);
				}
			}
			else {
				GUILayout.Label("No node selected");
			}

			GUILayout.EndScrollView();
			EditorGUILayout.EndVertical();

			GUILayout.EndArea();
		}


		#region new shooter menu
		
		void OnAddShooterClicked(object userData) {
			DMKBulletShooterController shooterController = (DMKBulletShooterController)ScriptableObject.CreateInstance<DMKBulletShooterController>();
			shooterController.shooter = ScriptableObject.CreateInstance(userData as Type) as DMKBulletShooter;
			
			shooterController.editorExpanded = true;
			shooterController.parentController = selectedController;
			shooterController.gameObject = selectedController.transform.gameObject;

			shooterController.groupId = selectedDanmaku.shooters.Count;
			
			if(selectedDanmaku.shooters.Count > 0) {
				shooterController.bulletContainer = selectedDanmaku.shooters[0].bulletContainer;
				shooterController.tag = selectedDanmaku.shooters[0].tag;
			}
			selectedDanmaku.shooters.Add( shooterController );

			selectedDanmaku.UpdateShooters();
			EditorUtility.SetDirty(this.selectedController.gameObject);
		}
		
		void DisplayNewShooterMenu() {
			GenericMenu menu = new GenericMenu();
			
			foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(Type type in asm.GetTypes()) {
					if(type.BaseType == typeof(DMKBulletShooter)) {
						menu.AddItem(new GUIContent(DMKUtil.GetTypeClassName(type)), false, OnAddShooterClicked, type);
					}
				}
			}
			
			menu.ShowAsContext();
		}

		#endregion
		
		#region modifier menu
		
		void OnAddModifierClicked(object userData) {
			DMKShooterModifier modifier = ScriptableObject.CreateInstance(userData as Type) as DMKShooterModifier;
			selectedDanmaku.AddModifier(modifier);
			// to do
			//selectedShooter.shooter.AddModifier(modifier);
		}
		
		void DisplayNewModifierMenu() {
			GenericMenu menu = new GenericMenu();
			
			foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(Type type in asm.GetTypes()) {
					if(type.BaseType == typeof(DMKShooterModifier)) {
						menu.AddItem(new GUIContent(DMKUtil.GetTypeClassName(type)), false, OnAddModifierClicked, type);
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
		
		void OnShooterMenuRemoveClicked(object userData) {
			bool isSubShooter = (userData as string) == (true).ToString();
		
			if(EditorUtility.DisplayDialog("Remove Shooter", "Are you sure you want to remove this Shooter?", "Yes", "No")) {
				if(!isSubShooter) {
					if(selectedGraphObject != null)
						selectedDanmaku.shooters.Remove(selectedGraphObject as DMKBulletShooterController);
				} else {
					foreach(DMKBulletShooterController controller in selectedDanmaku.shooters) {
						if(controller.subController) {
							if(controller.subController.internalController == selectedGraphObject) {
								controller.subController = null;
							}
						}
					}
				}
				selectedGraphObject = null;
			}
		}
		
		void OnShooterMenuPasteAsNewClicked() {
			DMKBulletShooterController shooterController = selectedGraphObject as DMKBulletShooterController;

			DMKBulletShooterController newController = (DMKBulletShooterController)ScriptableObject.CreateInstance<DMKBulletShooterController>();
			newController.CopyFrom(shooterController);
			
			newController.groupId = selectedDanmaku.shooters.Count;
			selectedDanmaku.shooters.Add (newController);
			selectedDanmaku.UpdateShooters();
		}

		void OnShooterMenuCreateLinkClicked() {
			DMKBulletShooterController shooterController = selectedGraphObject as DMKBulletShooterController;

			creatingLink = true;
			linkSourceType = typeof(DMKBulletShooterController);
			linkStartPos = shooterController.editorWindowRect;
			shooterController.shooter.modifier = null;
		}

		void OnShooterMenuAddSubShooterClicked(object userData) {
			DMKBulletShooterController shooterController = (DMKBulletShooterController)ScriptableObject.CreateInstance<DMKBulletShooterController>();
			shooterController.shooter = ScriptableObject.CreateInstance(userData as Type) as DMKBulletShooter;

			DMKBulletShooterController selectedController = (selectedGraphObject as DMKBulletShooterController);

			shooterController.editorExpanded = true;
			shooterController.parentController = selectedController.parentController;
			shooterController.gameObject = null;
			shooterController.bulletContainer = selectedController.bulletContainer;
			shooterController.overallLength = 30;
			shooterController.gameObject = selectedController.gameObject;
			shooterController.followParentDirection = true;

			DMKSubBulletShooterController subController = (DMKSubBulletShooterController)ScriptableObject.CreateInstance<DMKSubBulletShooterController>();
			subController.internalController = shooterController;
			selectedController.subController = subController;
			EditorUtility.SetDirty(this.selectedController.gameObject);
		}


		void DisplayShooterToolsMenu(bool isSubShooter) {
			GenericMenu menu = new GenericMenu();

			DMKBulletShooterController shooterController = selectedGraphObject as DMKBulletShooterController;
			if(!shooterController)
				return;

			menu.AddItem(new GUIContent("Link Modifier"), false, OnShooterMenuCreateLinkClicked);

			if(shooterController.subController == null) {
				foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
					foreach(Type type in asm.GetTypes()) {
						if(type.BaseType == typeof(DMKBulletShooter)) {
							menu.AddItem(new GUIContent("New Sub-Shooter/" + DMKUtil.GetTypeClassName(type)), false, OnShooterMenuAddSubShooterClicked, type);
						}
					}
				}
			}

			if(!isSubShooter) {

				foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
					foreach(Type type in asm.GetTypes()) {
						if(type.BaseType == typeof(DMKTrigger)) {
							menu.AddItem(new GUIContent("New Trigger/" + DMKUtil.GetTypeClassName(type)), false, OnAddTriggerClicked, type);
						}
					}
				}
			}

			menu.AddSeparator("");


			menu.AddItem(new GUIContent("Copy"), false, OnShooterMenuCopyClicked);
			if(copiedShooter != null &&
			   copiedShooter != selectedGraphObject)
				menu.AddItem(new GUIContent("Paste"), false, OnShooterMenuPasteClicked);
			else
				menu.AddDisabledItem(new GUIContent("Paste"));
			if(copiedShooter != null)
				menu.AddItem(new GUIContent("Paste as New"), false, OnShooterMenuPasteAsNewClicked);
			menu.AddSeparator("");

		
			menu.AddItem(new GUIContent("Remove"), false, OnShooterMenuRemoveClicked, isSubShooter.ToString());
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
			linkSourceType = typeof(DMKShooterModifier);
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


		#region trigger tools menu

		
		void OnTriggerMenuRemoveClicked() {
			if(selectedGraphObject != null)
				selectedDanmaku.triggers.Remove(selectedGraphObject as DMKTrigger);

			selectedGraphObject = null;
		}
		
		void OnTriggerMenuCreateLinkClicked() {
			DMKTrigger trigger = selectedGraphObject as DMKTrigger;
			
			creatingLink = true;
			linkSourceType = typeof(DMKTrigger);
			linkStartPos = trigger.editorWindowRect;
		}
		
		void DisplayTriggerToolsMenu() {
			GenericMenu menu = new GenericMenu();
			
			DMKTrigger trigger = selectedGraphObject as DMKTrigger;
			if(!trigger)
				return;

			menu.AddItem(new GUIContent("Remove"), false, OnTriggerMenuRemoveClicked);
			menu.ShowAsContext();
		}
		
		#endregion

		#region shooter graph menu

		void OnAddTriggerClicked(object userData) {
			DMKTrigger trigger = ScriptableObject.CreateInstance(userData as Type) as DMKTrigger;
			selectedDanmaku.triggers.Add (trigger);
			// to do
			//selectedShooter.shooter.AddModifier(modifier);
		}

		void DisplayShooterGraphMenu() {
			GenericMenu menu = new GenericMenu();

			foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(Type type in asm.GetTypes()) {
					if(type.BaseType == typeof(DMKBulletShooter)) {
						menu.AddItem(new GUIContent("New Shooter/" + DMKUtil.GetTypeClassName(type)), false, OnAddShooterClicked, type);
					}
				}
			}

			foreach(System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach(Type type in asm.GetTypes()) {
					if(type.BaseType == typeof(DMKShooterModifier)) {
						menu.AddItem(new GUIContent("New Modifier/" + DMKUtil.GetTypeClassName(type)), false, OnAddModifierClicked, type);
					}
				}
			}


			menu.ShowAsContext();
		}
		
		#endregion

		#region danmaku option menu

		void OnDanmakuMenuCopyClicked(object userData) {
			copiedDanmaku = userData as DMKDanmaku;
		}

		void OnDanmakuMenuPasteClicked(object userData) {
			if(copiedDanmaku != null) {
				DMKDanmaku dst = userData as DMKDanmaku;
				dst.CopyFrom(copiedDanmaku);
			}
		}

		void OnDanmakuMenuPasteAsNewClicked(object userData) {
			if(copiedDanmaku != null) {
				this.CreateNewDanmaku().CopyFrom(copiedDanmaku);
			}
		}

		void OnDanmakuMenuRemoveClicked(object userData) {
			if(selectedDanmaku == userData) {
				selectedDanmaku = null;
			}
			selectedController.danmakus.Remove(userData as DMKDanmaku);
			this.Repaint();
		}

		void OnDanmakuMenuCreateNewClicked(object userData) {
			this.CreateNewDanmaku();
		}
		
		void ShowDanmakuOptionMenu(DMKDanmaku danmaku) {
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent("Create New Danmaku"), false, OnDanmakuMenuCreateNewClicked, danmaku);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Copy Selected"), false, OnDanmakuMenuCopyClicked, danmaku);
			menu.AddItem(new GUIContent("Paste"), false, OnDanmakuMenuPasteClicked, danmaku);
			if(copiedDanmaku != null)
				menu.AddItem(new GUIContent("Paste As New"), false, OnDanmakuMenuPasteAsNewClicked, danmaku);
			menu.AddSeparator("");
			
			menu.AddItem(new GUIContent("Remove Selected"), false, OnDanmakuMenuRemoveClicked, danmaku);
			menu.ShowAsContext();
		}

		#endregion

	};

	
}
