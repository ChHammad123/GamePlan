using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class anthem : MonoBehaviour
{
	public int QuickMatchIndex = 4;

	void Start ()
	{
		Time.timeScale = 1f;

		Invoke("CompleteAnthem", 69f);

	}
	
	public void Skip ()
	{
		SceneManager.LoadScene(QuickMatchIndex);
	}

	public void CompleteAnthem ()
	{
		SceneManager.LoadScene(QuickMatchIndex);
	}
}
