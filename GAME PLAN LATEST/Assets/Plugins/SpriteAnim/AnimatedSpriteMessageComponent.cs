using UnityEngine;

public class AnimatedSpriteMessageComponent : MonoBehaviour {

	public void DisableBehaviour(string behaviour) {
		Component c = gameObject.GetComponent(behaviour);
		if ( c ) {
			MonoBehaviour mb = c as MonoBehaviour;
			if ( mb ) {
				mb.enabled = false;
			} else {
				Debug.LogError("Type '"+behaviour+"' can't be disabled");
			}
		}
	}

	public void EnableBehaviour(string behaviour) {
		Component c = gameObject.GetComponent(behaviour);
		if ( c ) {
			MonoBehaviour mb = c as MonoBehaviour;
			if ( mb ) {
				mb.enabled = true;
			} else {
				Debug.LogError("Type '"+behaviour+"' can't be enabled");
			}
		}
	}


}

