using UnityEngine;
using System.Collections;

public class FaceDirectionOfMovement : MonoBehaviour {
	Vector3 lastPosition;
	Transform tx;

	public Vector3 forward = Vector3.forward;

	public float arrowSize = 2f;

	public bool backwards;

	void Start() {
		tx = transform;
	}
	void Update() {
		Vector3 dif = tx.position - lastPosition;
		if ( dif.sqrMagnitude == 0 ) return;
		Vector3 currentForward = transform.TransformDirection(forward);
		Quaternion rot = Quaternion.FromToRotation(currentForward, backwards ? -dif : dif);
		transform.rotation = rot * transform.rotation;

		lastPosition = tx.position;
	}

	void OnDrawGizmosSelected() {
		GUIUtils.Gizmos.DrawArrow(transform.position, transform.position + forward * arrowSize);
	}
}

