using UnityEngine;
using System;

public class DMKSettings {

	[SerializeField]
	public static int targetFPS = 60;
	[SerializeField]
	public static int pixelPerUnit = 100;
	[SerializeField]
	public static float unitPerPixel = 1f / 100;

	public static bool CheckNeedInternalTimer() {
		Application.targetFrameRate = -1;
		if(targetFPS == Application.targetFrameRate)
			return false;
		if(Application.targetFrameRate != -1 && targetFPS > Application.targetFrameRate) {
			Debug.LogError("Application.targetFrameRate is lower than targetFPS, lowering targetFPS to " + targetFPS.ToString());
			targetFPS = Application.targetFrameRate;
			return false;
		}
		return true;
	}
}
