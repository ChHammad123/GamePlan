using UnityEngine;
using System.Collections;

public class ConstantRotate : MonoBehaviour {
	public Vector3 rotation, variation;
	public bool local = false;

	private Vector3 realRotation;

	private void Start() {
		realRotation = new Vector3(
			rotation.x + Random.Range(-variation.x, variation.x),
			rotation.y + Random.Range(-variation.y, variation.y),
			rotation.z + Random.Range(-variation.z, variation.z)
			);
	}

	void Update() {
		if ( local ) {
			if ( GetComponent<Rigidbody>() && !GetComponent<Rigidbody>().isKinematic ) {
				GetComponent<Rigidbody>().AddRelativeTorque(realRotation * Time.deltaTime);
			} else {
				transform.Rotate( realRotation * Time.deltaTime );
			}
		} else {
			if ( GetComponent<Rigidbody>() && !GetComponent<Rigidbody>().isKinematic ) {
				GetComponent<Rigidbody>().AddTorque(realRotation * Time.deltaTime);
			} else {
				transform.Rotate( realRotation * Time.deltaTime, Space.World );
			}
		}
	}

	public void SetRotation(Vector3 rotation) {
		realRotation = rotation;
	}

	public void ReverseRotation() {
		realRotation = -realRotation;
	}

}


