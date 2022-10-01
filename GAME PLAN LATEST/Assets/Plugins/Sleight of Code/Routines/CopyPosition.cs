using UnityEngine;
using System.Collections;

public class CopyPosition : SOCBehaviour {
	public Transform target;
	public Vector3 offset;
	public bool offsetLocalToTarget;

	// Update is called once per frame
	void Update () {
		if ( offsetLocalToTarget ) {
			tx.position = target.position + target.TransformDirection(offset);
		} else {
			tx.position = target.position+offset;
		}
	}
}
