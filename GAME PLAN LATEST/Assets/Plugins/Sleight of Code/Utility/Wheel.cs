
using UnityEngine;
using System.Collections;

public class Wheel : SOCBehaviour {
	public WheelCollider wheelCollider;
	public Transform wheelModel;

	// Use this for initialization
	void Start() {
	
	}
	
	// Update is called once per frame
	void Update() {
		WheelHit wh;
		if ( wheelCollider.GetGroundHit(out wh)) {
			wheelModel.position = wh.point + wh.normal * wheelCollider.radius;
		} else {
			wheelModel.localPosition = Vector3.Lerp(wheelModel.localPosition, wheelCollider.transform.localPosition, Time.deltaTime*2f);
		}
		wheelModel.Rotate(tx.right * (wheelCollider.rpm * 6f) * Time.deltaTime, Space.World);
	}
}

