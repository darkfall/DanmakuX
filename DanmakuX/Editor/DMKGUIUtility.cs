using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class DMKGUIUtility {

	public static GUIStyle boxStyle;
	public static GUIStyle listEntryNormal;
	public static GUIStyle listEntryFocused;

	public static bool Initialized = false;

	static void Init() {
		boxStyle = new GUIStyle(GUI.skin.box);

		listEntryNormal = new GUIStyle(GUI.skin.label);
		listEntryNormal.alignment = TextAnchor.MiddleCenter;

		listEntryFocused = new GUIStyle(GUI.skin.label);
		listEntryFocused.alignment = TextAnchor.MiddleCenter;
		SetStyleTextColor(listEntryFocused, new Color(0, 1, 0, 1));

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
		if(!Initialized) {
			DMKGUIUtility.Init();
		}
		EditorGUILayout.BeginVertical(boxStyle);
		int newSelectedIndex = -1;
		{
			for(int index=0; index<entries.Count; ++index) {

				if(GUILayout.Button((string)entries[index],
				                    selectedIndex == index ? DMKGUIUtility.listEntryFocused : DMKGUIUtility.listEntryNormal)) {
					newSelectedIndex = index;
				}
			} 

		}

		EditorGUILayout.EndVertical();

		return newSelectedIndex == -1 ? selectedIndex : newSelectedIndex;
	}

};