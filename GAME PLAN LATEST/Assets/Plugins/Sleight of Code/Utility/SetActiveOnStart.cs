using UnityEngine;
using System.Collections;

public class SetActiveOnStart : MonoBehaviour {
	public GameObject[] setActive;
	public GameObject[] setInactive;

	void Start() {
		foreach(var thing in setActive) thing.SetActive(true);
		foreach(var thing in setInactive) thing.SetActive(false);
	}
}

