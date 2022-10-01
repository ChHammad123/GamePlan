using UnityEngine;
using System.Collections;
#if UNITY_4_6
using UnityEngine.UI;
#endif

public class SOCBehaviour : MonoBehaviour {
	[HideInInspector] public Transform tx;
	[HideInInspector] public Rigidbody rb;
	[HideInInspector] public Rigidbody2D rb2;
	[HideInInspector] public RectTransform rt;

	protected virtual void Awake() { 
		tx = transform;
		rb = GetComponent<Rigidbody>();
		rb2 = GetComponent<Rigidbody2D>();

		rt = GetComponent<RectTransform>();
	}
	protected virtual void OnDisable() {
		foreach(VFX v in GetComponentsInChildren<VFX>()) v.ParentSuiciding();
	}
}

