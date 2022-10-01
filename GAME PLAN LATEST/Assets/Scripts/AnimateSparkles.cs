using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateSparkles : MonoBehaviour
{
    Vector3 minScale;
    public Vector3 maxScale;
    public bool repeatable;

    public FloatRange speedRange;
    public float duration = 5;

    float speed;

    private void Start()
    {
        StartCoroutine(OK());
    }

    public IEnumerator OK()
    {
        speed = Random.Range(speedRange.min, speedRange.max);

        minScale = transform.localScale;

        do
        {
            yield return changeScale(minScale, maxScale, duration);
            yield return changeScale(maxScale, minScale, duration);
        }
        while (repeatable);
    }
    
    IEnumerator changeScale(Vector3 a, Vector3 b,float time)
    {
        float i = 0;
        float rate = (1 / time) * speed;

        while(i < 1)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(a, b, i);
            yield return null;
        }
    }

}
