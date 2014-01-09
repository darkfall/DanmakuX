using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(DMKSettings))]
public class DMKSettingsInspector: Editor {
		
	public override void OnInspectorGUI() {
		DMKSettingsEditor.SettingsGUI();
	}
	
	public void OnSceneGUI() {
		Rect r = DMKSettings.instance.GetCameraRect();
		Camera cam = DMKSettings.instance.mainCamera;
		Vector3[] verts = new Vector3[4] { 
			new Vector3(r.x, r.y, cam.transform.position.z),
			new Vector3(r.x, r.y + r.height, cam.transform.position.z),
			new Vector3(r.x + r.width, r.y + r.height, cam.transform.position.z),
			new Vector3(r.x + r.width, r.y, cam.transform.position.z)
		};
		Handles.DrawSolidRectangleWithOutline(verts, new Color(0.1f, 0.1f, 0.1f, 0.1f), Color.green);
	}

}

