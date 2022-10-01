using UnityEngine;
using System.Collections;

public class SinwaveRigidbody : MonoBehaviour {
	public Vector3 
		amount,
		speed,
		timeOffset;

	private Vector3 positionOffset;

	public bool local = true;
	Rigidbody rb;
	Transform tx, p;

	void Start() { 
		rb = GetComponent<Rigidbody>();
		tx = transform;
		p = tx.parent;
		if ( null != p ) {
			positionOffset = tx.localPosition;
		} else {
			positionOffset = Vector3.zero;
		}
	}

	void LateUpdate() {
		Vector3 v3 = new Vector3(
			Mathf.Sin(Time.fixedTime*speed.x+timeOffset.x)*amount.x,
			Mathf.Sin(Time.fixedTime*speed.y+timeOffset.y)*amount.y,
			Mathf.Sin(Time.fixedTime*speed.z+timeOffset.z)*amount.z
			);
		if ( null != rb ) {
			if ( local && null != p ) {
				rb.position = p.position + v3 + positionOffset;
			}
			else rb.position = v3 + positionOffset;
		} else {
			if ( local && null != p ) {
				tx.localPosition = v3 + positionOffset;
			} else tx.position = v3 + positionOffset;
		}
	}
}
