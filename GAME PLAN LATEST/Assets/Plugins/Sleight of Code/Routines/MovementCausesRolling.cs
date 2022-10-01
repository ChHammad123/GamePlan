using UnityEngine;


[ExecuteInEditMode]
public class MovementCausesRolling : MonoBehaviour {
	public float radius;

	Vector3 lastPosition;
	Transform tx;
	float lastRealTime;
	void Start() { tx = transform; lastPosition = tx.position; lastRealTime = Time.realtimeSinceStartup; }
	void Update() {
		Vector3 dif = tx.position - lastPosition;

		Vector3 left = Vector3.Cross(dif,Vector3.up);
		float t = Time.deltaTime;
		if ( t == 0f ) {
			t = Time.realtimeSinceStartup - lastRealTime;
			lastRealTime = Time.realtimeSinceStartup;
		}
		float v = dif.magnitude / t;
		float w = v / radius;

		tx.Rotate(left, -w, Space.World);
		lastPosition = tx.position;
	}
}
