using UnityEngine;
using System.Collections;

public class ConstantScale : SOCBehaviour {
	public Vector3 speed;

	void Update() {
		tx.localScale += speed * Time.deltaTime;
	}
}

