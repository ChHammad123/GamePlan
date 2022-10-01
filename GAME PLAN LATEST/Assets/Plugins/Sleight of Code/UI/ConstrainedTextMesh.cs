using UnityEngine;
using System.Collections;
using System.Reflection;

public class ConstrainedTextMesh : MonoBehaviour {
	public string text {
		get { return mesh.text; }
		set { 
			mesh.text = value; 
			sizer.FitToWidth(targetWidth);
		}
	}
	public float targetWidth;

	TextMesh mesh;
	TextSize sizer;
//	string lastText = "";

	void Awake() {
		mesh = GetComponentInChildren<TextMesh>();
		if ( !mesh ) {
			Debug.LogError("No text mesh in children of " + name);
			Destroy(this);
		} else {
			sizer = new TextSize(mesh);
		}
	}

}

