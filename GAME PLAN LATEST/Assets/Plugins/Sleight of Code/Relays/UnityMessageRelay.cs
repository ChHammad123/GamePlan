using UnityEngine;
using System.Collections;

public class UnityMessageRelay : MonoBehaviour {
	public delegate void Relay(UnityMessageRelay umr);

	public event Relay onDisable;

	void OnDisable() {
		if ( null != onDisable ) onDisable(this);
	}

}

