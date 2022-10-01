using UnityEngine;
using System.Collections;

public class ShowSpeed : MonoBehaviour {
	Vector3 lastPosition;
	float speed;
	void Update() {
		float dist = (transform.position - lastPosition).magnitude;
		speed = dist / Time.deltaTime;
		lastPosition = transform.position;
	}
	void OnGUI() {
		Vector3 center = Camera.main.WorldToScreenPoint(transform.position);
		GUI.Label(new Rect(center.x,center.y,120,25),speed + "m/s");	
	}
}

