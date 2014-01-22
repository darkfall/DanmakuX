using System;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;

#endif

namespace danmakux {

	public class DMKUtil {

		public static float GetDgrBetweenObjects(GameObject obj1, GameObject obj2) {
			Vector3 p1 = obj1.transform.position;
			Vector3 p2 = obj2.transform.position;
			Vector3 dist = p2 - p1;
			return Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
		}

		public static AnimationCurve NewCurve(float v1, float v2) {
			AnimationCurve curve = new AnimationCurve();
			curve.AddKey(new Keyframe(0, v1));
			curve.AddKey(new Keyframe(1, v2));
			
			return curve;
		}

		public static AnimationCurve CopyCurve(AnimationCurve curve) {
			AnimationCurve result = new AnimationCurve();
			foreach(Keyframe key in curve.keys) {
				result.AddKey(key);
			}
			return result;
		}

		public static AnimationCurve CopyCurve(AnimationCurve curve, Func<float, float> d) {
			AnimationCurve result = new AnimationCurve();
			foreach(Keyframe key in curve.keys) {
				result.AddKey(new Keyframe(key.time, d(key.value)));
			}
			return result;
		}

		public static string[] ToStringArray(System.Collections.IList entries) {
			System.Collections.Generic.List<string> strs = new System.Collections.Generic.List<string>();
			foreach(object obj in entries)
				strs.Add(obj.ToString());
			return strs.ToArray();
		}

#if UNITY_EDITOR
		// http://forum.unity3d.com/threads/219799-Sort-Layer-Renderer-Extension
		public static string[] GetSortingLayerNames() {
			Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		}
#endif
		
	}

		
}
