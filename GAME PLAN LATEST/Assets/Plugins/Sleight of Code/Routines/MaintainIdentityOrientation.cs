using UnityEngine;

public class MaintainIdentityOrientation : SOCBehaviour {
	[DisplayIf("inLateUpdate",false)]
	public bool inUpdate=true;
	[DisplayIf("inUpdate",false)]
	public bool inLateUpdate=false;
	[DisplayIf("maintainSpecific",false)]
	public bool maintainStartRot=false;
	[DisplayIf("maintainStartRot",false)]
	public bool maintainSpecific=false;

	[DisplayIf("maintainSpecific",true)]
	public Vector3 targetRotation;

	Quaternion rot;
	protected override void Awake() {
		base.Awake();
		if ( maintainStartRot ) {
			rot = tx.rotation;
		} else {
			if ( maintainSpecific ) {
				rot = Quaternion.Euler(targetRotation);
			} else {
				rot = Quaternion.identity;
			}
		}
	}
	void Update() {
		if ( inUpdate ) tx.rotation = rot;
	}
	void LateUpdate() {
		if ( inLateUpdate ) tx.rotation = rot;
	}
}

