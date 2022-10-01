using UnityEngine;
using System.Collections;

public class VFX : Pooler {
	public float destroyAfterTime = 1f;
	public bool beginCountdownOnEnable = false;

	protected virtual void OnEnable() {
		if ( beginCountdownOnEnable && destroyAfterTime > 0f ) {
			PoolMaster.Destroy(gameObject, destroyAfterTime);
		}
	}

	public virtual void ParentSuiciding() {
		transform.parent = null;
		PoolMaster.Destroy(gameObject, destroyAfterTime);
	}

}

