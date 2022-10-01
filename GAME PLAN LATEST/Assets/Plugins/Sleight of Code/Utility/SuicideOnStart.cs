using UnityEngine;
using System.Collections;

public class SuicideOnStart : MonoBehaviour {

	public enum Timing {
		Awake, Start, Update, OnEnable
	}
	public enum Action {
		Destroy, Disable
	}

	public Timing timing;
	public Action action;

	void Start() {
		if ( timing == Timing.Start ) Go();
	}
	
	void Update() {
		if ( timing == Timing.Update ) Go();
	}

	void Awake() {
		if ( timing == Timing.Awake ) Go();
	}

	void OnEnable() {
		if ( timing == Timing.OnEnable ) Go();
	}

	void Go() {
		if ( action == Action.Destroy ) Destroy(gameObject);
		if ( action == Action.Disable ) gameObject.SetActive(false);
	}
}

