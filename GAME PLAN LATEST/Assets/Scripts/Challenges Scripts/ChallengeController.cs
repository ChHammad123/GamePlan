using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeController : MonoBehaviour
{
    public bool ChallengeOverride = false;

    public int PlayChallengIndex = 0;

    public ChallengeLevelDataHolder[] AllChallenges;

    private void Awake()
    {
        for (int i = 0; i < AllChallenges.Length; i++) 
            AllChallenges[i].Challenge.SetActive(false);
    }

    private void Start()
    {
        if (!ChallengeOverride)
        {
            int selectedChallenge = PlayerPrefs.GetInt("SelectedChallenge");

            for (int i = 0; i < AllChallenges.Length; i++)
            {
                if (i == selectedChallenge)
                {
                    AllChallenges[i].Challenge.SetActive(true);
                    break;
                }
            }
        }
        else
        {
            AllChallenges[PlayChallengIndex].Challenge.SetActive(true);
        }
    }
}
