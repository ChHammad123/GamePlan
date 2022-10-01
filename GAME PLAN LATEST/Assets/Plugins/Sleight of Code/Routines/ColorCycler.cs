using UnityEngine;
using System.Collections;

public class ColorCycler : MonoBehaviour {

	public Color[] colors;
	public float colorsPerSecond = 1f;

	public Material material;

	public string[] setFields;
	public float timeOffset = 0f;

	int fromColor, toColor;

	void Start() { 
		if ( null == material ) material = GetComponent<Renderer>().material;
		fromColor = 0;
	}

	void Update() {
		if ( colors.Length < 1 ) return;

		float amount = Time.time * colorsPerSecond + timeOffset;
		toColor = ((int)amount) % colors.Length;
		fromColor = toColor-1;
		if ( fromColor < 0 ) fromColor = colors.Length-1;
		amount -= (int)amount;
		if ( setFields.Length==0 ) {
			material.color = Color.Lerp(colors[fromColor],colors[toColor],amount);
		} else {
			foreach(string setField in setFields) {
				material.SetColor(setField,Color.Lerp(colors[fromColor],colors[toColor],amount));
			}
		}

	}
}

