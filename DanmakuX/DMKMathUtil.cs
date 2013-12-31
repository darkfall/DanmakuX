using System;
using UnityEngine;

class DMKMathUtil {
	
	public static float Clamp(float start, float end, float v) {
		if(v < start)
			v = start;
		else if(v > end)
			v = end;
		return v;
	}

	public static float GetDgrBetweenObjects(GameObject obj1, GameObject obj2) {
		Vector3 p1 = obj1.transform.position;
		Vector3 p2 = obj2.transform.position;
		Vector3 dist = p2 - p1;
		return Mathf.Atan2(dist.y, dist.x) * Mathf.Rad2Deg;
	}

}