using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {
	public bool backwards = false;

	Transform tx;
	void Start() { tx = transform; }

	// Update is called once per frame
	void Update() {
		if ( backwards ) {
			tx.forward = (tx.position - Camera.main.transform.position);
		} else {
			tx.forward = (Camera.main.transform.position - tx.position);
		}
	}
}

