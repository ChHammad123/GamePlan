using UnityEngine;
using System.Collections;

public class MoveTowards : SOCBehaviour {
	public Transform target;
	public float speed = 2f;
	public bool useLerp;

	float progress;
	// Update is called once per frame
	void Update() {
		if ( target ) {
			if ( useLerp ) {
				progress += speed * Time.deltaTime;
				tx.position = Vector3.Lerp(tx.position,target.position,progress);
			} else {
				tx.position = Vector3.MoveTowards(tx.position,target.position,speed*Time.deltaTime);
			}
		}
	}
}

