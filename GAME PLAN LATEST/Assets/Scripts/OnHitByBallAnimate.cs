using UnityEngine;
using System.Collections;

public class OnHitByBallAnimate : MonoBehaviour {
	new public string animation;


	void OnCollisionEnter(Collision c) {
		GetComponent<AnimatedSprite>().PlayAnimation(animation);
	}
}

