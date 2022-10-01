using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ShadowTextMesh : MonoBehaviour {

	public Text shadowTarget;

	string lastText = null;
	Text myMesh;
	void Start()
	{
		myMesh = GetComponent<Text>(); 
	}

	void Update() 
	{
		if ( shadowTarget && lastText != shadowTarget.text ) {
			lastText = shadowTarget.text;
			myMesh.text = lastText;
		}
	}
}

