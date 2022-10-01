using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ConformToBounds : SOCBehaviour {
	// TODO: Only works for non-rotated objects. Use rotation to calculate new scale (maybe?)
	public Transform minBounds;
	public Transform maxBounds;

	Vector3 lastMin, lastMax;

	Renderer r;

	void Start() {
		r = GetComponent<Renderer>();
		if ( null == r ) {
			Destroy(this);
			Debug.LogError("Conform To Bounds can only run on a renderer!");
		}
	}

	void Update() {
		if ( lastMin == minBounds.position && lastMax == maxBounds.position ) return;
		lastMin = minBounds.position;
		lastMax = maxBounds.position;

		Vector3 t = maxBounds.position - minBounds.position;
		Vector3 targetPosition = (maxBounds.position + minBounds.position) / 2f;
		if ( t.x < 0f ) t.x = -t.x;
		if ( t.y < 0f ) t.y = -t.y;
		if ( t.z < 0f ) t.z = -t.z;
		Vector3 c = r.bounds.size;

		Vector3 scaling = new Vector3(
			t.x / c.x, t.y / c.y, t.z / c.z
			);

		Vector3 currentScale = tx.localScale;
		currentScale.Scale(scaling);

		tx.position = targetPosition;
		tx.localScale = currentScale;
	}
}

