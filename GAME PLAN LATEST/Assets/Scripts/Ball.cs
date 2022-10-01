using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
	void OnCollisionEnter(Collision c)
	{
		if ( c.collider.tag == "HideBall" ) 
			gameObject.SetActive(false);
	}
}

