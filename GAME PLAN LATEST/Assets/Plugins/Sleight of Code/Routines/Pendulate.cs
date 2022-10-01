using UnityEngine;
using System.Collections;

public class Pendulate : MonoBehaviour {
	public Vector3 range;
	public Vector3 speed;
	public Vector3 timeOffset;

	public bool local, useRigidbody;

	Rigidbody rb;
	Transform tx;
	void Awake() {
		rb = GetComponent<Rigidbody>();
		tx = transform;
		timeOffset *= Mathf.PI * 2f;
	}
	void Update () {
		Quaternion euler = Quaternion.Euler(
			Mathf.Sin(Time.time * speed.x + timeOffset.x)*range.x,
			Mathf.Sin(Time.time * speed.y + timeOffset.y)*range.y,
			Mathf.Sin(Time.time * speed.z + timeOffset.z)*range.z
			);
		if ( useRigidbody ) {
			rb.rotation = euler;
		} else {
			if ( local ) {
				tx.localRotation = euler;
			} else {
				tx.rotation = euler;
			}
		}
	}
}
