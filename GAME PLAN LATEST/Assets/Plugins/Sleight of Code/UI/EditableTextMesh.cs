using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class EditableTextMesh : MonoBehaviour {
	public string text { 
		get { if ( textMesh ) return textMesh.text; else return ""; }
		set { 
			if ( textMesh ) {
				textMesh.text = value;
				insertAt = value.Length;
				previousText = value;
			}
		}
	}

	public int maxLength = int.MaxValue;
	public bool
		allowAlpha = true,
		allowSymbol = true,
		allowNum = true,
		escapable = true;

//	static float iBeamRate = 0.3f;

	bool iBeamOn = false;
//	float iBeamTimer = 0.0f;

	static EditableTextMesh activeOne = null;
	public bool Active {
		get { 
			return isActive;
		}
		set {
			if ( isActive == value ) return;
			if ( null != activeOne ) {
				if ( activeOne.iBeamOn ) {
					activeOne.textMesh.text.Remove(activeOne.insertAt,1);
					activeOne.iBeamOn = false;
				}
				EditableTextMesh disableMe = activeOne;
				activeOne = null;
				disableMe.Active = false;
			}
			isActive = value;
			activeOne = this;
			if ( isActive ) {
				if ( textMesh ){
					previousText = textMesh.text;
					insertAt = previousText.Length;
				}
			}
		}
	}
	int insertAt;
	bool isActive;
	bool wantsFocus;
	string previousText;
	TextMesh textMesh;

	public static EditableTextMesh Create(TextMesh onText) {
		EditableTextMesh otf = onText.GetOrAddComponent<EditableTextMesh>();
		otf.escapable = true;
		return otf;
	}

	void Awake() {
		textMesh = GetComponent<TextMesh>();
	}

	void OnDisable() {
		if ( activeOne == this ) activeOne = null;
	}

	void Update() {
		if ( escapable && Input.GetKeyDown(KeyCode.Escape) ) {
			textMesh.text = previousText;
			Active = false;
		}
		if ( Input.GetKeyDown(KeyCode.Return) ) {
			Active = false;
		}
		if ( isActive ) {
			if ( iBeamOn ) textMesh.text = textMesh.text.Remove(insertAt,1);

			if ( Input.GetKeyDown(KeyCode.LeftArrow) ) insertAt--;
			if ( Input.GetKeyDown(KeyCode.RightArrow) ) insertAt++;
			if ( Input.GetKeyDown(KeyCode.Home) ) insertAt = 0;
			if ( Input.GetKeyDown(KeyCode.End) ) insertAt = textMesh.text.Length;
			insertAt = Mathf.Clamp(insertAt, 0, textMesh.text.Length);
			if ( Input.GetKeyDown(KeyCode.Delete) ) {
				if ( insertAt < textMesh.text.Length ) textMesh.text = textMesh.text.Remove(insertAt,1);
			}
			foreach(char c in Input.inputString) {
				switch(c) {
					case '\b':
						if ( insertAt > 0 ) {
							textMesh.text = textMesh.text.Remove(insertAt-1,1);
							insertAt--;
						}
						break;
					case '\n': case '\r':
						Active = false;
						break;
					default:
						textMesh.text = textMesh.text.Insert(insertAt,c.ToString());
						insertAt++;
						break;
				}
			}
			if ( textMesh.text.Length > maxLength ) textMesh.text = textMesh.text.Substring(0,maxLength);
			/* doesn't work with characters inserted, needs a separate graphic
			iBeamTimer -= Time.deltaTime;
			if ( iBeamTimer < 0f ) {
				iBeamTimer += iBeamRate;
				iBeamOn = !iBeamOn;
				if ( iBeamOn ) {
					textMesh.text = textMesh.text.Insert(insertAt, "|");
				} else {
					textMesh.text = textMesh.text.Remove(insertAt, 1);
				}
			}
			/**/
		}
	}


}

