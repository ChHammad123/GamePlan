using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RelativeOffset : SOCBehaviour {
	public Vector2 minOffset, maxOffset;

	//Vector2 lastMin, lastMax;

	void Update() {
		//if ( minOffset != lastMin || maxOffset != lastMax ) {
	//		lastMin = minOffset; lastMax = maxOffset;
			float w = Screen.width, h = Screen.height;

			if ( rt.parent ) {
				var prt = rt.parent.GetComponent<RectTransform>();
				if ( prt ) {
					w = prt.rect.width;
					h = prt.rect.height;
				}
			}

			rt.offsetMin = new Vector2(minOffset.x * w, minOffset.y * h);
			rt.offsetMax = new Vector2(maxOffset.x * w, maxOffset.y * h);
		//}
	}
}

