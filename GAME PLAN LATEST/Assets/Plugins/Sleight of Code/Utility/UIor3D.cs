using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class UIor3DText {
	//public static implicit operator Text(UIor3DText uo3) { return uo3.uiText; }
	//public static implicit operator TextMesh(UIor3DText uo3) { return uo3.textMesh; }
	public static implicit operator bool(UIor3DText uo3) { return null != uo3 && (uo3.uiText || uo3.textMesh); }

	public Text uiText;
	public TextMesh textMesh;

	/*
	public bool limitSize;
	public bool squeezeWidth;

	[DisplayIf("limitSize",true)]
	public float sizeLimit;

	TextSize sizer = null;
	/**/

	public Color color {
		get {
			if ( uiText ) return uiText.color;
			return textMesh.GetComponent<Renderer>().material.color;
		}
		set {
			if ( uiText ) uiText.color = value;
			if ( textMesh ) textMesh.GetComponent<Renderer>().material.color = value;
		}
	}

	public string text {
		get {
			if ( uiText ) return uiText.text;
			return textMesh.text;
		}
		set {
			if ( uiText ) uiText.text = value;
			if ( textMesh ) {
				textMesh.text = value;
				/*
				if ( limitSize ) {
					if ( null == sizer ) sizer = new TextSize(textMesh);
					sizer.FitToWidth(sizeLimit);
				}
				/**/
			}
		}
	}
}

[System.Serializable]
public class UIor3DSprite {
	public static implicit operator SpriteRenderer(UIor3DSprite uo3) { return uo3.spriteRenderer; }
	public static implicit operator Image(UIor3DSprite uo3) { return uo3.image; }

	public void SetSpriteNullable(Sprite sprite) {
		if ( image ) {
			image.sprite = sprite;
			if ( null == sprite ) image.color = Color.clear;
			else image.color = Color.white;
		} else {
			spriteRenderer.sprite = sprite;
		}
	}

	public SpriteRenderer spriteRenderer;
	public Image image;

	public Sprite sprite {
		get {
			if ( image ) return image.sprite;
			return spriteRenderer.sprite;
		}
		set {
			if ( image ) image.sprite = value;
			if ( spriteRenderer ) spriteRenderer.sprite = value;
		}
	}

}
