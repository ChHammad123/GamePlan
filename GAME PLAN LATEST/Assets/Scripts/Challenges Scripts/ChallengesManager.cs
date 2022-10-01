using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChallengesManager : MonoBehaviour
{
    [Header("Scenes Data \n")]
    public int ChallengeSceneIndex = 0;
    
    public int MainMenuSceneIndex = 0;

    [Header("Challenges Data \n")]
    public bool challengeOverride = false;
    
    public int UnlockLevelsCount = 0;

    public GameObject[] allTexts;

    [Header("Challenge Data Holder \n")]
    public ChallengeDataHolder[] AllChallenges;

    private void Start()
    {
        // Resetting Challenges

        for (int i = 0; i < AllChallenges.Length; i++) 
        {
            AllChallenges[i].Lock.gameObject.SetActive(true); 
            AllChallenges[i].ChallengeButton.gameObject.SetActive(false);
        }

        // Activating Challenges
        if (!challengeOverride)
        {
            int challengesCount = PlayerPrefs.GetInt("ChallengesUnlocked", 0);

            if (challengesCount >= AllChallenges.Length)
                challengesCount = AllChallenges.Length - 1;

            for (int i = 0; i <= challengesCount; i++)
            {
                AllChallenges[i].Lock.gameObject.SetActive(false);
                AllChallenges[i].ChallengeButton.gameObject.SetActive(true);
            }
        }

        else
        {
            for (int i = 0; i < UnlockLevelsCount; i++) 
            {
                AllChallenges[i].Lock.gameObject.SetActive(false);
                AllChallenges[i].ChallengeButton.gameObject.SetActive(true);
            }
        }

        AllChallenges[0].Lock.gameObject.SetActive(false);

        TextChanged(0);
    }

    public void StartChallenge()
    {
        int index = 0;

        for (int i = 0; i < allTexts.Length; i++)
        {
            if (allTexts[i].gameObject.activeSelf)
            {
                index = i;
                break;
            }
        }

        PlayerPrefs.SetInt("SelectedChallenge", index);
        PlayerPrefs.Save();

        SceneManager.LoadScene(ChallengeSceneIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneIndex);
    }

    public void TextChanged(int index)
    {
        for (int i = 0; i < allTexts.Length; i++)
            allTexts[i].SetActive(false);

        allTexts[index].SetActive(true);
    }
}
