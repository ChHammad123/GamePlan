 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    [Header("NEW \n")]

    public Button PlayButton;

    public int QuickMatchSceneIndex, TournamentSceneIndex, ChallengeSceneIndex, PracticeSceneIndex, NationalAnthemSceneIndex;

    public Toggle[] AllToggles;

    public GameObject[] OwnTeamFlags;

    public GameObject[] OpponentTeamFlags;

    // OLD

    [Header("OLD \n ")]

    public GameObject Shop;
    public GameObject Coin, Schedule, TeamSelection, CommingSoon;
    public GameObject MatchDisplay, SettingPanel;
	public GameObject PslSchedule, TeamSchedule;
	public GameObject MainPanel;
	public GameObject Loading;
    public GameObject ShopClose;

    public Sprite[] DiffiArray;
    int index = 0;
   
    public Image Difficulty;

    public Sprite[] OversArray;
   
    public Image Overs;
    public Image Pitch;
    public Sprite[] pitchArray;
    public Image PitchText;
    public Sprite[] pitchTextArray;

    // New Functions

    public void ChangeFlagOwn(int direction)
    {
        if (direction == 1)
        {
            for (int i = 0; i < OwnTeamFlags.Length; i++)
            {
                if(OwnTeamFlags[i].activeSelf)
                {
                    if (i + 1 >= OwnTeamFlags.Length)
                    {
                        OwnTeamFlags[i].SetActive(false);
                        OwnTeamFlags[0].SetActive(true);
                    }
                    else
                    {
                        OwnTeamFlags[i].SetActive(false);
                        OwnTeamFlags[i + 1].SetActive(true);
                    }
                    break;
                }
            }
        }

        else if (direction == -1)
        {
            for (int i = 0; i < OwnTeamFlags.Length; i++)
            {
                if (OwnTeamFlags[i].activeSelf)
                {
                    if (i - 1 < 0) 
                    {
                        OwnTeamFlags[i].SetActive(false);
                        OwnTeamFlags[(OwnTeamFlags.Length - 1)].SetActive(true);
                    }
                    else
                    {
                        OwnTeamFlags[i].SetActive(false);
                        OwnTeamFlags[i - 1].SetActive(true);
                    }
                    break;
                }
            }
        }

        // Play Button Check (Same Teams Can't Procede)

        bool flag = true;

        for (int i = 0; i < OwnTeamFlags.Length; i++)
        {
            if(OwnTeamFlags[i].activeSelf && OpponentTeamFlags[i].activeSelf)
            {
                PlayButton.interactable = false;
                PlayButton.image.color = Color.grey;
                flag = false;
                break;
            }
        }

        if(flag)
        {
            PlayButton.interactable = true;
            PlayButton.image.color = Color.white;
            flag = false;
        }
    }
    
    public void ChangeFlagOpponent(int direction)
    {
        if (direction == 1)
        {
            for (int i = 0; i < OpponentTeamFlags.Length; i++)
            {
                if(OpponentTeamFlags[i].activeSelf)
                {
                    if (i + 1 >= OpponentTeamFlags.Length)
                    {
                        OpponentTeamFlags[i].SetActive(false);
                        OpponentTeamFlags[0].SetActive(true);
                    }
                    else
                    {
                        OpponentTeamFlags[i].SetActive(false);
                        OpponentTeamFlags[i + 1].SetActive(true);
                    }
                    break;
                }
            }
        }

        else if (direction == -1)
        {
            for (int i = 0; i < OpponentTeamFlags.Length; i++)
            {
                if (OpponentTeamFlags[i].activeSelf)
                {
                    if (i - 1 < 0) 
                    {
                        OpponentTeamFlags[i].SetActive(false);
                        OpponentTeamFlags[(OpponentTeamFlags.Length - 1)].SetActive(true);
                    }
                    else
                    {
                        OpponentTeamFlags[i].SetActive(false);
                        OpponentTeamFlags[i - 1].SetActive(true);
                    }
                    break;
                }
            }
        }

        // Play Button Check (Same Teams Can't Procede)

        bool flag = true;

        for (int i = 0; i < OwnTeamFlags.Length; i++)
        {
            if(OwnTeamFlags[i].activeSelf && OpponentTeamFlags[i].activeSelf)
            {
                PlayButton.interactable = false;
                PlayButton.image.color = Color.grey;
                flag = false;
                break;
            }
        }

        if(flag)
        {
            PlayButton.interactable = true;
            PlayButton.image.color = Color.white;
            flag = false;
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;

        int battingTeam = 0;
        int FieldingTeamIndex = 1;

        for (int i = 0; i < OwnTeamFlags.Length; i++)
        {
            if (OwnTeamFlags[i].activeSelf)
            {
                battingTeam = i;
                break;
            }
        }

        for (int i = 0; i < OpponentTeamFlags.Length; i++)
        {
            if (OpponentTeamFlags[i].activeSelf)
            {
                FieldingTeamIndex = i;
                break;
            }
        }

        BattingTeam(battingTeam);
        
        FieldingTeam(FieldingTeamIndex);

        bool flag= true;

        for (int i = 0; i < AllToggles.Length; i++)
        {
            if (AllToggles[i].isOn)
            {
                if (i == 0)
                    PlayerPrefs.SetInt("TotalOvers", 5);
                else if (i == 1)
                    PlayerPrefs.SetInt("TotalOvers", 10);

                flag = false;

                break;
            }
        }

        if (!flag)
        {
            SceneManager.LoadScene(NationalAnthemSceneIndex);
        }
    }

	public void PlayQuick ()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("isTournament", 0);
        PlayerPrefs.Save();

        Debug.Log("Play Quick BTn (Time.timeScale = " + Time.timeScale + " )");

        TeamSelection.SetActive (true);
	}
    
    public void PlayTournament()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("isTournament", 1);
        PlayerPrefs.Save();

        Debug.Log("Play Tournament BTn (Time.timeScale = " + Time.timeScale + " )");

        SceneManager.LoadScene(TournamentSceneIndex);
    }

    public void PlayChallenge()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("isTournament", 2);
        PlayerPrefs.Save();

        Debug.Log("Play Challenge BTn (Time.timeScale = " + Time.timeScale + " )");

        SceneManager.LoadScene(ChallengeSceneIndex);
    }

    public void PlayPractice()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("isTournament", -1);
        PlayerPrefs.Save();

        Debug.Log("Play Practice BTn (Time.timeScale = " + Time.timeScale + " )");

        SceneManager.LoadScene(PracticeSceneIndex);
    }


    public void SetStartingFlags()
    {
        int battersIndex = PlayerPrefs.GetInt("BattingTeam", 0);
        int fieldersIndex = PlayerPrefs.GetInt("FieldingTeam", 1);
        
        for (int i = 0; i < OwnTeamFlags.Length; i++)
            OwnTeamFlags[i].SetActive(false);

        for (int i = 0; i < OpponentTeamFlags.Length; i++)
            OpponentTeamFlags[i].SetActive(false);

        OwnTeamFlags[battersIndex].SetActive(true);
        OpponentTeamFlags[fieldersIndex].SetActive(true);
    }

    // Unity Predefined Functions

    void Start ()
    {
        Time.timeScale = 1f;

        Pitch.sprite = pitchArray[index];
        PitchText.sprite = pitchTextArray[index];
        Difficulty.sprite = DiffiArray[index];
        Overs.sprite = OversArray[index];

        SetStartingFlags();

		Invoke ("LoadingFalse", 5.0f);

        Debug.Log("Main Menu Script (Time.timeScale = " +Time.timeScale +" )");
	}

    // Old Functions

    public void PitchNext ()
    {
        index++;
        if (index >= pitchArray.Length)
        {
            index = 0;
        }
        Pitch.sprite = pitchArray[index];
       
    }

    public void PitchPrevious ()
    {
        index--;
        if (index < 0)
        {
            index = pitchArray.Length - 1;
        }
        Pitch.sprite = pitchArray[index];
       
    }

    public void PitchTextNext ()
    {
        index++;
        if (index >= pitchTextArray.Length)
        {
            index = 0;
        }
        PitchText.sprite = pitchTextArray[index];

    }

    public void PitchtextPrevious ()
    {
        index--;
        if (index < 0)
        {
            index = pitchTextArray.Length - 1;
        }
        PitchText.sprite = pitchTextArray[index];
    }

    public void NextSelect ()
    {
        index++;
        if (index >= DiffiArray.Length)
        {
            index = 0;
        }
        Difficulty.sprite = DiffiArray[index];

    }

    public void PreviousSelect ()
    {

        index--;
        if (index < 0)
        {
            index = DiffiArray.Length - 1;
        }
        Difficulty.sprite = DiffiArray[index];
    }

    public void NextSelectOvers ()
    {
        index++;
        if (index >= OversArray.Length)
        {
            index = 0;
        }
        Overs.sprite = OversArray[index];

    }

    public void PreviousSelectOvers ()
    {

        index--;
        if (index < 0)
        {
            index = OversArray.Length - 1;
        }
        Overs.sprite = DiffiArray[index];
    }

	public void Play ()
	{
		MatchDisplay.SetActive (true);		
	}

	public void Setting ()
	{
		SettingPanel.SetActive (true);
	}

	public void CommingSoonPanel ()
	{
		CommingSoon.SetActive (true);
	}

	public void CommingSoonPanelClose ()
	{
		CommingSoon.SetActive (false);
	}

	public void ShopPanel ()
	{
		Shop.SetActive (true);
	}

	public void CoinPanel ()
	{
		Coin.SetActive (true);
	}

	public void SchedulePanel ()
	{
		Schedule.SetActive (true);
	}

	public void TeamSelectionPanel ()
	{
		TeamSelection.SetActive (true);
	}

	public void PslSchedulePanel ()
	{
		PslSchedule.SetActive (true);
	}

	public void TeamSchedulePanel ()
	{
		TeamSchedule.SetActive (true);
	}

	public void PslSchedulePanelClose ()
	{
		PslSchedule.SetActive (false);
	}

	public void TeamSchedulePanelClose ()
	{
		TeamSchedule.SetActive (false);
	}

	public void PSLMainSchedule ()
	{
		Schedule.SetActive (false);
	}

	public void SettingPanelOpen ()
	{
		SettingPanel.SetActive (true);
	}

	public void SettingPanelClose ()
	{
		SettingPanel.SetActive (false);
	}

	public void MainPanelShow ()
	{
		MainPanel.SetActive (false);
		SettingPanel.SetActive (false);
		Schedule.SetActive (false);
		TeamSchedule.SetActive (false);
		CommingSoon.SetActive (false);
		Coin.SetActive (false);
		Shop.SetActive (false);
		MatchDisplay.SetActive (true);
	}

    public void BattingTeam(int index)
    {
        PlayerPrefs.SetInt("BattingTeam", index);
        PlayerPrefs.Save();
    }    

    public void FieldingTeam(int index)
    {
        PlayerPrefs.SetInt("FieldingTeam", index);
        PlayerPrefs.Save();
    }    

	public void PSLSCHEDULEclossssss ()
	{
        Coin.SetActive(false);
		MatchDisplay.SetActive (true);
		TeamSelection.SetActive (false);
	}

	void LoadingFalse ()
	{
		Loading.SetActive (false);
	}

	public void MoreGames ()
	{
		Application.OpenURL ("https://play.google.com/store/apps/dev?id=9150013904104793373");
	}

    public void RateUs ()
    {
        Application.OpenURL ("https://play.google.com.gamebridgestudio.IPLclubmanager");
    }

    public void OnLineStreaming()
    {
        Application.OpenURL("https://www.google.com/search?biw=1920&bih=963&ei=C2DxXKumPNzlgweuyLcw&q=worldcup+live&oq=worldcup+live&gs_l=psy-ab.3..0i324i10l3j0i131i10j0i10l6.520.6958..7382...3.0..0.342.3846.2-11j3......0....1..gws-wiz.....4..0i71j35i39j0i131i67j0i131j0j0i67j0i10i3.zkjXLZ-eZ4A#sie=lg;/m/0cxvrj;5;/m/021vk;mt;fp;1;;");
    }

    public void ShopClosefun()
    {
        ShopClose.SetActive(false);
    }

    public void OnlineSchedule()
    {
        Application.OpenURL("https://timesofindia.indiatimes.com/sports/cricket/ipl/schedule");
    }

}
    