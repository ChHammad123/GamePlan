using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateHtText : MonoBehaviour
{
    public float delay;

    public Image image;

    public AnimateSparkles[] AllSparkles;


    private void OnEnable()
    {   
        StartCoroutine(AnimateBG());

        //for (int i = 0; i < AllSparkles.Length; i++)
        //    AllSparkles[i].StartCoroutine(AllSparkles[i].OK());
    }

    private void OnDisable()
    {
        //for (int i = 0; i < AllSparkles.Length; i++)
        //    AllSparkles[i].StopAllCoroutines();

        StopAllCoroutines();
    }

    IEnumerator AnimateBG()
    {
        while(true)
        {
            image.gameObject.SetActive(true);

            yield return new WaitForSeconds(delay);

            image.gameObject.SetActive(false);

            yield return new WaitForSeconds(delay);

        }
    }
}
