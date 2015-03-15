using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace danmakux {

	public class DMKGUIUtility {

		public static GUIStyle boxStyle;
		public static GUIStyle listEntryNormal;
		public static GUIStyle listEntryFocused;
		public static GUIStyle boxNoBackground;
		public static GUIStyle horizontalLine;

		public static bool Initialized = false;

		public static void Init() {
			if(!Initialized) {
				boxStyle = new GUIStyle(GUI.skin.box);
				
				listEntryNormal = new GUIStyle(GUI.skin.label);
				listEntryNormal.alignment = TextAnchor.MiddleCenter;
				
				listEntryFocused = new GUIStyle(GUI.skin.label);
				listEntryFocused.alignment = TextAnchor.MiddleCenter;
				SetStyleTextColor(listEntryFocused, new Color(0, 1, 0, 1));
				
				boxNoBackground = new GUIStyle(GUI.skin.box);
				boxNoBackground.normal.background = boxNoBackground.focused.background = boxNoBackground.active.background = null;

				horizontalLine = new GUIStyle("box");
				horizontalLine.border.top = horizontalLine.border.bottom = 1;
				horizontalLine.margin.top = horizontalLine.margin.bottom = 1;
				horizontalLine.padding.top = horizontalLine.padding.bottom = 1;
				
			}
			Initialized = true;
		}

		public static GUIStyle SetStyleTextColor(GUIStyle style, Color c) {
			style.normal.textColor =
			style.active.textColor =
			style.hover.textColor =
			style.focused.textColor =
			style.onNormal.textColor =
			style.onActive.textColor =
			style.onHover.textColor =
			style.onFocused.textColor = c;
			return style;
		}

		public static int MakeSimpleList(int selectedIndex, System.Collections.IList entries, TextAnchor textAlignment = TextAnchor.MiddleCenter) {
			return MakeSimpleList(selectedIndex, entries, () => {}, textAlignment);
		}

		public static int MakeSimpleList(int selectedIndex, System.Collections.IList entries, Action onSelectionChange, TextAnchor textAlignment = TextAnchor.MiddleCenter) {
			if(!Initialized) {
				DMKGUIUtility.Init();
			}
			if(entries.Count == 0)
				return selectedIndex;

			listEntryNormal.alignment = listEntryFocused.alignment = textAlignment;
			
			GUILayout.BeginVertical(boxStyle);
			int newSelectedIndex = -1;
			{
				for(int index=0; index<entries.Count; ++index) {
					if(GUILayout.Button(entries[index].ToString(),
					                    selectedIndex == index ? DMKGUIUtility.listEntryFocused : DMKGUIUtility.listEntryNormal)) {
						newSelectedIndex = index;
					}
				} 
			}
			
			GUILayout.EndVertical();
			
			int result = newSelectedIndex == -1 ? selectedIndex : newSelectedIndex;
			if(result != selectedIndex)
				onSelectionChange();
			return result;
		}

#if UNITY_EDITOR
		public static bool MakeCurveToggle(bool flag) {
			GUILayout.Label("Curve", GUILayout.Width(40));
			return EditorGUILayout.Toggle("", flag, GUILayout.Width(16));
		}
		
		public static void MakeCurveControl(ref DMKCurveProperty curve, string label) {
			EditorGUILayout.BeginHorizontal();
			switch(curve.type) {
			case DMKCurvePropertyType.Constant:
				curve.value = EditorGUILayout.FloatField(label, curve.value);
				break;
			case DMKCurvePropertyType.Curve:
				curve.curve = EditorGUILayout.CurveField(label, curve.curve);
				break;
			case DMKCurvePropertyType.RandomVal:
				EditorGUILayout.BeginVertical();
				curve.value = EditorGUILayout.FloatField(label, curve.value);
				curve.valEnd = EditorGUILayout.FloatField(" ", curve.valEnd);
				EditorGUILayout.EndVertical();
				break;
			}
			curve.type = (DMKCurvePropertyType)EditorGUILayout.EnumPopup(curve.type, GUILayout.Width(64));
			EditorGUILayout.EndHorizontal();
		}


		public static void DrawSceneRect(Rect r, float z) {
			Handles.DrawLine(new Vector3(r.x, r.y, z) ,
			                 new Vector3(r.x, r.y + r.height, z));
			Handles.DrawLine(new Vector3(r.x, r.y + r.height, z) ,
			                 new Vector3(r.x + r.width, r.y + r.height, z));
			Handles.DrawLine(new Vector3(r.x + r.width, r.y + r.height, z) ,
			                 new Vector3(r.x + r.width, r.y, z));
			Handles.DrawLine(new Vector3(r.x + r.width, r.y, z) ,
			                 new Vector3(r.x, r.y, z));
		}
#endif
		public static void DrawTextureWithTexCoordsAndColor(Rect dst, Texture tex, Rect coords, Color c) {
			Color gc = GUI.color;
			GUI.color = c;
			GUI.DrawTextureWithTexCoords(dst, 
			                             tex, 
			                             coords,
			                             true);
			GUI.color = gc;
		}

		public static void DrawTextureAt(Texture tex, Rect dst, Color c) {
			Color gc = GUI.color;
			GUI.color = c;
			GUI.DrawTexture(dst, tex, ScaleMode.ScaleToFit, true);
			GUI.color = gc;
		}

	};

	
}
