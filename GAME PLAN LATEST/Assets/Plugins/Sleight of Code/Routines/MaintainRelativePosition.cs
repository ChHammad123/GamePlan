using UnityEngine;
using System.Collections;

public class MaintainRelativePosition : SOCBehaviour {

	public Transform toTarget;
	public Vector3 position;

	public bool inUpdate = true;
	public bool inLateUpdate;

	void Update() {
		if ( inUpdate ) tx.position = toTarget.position + position;
	}
	void LateUpdate() {
		if ( inLateUpdate ) tx.position = toTarget.position + position;
	}
}

