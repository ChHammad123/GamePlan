using UnityEngine;
using System.Collections;

public class EventStuff : MonoBehaviour {
	public void EnableObject() {
		gameObject.SetActive(true);
	}
	public void DisableObject() {
		gameObject.SetActive(false);
	}
}
