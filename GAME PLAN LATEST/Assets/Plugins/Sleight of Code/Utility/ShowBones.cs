using UnityEngine;
using System.Collections;

public class ShowBones : MonoBehaviour {
	public bool always = true;

	void OnDrawGizmos() {
		if ( always ) OnDrawGizmosSelected();
	}

	void OnDrawGizmosSelected() {
		Transform[] Q = new Transform[]{transform};
		while(Q.Length > 0 ) {
			foreach(Transform t in Q[0]) {
				Gizmos.DrawLine(t.position, Q[0].position);
				Debug.DrawLine(t.position,Q[0].position);
				Q = Q.With(t);
			}
			Q = Q.WithoutIndices(0);
		}
	}

}

