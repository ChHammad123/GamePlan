using UnityEngine;
using System.Collections;

public class ColliderMessageRelay : MonoBehaviour {
	public delegate void CollisionRelay(ColliderMessageRelay cmr, Collision col);
	public delegate void ColliderRelay(ColliderMessageRelay cmr, Collider col);

	public event CollisionRelay
		onCollisionEnter=null,
		onCollisionStay=null,
		onCollisionExit=null;
	public event ColliderRelay
		onTriggerEnter=null,
		onTriggerStay=null,
		onTriggerExit=null;



	void OnTriggerEnter(Collider c) { CallIfNotNull(onTriggerEnter,c); }
	void OnTriggerExit(Collider c) { CallIfNotNull(onTriggerExit,c); }
	void OnTriggerStay(Collider c) { CallIfNotNull(onTriggerStay,c); }

	void OnCollisionEnter(Collision c) { CallIfNotNull(onCollisionEnter,c); }
	void OnCollisionExit(Collision c) { CallIfNotNull(onCollisionExit,c); }
	void OnCollisionStay(Collision c) { CallIfNotNull(onCollisionStay,c); }

	void CallIfNotNull(CollisionRelay r, Collision c) {
		if ( null != r ) r(this, c);
	}
	void CallIfNotNull(ColliderRelay r, Collider c) {
		if ( null != r ) r(this, c);
	}
}

