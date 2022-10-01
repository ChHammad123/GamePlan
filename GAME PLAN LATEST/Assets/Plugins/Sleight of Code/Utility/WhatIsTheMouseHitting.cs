using UnityEngine;
using System.Collections;

public class WhatIsTheMouseHitting : MonoBehaviour {
	public Camera from;

	void Update() {

		if ( Input.GetKeyDown(KeyCode.Mouse0) ) {
			if ( !from ) from = Camera.main;
			Ray ray = from.ScreenPointToRay(Input.mousePosition);
			var stuff = Physics.RaycastAll(ray);
			foreach(var thing in stuff) {
				Debug.Log("Mouse ray hits " + thing.collider.name);
			}
		}
	}
}

