using UnityEngine;
using System.Collections;


public class SetRenderQueueOfChildren : MonoBehaviour {
	public int ensureAbove;
	public int waitFrames = 0;
	IEnumerator Start() {
		while(waitFrames > 0) {
			waitFrames--;
			yield return null;
		}
		foreach(var x in GetComponentsInChildren<Renderer>()) {
			if ( x.sharedMaterial.renderQueue <= ensureAbove ) {
				x.material.renderQueue += ensureAbove;
			}
		}
	}
}

