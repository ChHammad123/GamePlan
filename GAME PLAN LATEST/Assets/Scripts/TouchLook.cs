using UnityEngine;
using System.Collections;

public class TouchLook : MonoBehaviour {
	public float xRange = 60f, yRange = 90f;

	void Update() {
		float h = 0f, v = 0f;
		if ( Input.touchCount > 0 ) {
			var touch = Input.touches[0];
			h = touch.position.x;
			v = touch.position.y;
		} else {
			h = Input.mousePosition.x;
			v = Input.mousePosition.y;
		}
		h = Mathf.Clamp(h, 0, Screen.width) / Screen.width;
		v = Mathf.Clamp(v, 0, Screen.height) / Screen.height;
		h-= 0.5f;
		v-= 0.5f;

		transform.eulerAngles = new Vector3(
			-v * xRange, h * yRange, 0f
			);
	}
}

