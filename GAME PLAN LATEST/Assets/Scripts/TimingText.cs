using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimingText : MonoBehaviour
{
    public float delay = 2.5f;

    TextMeshProUGUI ThisText;

    void Start()
    {
        ThisText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        Invoke("DisableAfterDelay", delay);
    }

    void DisableAfterDelay()
    {
        this.gameObject.SetActive(false);
    }

}
