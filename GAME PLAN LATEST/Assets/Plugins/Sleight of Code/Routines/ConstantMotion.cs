using UnityEngine;
using System.Collections;

public class ConstantMotion : SOCBehaviour {
	public Vector3 motion;

	[DisplayIf("actAsVelocity",false)]
	public bool affectLocalPosition;

	public bool applyLocalDirection;
	public bool actAsForce;
	public bool actAsVelocity;

	[DisplayIf("actAsVelocity",true)]
	public float velocityCatchUpRate = 1f;

	Vector3 totalForce;

	// Update is called once per frame
	void Update() {
		Vector3 actualMotion = motion;
		if ( applyLocalDirection ) actualMotion = tx.localRotation * motion;
		if ( actAsForce ) {
			totalForce += actualMotion * Time.deltaTime;
			actualMotion = totalForce;
		}
		if ( actAsVelocity ) {
			if ( rb ) {
				rb.velocity = Vector3.Lerp(rb.velocity, actualMotion, velocityCatchUpRate*Time.deltaTime);
			}
			if ( rb2 ) {
				rb2.velocity = Vector3.Lerp(rb2.velocity, actualMotion, velocityCatchUpRate*Time.deltaTime);
			}
		} else {
			actualMotion *= Time.deltaTime;
			if ( affectLocalPosition ) {
				tx.localPosition += actualMotion;
			} else {
				tx.position += actualMotion;
			}
		}
	}
}

