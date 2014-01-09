using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class DMKGUIUtility {

	public static GUIStyle boxStyle;
	public static GUIStyle listEntryNormal;
	public static GUIStyle listEntryFocused;
	public static GUIStyle boxNoBackground;

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

	public static int MakeSimpleList(int selectedIndex, System.Collections.IList entries) {
		return MakeSimpleList(selectedIndex, entries, () => {});
	}

	public static int MakeSimpleList(int selectedIndex, System.Collections.IList entries, Action onSelectionChange) {
		if(!Initialized) {
			DMKGUIUtility.Init();
		}
		if(entries.Count == 0)
			return selectedIndex;
		
		EditorGUILayout.BeginVertical(boxStyle);
		int newSelectedIndex = -1;
		{
			for(int index=0; index<entries.Count; ++index) {
				if(GUILayout.Button(entries[index].ToString(),
				                    selectedIndex == index ? DMKGUIUtility.listEntryFocused : DMKGUIUtility.listEntryNormal)) {
					newSelectedIndex = index;
				}
			} 
		}
		
		EditorGUILayout.EndVertical();
		
		int result = newSelectedIndex == -1 ? selectedIndex : newSelectedIndex;
		if(result != selectedIndex)
			onSelectionChange();
		return result;
	}

	
	public static bool MakeCurveToggle(bool flag) {
		GUILayout.Label("Curve", GUILayout.Width(40));
		return EditorGUILayout.Toggle("", flag, GUILayout.Width(16));
	}
	
	public static void MakeCurveControl(ref DMKCurveProperty curve, string label) {
		EditorGUILayout.BeginHorizontal();
		if(curve.useCurve) {
			curve.curve = EditorGUILayout.CurveField(label, curve.curve);
		} else {
			curve.value = EditorGUILayout.FloatField(label, curve.value);
		}
		curve.useCurve = MakeCurveToggle(curve.useCurve);
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

};