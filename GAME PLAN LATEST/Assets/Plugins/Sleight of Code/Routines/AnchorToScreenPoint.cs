using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AnchorToScreenPoint : MonoBehaviour {
	//public bool findCamera = false;
	//public string cameraName;
	public Camera ofCamera;

	public Vector3 position;
	public bool lateUpdateInsteadOfUpdate;

	public bool disregardZ = true;


	Transform tx;
	void Start() {
#if !UNITY_EDITOR
		tx = transform; 
#endif
		/*
		if ( findCamera ) {
			GameObject ob = GameObject.Find(cameraName);
			if ( ob ) {
				ofCamera = ob.GetComponent<Camera>();
			}
			if ( null == ofCamera ) {
				Debug.LogWarning("Couldn't find camera " + cameraName + " for anchoring.");
			}
		}
		*/
	}
	void Update() {
		if ( !lateUpdateInsteadOfUpdate ) {
			UpdatePosition();
		}
	}

	void UpdatePosition() {
		if ( ofCamera ) {
			float zPos = transform.position.z;
#if UNITY_EDITOR
			Vector3 from = new Vector3(position.x*Screen.width, position.y*Screen.height, position.z);
			Vector3 pos = ofCamera.ScreenToWorldPoint(from);
			if ( pos != transform.position ) transform.position = pos;

#else
			tx.position = ofCamera.ScreenToWorldPoint(new Vector3(position.x*Screen.width, position.y*Screen.height, position.z));
#endif
			if ( disregardZ ) {
				transform.position = transform.position.WithZ(zPos);
			}
		} else {
		}
	}

	void LateUpdate() {
		if ( lateUpdateInsteadOfUpdate ) {
			UpdatePosition();
		}
	}

}

