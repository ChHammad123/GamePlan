using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class NoOffsetsOnRect : MonoBehaviour {
	RectTransform rt;
	public bool enableMove;

	void Awake() {
		rt = GetComponent<RectTransform>();
	}

	void Update() {
		if ( rt ) {
			if ( enableMove ) {
				float width = Screen.width;
				float height = Screen.height;
				// move thing to right =
				// ofmin.x gets bigger
				// ofmax.x gets smaller
				Vector2 adjust = new Vector2(
					rt.offsetMin.x / width,
					rt.offsetMin.y / height
					);

				rt.anchorMin += adjust;
				rt.anchorMax += adjust;
			}
			rt.offsetMax = Vector2.zero;
			rt.offsetMin = Vector2.zero;
		}
	}
}

