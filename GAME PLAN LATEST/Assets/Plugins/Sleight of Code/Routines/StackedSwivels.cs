using UnityEngine;
using System.Collections;

public class StackedSwivels : MonoBehaviour {
	public enum Mode {
		Manual, FollowTargetPosition, CopyTargetRotation
	}
	public float swivelSpeed = 90f;
	public Transform rotateThisOnYAxis, rotateThisOnXAxis, rotateThisOnZAxis;

	public Mode mode;

	[DisplayIf("mode","Manual",false)]
	public Transform target;

	void Update() {
		if ( !target ) return;
		switch(mode) {
			case Mode.CopyTargetRotation:
				Aim(transform.position + target.forward);
				break;
			case Mode.FollowTargetPosition:
				Aim(target.position);
				break;
		}
	}

	public virtual void Aim(Vector3 at) {
		Quaternion angles;
		if ( rotateThisOnXAxis ) {
			angles = Quaternion.LookRotation((at-rotateThisOnXAxis.position).WithX(0f));
			rotateThisOnXAxis.eulerAngles = rotateThisOnXAxis.eulerAngles.WithX(Quaternion.RotateTowards(rotateThisOnXAxis.rotation, angles, swivelSpeed*Time.deltaTime).eulerAngles.x);
		}
		if ( rotateThisOnYAxis ) {
			angles = Quaternion.LookRotation((at-rotateThisOnYAxis.position).WithY(0f));
			rotateThisOnYAxis.eulerAngles = rotateThisOnYAxis.eulerAngles.WithY(Quaternion.RotateTowards(rotateThisOnYAxis.rotation, angles, swivelSpeed*Time.deltaTime).eulerAngles.y);
//			rotateThisOnYAxis.rotation = Quaternion.RotateTowards(rotateThisOnYAxis.rotation, angles, swivelSpeed*Time.deltaTime);
		}
		if ( rotateThisOnZAxis ) {
			angles = Quaternion.LookRotation((at-rotateThisOnZAxis.position).WithZ(0f));
			rotateThisOnZAxis.eulerAngles = rotateThisOnZAxis.eulerAngles.WithZ(Quaternion.RotateTowards(rotateThisOnZAxis.rotation, angles, swivelSpeed*Time.deltaTime).eulerAngles.z);
//			rotateThisOnZAxis.rotation = Quaternion.RotateTowards(rotateThisOnZAxis.rotation, angles, swivelSpeed*Time.deltaTime);
		}
	}
	
}

