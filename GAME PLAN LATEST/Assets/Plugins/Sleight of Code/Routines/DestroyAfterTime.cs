using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour {
	public float time;

	void OnEnable() {
		PoolMaster.Destroy(gameObject,time);
	}
	
}

