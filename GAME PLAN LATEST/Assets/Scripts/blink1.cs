using UnityEngine;
using System.Collections;

public class blink1 : MonoBehaviour {
	private SpriteRenderer sp;
	// Use this for initialization
   
	void Start () {
		sp = this.gameObject.GetComponent<SpriteRenderer> ();
		InvokeRepeating ("blinking",0.15f,0.15f);
	}

	void blinking ()
	{ 
		if (this.gameObject.activeInHierarchy) {
			sp.enabled = !(sp.enabled);
		}
	}
}
