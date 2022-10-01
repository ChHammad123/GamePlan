using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class scripptttt : MonoBehaviour
{
	[SerializeField] private GameObject Batsmen;
	
	public void Menu ()
	{
		print ("done");

        SceneManager.LoadScene(0);
    }
    public void Animation6()
    {
        Batsmen.GetComponent<Animator> ().enabled = true;
        Invoke ("Animation6Off", 5.0f);
    }

    void Animation6Off ()
    {
        Batsmen.GetComponent<Animator> ().enabled = false;
    }
}
