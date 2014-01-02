using UnityEngine;
using System;

[Serializable]
public class DMKSettings: MonoBehaviour {

	public static string Version = "0.1";
	public static DMKSettings instance {
		get {
			DMKSettings settings = (DMKSettings)GameObject.FindObjectOfType<DMKSettings>();
			if(!settings) {
				GameObject settingsObj = new GameObject();
				settings = settingsObj.AddComponent<DMKSettings>();
				settingsObj.name = "DanmakuX";
				UnityEditor.EditorUtility.SetDirty(settingsObj);
			}
			return settings;
		}
	}

	public int targetFPS = 60;
	public float frameInterval = 1f / 60;
	public int pixelPerUnit = 100;
	public float unitPerPixel = 1f / 100;

	public Camera mainCamera;
	public bool useCustomOrthoSize = false;
	public float centerOffsetX = 0;
	public float centerOffsetY = 0;
	public float orthoSizeVertical = 8;
	public float orthoSizeHorizontal = 6;

	public void Awake() {

	}

	public bool CheckNeedInternalTimer() {
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

	public Rect GetCameraRect() {
		Camera camera = mainCamera;
		if(camera == null)
			camera = Camera.main;
		if(!camera.isOrthoGraphic)
			Debug.LogError("DMKSettings: No valid orthographic caemra found, please assign a orthographic camera in settings");

		Vector3 pos = Camera.main.transform.position;
		float   orthoV = useCustomOrthoSize ? orthoSizeVertical : Camera.main.orthographicSize;
		float   orthoH = useCustomOrthoSize ? orthoSizeHorizontal : Camera.main.orthographicSize * Camera.main.aspect;
		return new Rect(pos.x - orthoH, 
		                pos.y - orthoV, 
		                orthoH * 2, 
		                orthoV * 2);
	}
}
