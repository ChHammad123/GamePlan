using UnityEngine;
using System.Collections;

public class OnStartNavigateToPoint : MonoBehaviour {
	public Vector3 point;
	// Use this for initialization
	void Start() {
		UnityEngine.AI.NavMeshAgent nma = GetComponent<UnityEngine.AI.NavMeshAgent>();
		nma.SetDestination(point);
	}

	void OnDrawGizmos() {
		Gizmos.DrawLine(transform.position, point);
	}
	
}

