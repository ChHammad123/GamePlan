using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class stiumscript : MonoBehaviour
{
    public void LoadScene ()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(2);
    }
}
