using UnityEngine;
using System.Collections;

public class NavMeshAgentEvents : MonoBehaviour {
	public delegate void Callback(UnityEngine.AI.NavMeshAgent caller);

	public event Callback 
		onDestinationReached,
		onStatusChanged,
		onPathAcquired,
		onPathLost,
		onPathStale,
		onNewDestination;

	UnityEngine.AI.NavMeshAgent agent;

	void Start() {
		agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
	}
	Vector3 lastDestination;
	bool hadPath, wasStale;
	UnityEngine.AI.NavMeshPathStatus lastStatus;

	void Update() {
		if ( null != onNewDestination ) {
			if ( agent.destination != lastDestination ) onNewDestination(agent);
			lastDestination = agent.destination;
		}
		if ( null != onPathStale ) {
			if ( !wasStale && agent.isPathStale ) onPathStale(agent);
			wasStale = agent.isPathStale;
		}
		if ( null != onPathLost ) {
			if ( hadPath && !agent.hasPath ) onPathLost(agent);
			hadPath = agent.hasPath;
		}
		if ( null != onStatusChanged ) {
			if ( lastStatus != agent.pathStatus ) onStatusChanged(agent);
			lastStatus = agent.pathStatus;
		}
		if ( null != onDestinationReached ) {
			if ( agent.remainingDistance == 0f ) onDestinationReached(agent);
		}
		if ( null != onPathAcquired ) {
			if ( !hadPath && agent.hasPath ) onPathAcquired(agent);
		}
	}
}

