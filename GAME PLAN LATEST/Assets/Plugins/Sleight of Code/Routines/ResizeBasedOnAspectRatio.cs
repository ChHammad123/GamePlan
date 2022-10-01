using UnityEngine;
using System.Collections;

public class ResizeBasedOnAspectRatio : MonoBehaviour {
	public Vector2 targetAspect;
	public Vector3 effect = Vector3.one;

	Vector3 original;
	float target;
	void Start() {
		original = transform.localScale;
		target = targetAspect.x / targetAspect.y;
	}
	// Update is called once per frame
	public float current;
	void Update() {
		float aspect = (float)Screen.width / (float)Screen.height;
		current = aspect / target;
		transform.localScale = 
			UtilityExtensions.Multiop(
				(float[] inp)=>{return Mathf.Lerp(inp[0],inp[0]*current,inp[1]);},
			original, effect);
	}
}

