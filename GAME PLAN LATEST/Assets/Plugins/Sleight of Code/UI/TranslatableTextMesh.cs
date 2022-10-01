using UnityEngine;
using System.Collections;

public class TranslatableTextMesh : MonoBehaviour {
	public Translatable value;

	TextMesh myText;
	void Start() {
		myText = GetComponentInChildren<TextMesh>();
		if ( !myText ) {
			Debug.LogError("Text mesh not found");
			Destroy(this);
		}
		myText.text = value;
	}
}

