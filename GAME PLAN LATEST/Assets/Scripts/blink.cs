using UnityEngine;
using System.Collections;

public class blink : MonoBehaviour {
	private SpriteRenderer sp;
	// Use this for initialization
	void Start () {
		sp = this.gameObject.GetComponent<SpriteRenderer> ();
		InvokeRepeating ("blinking",0.27f,0.27f);
        
	}

	void blinking ()
	{ 
		if (this.gameObject.activeInHierarchy) {
			sp.enabled = !(sp.enabled);
		}
	}
}
