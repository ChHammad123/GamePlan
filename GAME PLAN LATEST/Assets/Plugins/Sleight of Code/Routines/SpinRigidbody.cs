using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SpinRigidbody : MonoBehaviour {

	public Vector3 speed;

	public Vector3 variation;

	void Start() {
		speed.x += Random.Range(-variation.x, variation.x);
		speed.y += Random.Range(-variation.y, variation.y);
		speed.z += Random.Range(-variation.z, variation.z);
		rb = GetComponent<Rigidbody>();
	}

	Rigidbody rb;
	void FixedUpdate() {
		rb.angularVelocity = speed;
	}
}
