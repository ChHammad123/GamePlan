using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

//using UnityEngine.Advertisements;
//using AppodealAds.Unity.Api;

public class GameManager : MonoBehaviour
{
	public static GameManager single;

	public bool WorldCupOverride = false;

	public WorldCupScheduleManager worldCupScheduleManager;

	[Header(" ==== Challenge Mode Data === \n ")]
	public bool ChallengeOverride = false;

	public int oversProvidedInMatch = 5;
	
	public int scoreToAchieve = 15;

	public int onGoingOver = 1;

	public int startRuns = 0;
	
	public int wicketsAllowedToFall = 10;

	public bool startingWicketsOverride = false;

	public int startingWicketNo = 1;

	public bool scoreHitsOverride = false;

	public int sixesToAchieve = 0, foursToAchieve = 0;

	public GameObject ChallengeFailPanel;
	
	public GameObject ChallengeSuccessPanel;


	[Header(" ==== Gameplay Data === \n ")]
	// New
	public GameObject TeamSelectionPanel;

	public GameObject MainMenuPanel;

	public GameObject[] UiToDecPractice;

	public int MainMenuIndex = 0;

	public GameObject GiantTextsPanel;

	// Old

	public Material batterMaterial, fielderMaterial;

	public PitchData pitchData;

	public HitData hitdata;

	public Ball ball;

	public AnimatedSprite batter, pitcher, fieldFarLeft, fieldLeft, fieldRight, fieldFarRight;

	public string swingLeftAnimation, swingRightAnimation;

	public int oversPerGame = 5;

	[Tooltip ("The time during the animation at which the bat hits the ball. If the bat 'hits' at one second into the animation, set this to 1.")]
	public float ballHitTime;

	Pitch currentPitch;
	Pitch.StrikeType currentStrike;

	Team playerTeam, fieldTeam;

	int playerTeamIndex;

	int currentBatterIndex = 0, otherBatterIndex = 1;
	int currentBowlerIndex = 0;
	int currentOver, _currentPitchInOver;

	int ballsThrown = 0;

	int currentPitchInOver { 
		get { return _currentPitchInOver; }
		set {
			_currentPitchInOver = value;
			if (uiInfo.overStatus != null)
			{
				uiInfo.overStatus.text = (currentOver - 1) + "." + (_currentPitchInOver);
			}
		}
	}

	int _currentRuns = 0;

	int currentRuns {
		get { return _currentRuns; }
		set {
			_currentRuns = value;
			uiInfo.currentRuns.text = _currentRuns.ToString ();
			if (ballsThrown == 0)
			{
				uiInfo.runRate.text = "0";
			} 
			else 
			{
				float rate = (float)_currentRuns / (float)(ballsThrown);
				rate *= 6;

				if(uiInfo.runRate != null)
					uiInfo.runRate.text = rate.ToString ("N1");
			}
		}
	}

	Player currentBatter { get { return playerTeam.players [currentBatterIndex]; } }

	Player otherBatter { get { return playerTeam.players [otherBatterIndex]; } }

	BatterPanel currentBatterInfo { get { return uiInfo.batterPanels [currentBatterIndex]; } }

	Player currentBowler { get { return fieldTeam.players [currentBowlerIndex]; } }

	bool gameIsOver = false;
	bool bowlIsOver = false;
	bool swung = true;

	int tempWicketFallsCount = 0;
	int sixesCount = 0;
	int foursCount = 0;

	public UIInfo uiInfo;

	public int battingTeamIndex = 0;
	public int fieldingTeamIndex = 1;

	// For WorldCup
	public int currentWorldCupMatchIndex = 0;

	[System.Serializable]
	public class UIInfo
	{
		public Text teamName, opposingTeamName;
		public Text currentBatter, otherBatter, bowler;

		public Text oversPerGameText;
		public Text overStatus;
		public Text currentRuns;

		public Text scoreThisHit;
		public TextMeshProUGUI leftScreenText;
		public TextMeshProUGUI RightScreenText;

		public Text teamShorthandName, opposingTeamShorthandName;
		public Text requiredRuns, numberOut;
		public Text runRate;
		public Text[] runsPerBowl;

		public Text numberGameWins, numberGameLosses;

		public ShadowTextMesh currentBatterScore, otherBatterScore;

		public int requiredBatterPanels;
		public RectTransform batterPanelContainer;
		public BatterPanel batterPanelTemplate;

		public GameObject batOrderDisplayControl;
		public GameObject gameplayDisplayControl;
		public GameObject betweenPoolMatchesEnable;
		public GameObject betweenFinalsMatchesEnable;
		public GameObject ingameScoreboardEnable;

		public GameObject enableToShowOut;
		public GameObject enableToShowWin;
		public GameObject enableToShowLoss;
		public GameObject enableToShowCompleteVictory;

		public Color outColor = Color.red.Desaturated (), inColor = Color.white;

		public Behaviour[] enableWhileMenuOpen;

		public SpriteRenderer wicketImage;
		public Sprite wicketNormal, wicketHit;

		[HideInInspector]
		public BatterPanel[] batterPanels;

		public void Init ()
		{
			float max = 1f;
			float interval = 1f / (float)(requiredBatterPanels);

			batterPanels = new BatterPanel[requiredBatterPanels];

			for (int i = 0; i < requiredBatterPanels; i++)
			
			{
				float min = max - interval;
				var panel = batterPanelTemplate.Instantiate ();
				panel.rt.SetParent (batterPanelContainer);
				panel.rt.localScale = Vector3.one;
				panel.rt.anchorMax = panel.rt.anchorMax.WithY (max);
				panel.rt.anchorMin = panel.rt.anchorMin.WithY (min);
				panel.rt.offsetMin = batterPanelTemplate.rt.offsetMin;
				panel.rt.offsetMax = batterPanelTemplate.rt.offsetMax;
				panel.enableToHighlight.SetActive (false);
				batterPanels [i] = panel;
				max = min;

			}

			GameObject.Destroy (batterPanelTemplate.gameObject);
		}
	}

	// Functions
	void Awake ()
	{
		single = this;
	}

	void Start ()
	{
		Time.timeScale = 1f;

		Debug.Log("GameManager (Time.timeScale = " + Time.timeScale + " )");

		tempWicketFallsCount = 0;

		uiInfo.Init ();
		uiInfo.batOrderDisplayControl.SetActive (false);
		uiInfo.gameplayDisplayControl.SetActive (false);
		uiInfo.ingameScoreboardEnable.SetActive (false);

		if (!WorldCupOverride)
		{
			if (PlayerPrefs.GetInt("isTournament") == 0)
			{
				TeamSelectionPanel.SetActive(false);
				MainMenuPanel.SetActive(false);

				SetBattingTeam(PlayerPrefs.GetInt("BattingTeam", 0));
				SetFieldingTeam(PlayerPrefs.GetInt("FieldingTeam", 1));

				StartGame();
			}

			else if (PlayerPrefs.GetInt("isTournament") == -1) // For Practice Mode
			{
				TeamSelectionPanel.SetActive(false);
				MainMenuPanel.SetActive(false);

				SetBattingTeam(Random.Range(0, 4));
				SetFieldingTeam(Random.Range(5, 9));
				StartGame();
			}

			else if (PlayerPrefs.GetInt("isTournament") == 2) // For Challenge Mode
			{
				TeamSelectionPanel.SetActive(false);
				MainMenuPanel.SetActive(false);

				SetBattingTeam(Random.Range(0, 4));
				SetFieldingTeam(Random.Range(5, 9));
				StartGame();
			}
		}

	}

	public void SetBattingTeam (int teamIndex)
	{
		battingTeamIndex = teamIndex;

		playerTeamIndex = teamIndex;

		var team = TeamData.single.teams [teamIndex];
		uiInfo.teamName.text = team.name;
		playerTeam = team;
		SetTeamMaterial (single.batterMaterial, team);

		for (int i = 0; i < team.players.Count; i++)
		{
			var batter = team.players [i];
			var ui = uiInfo.batterPanels [i];
			ui.Reset ();
			ui.tName.text = batter.name;
			batter.isOut = false;
		}
		uiInfo.teamShorthandName.text = team.shortName;
	}

	public void SetFieldingTeam (int teamIndex)
	{
		fieldingTeamIndex = teamIndex;

		//fieldTeamIndex = teamIndex;
		var team = TeamData.single.teams [teamIndex];
		fieldTeam = team;
		uiInfo.opposingTeamName.text = team.name;
		uiInfo.opposingTeamShorthandName.text = team.shortName;
		SetTeamMaterial (single.fielderMaterial, team);
	}

	void SetTeamMaterial (Material mat, Team team)
	{
		mat.SetColor ("_Primary", team.jersey);
		mat.SetColor ("_Secondary", team.sleeve);
		mat.SetColor ("_Skin", team.skin);
	}

	bool isTourney = false;

	bool won = false;

	public void SetTournamentMode (bool on)
	{
		isTourney = on;
	}

    public void StartGame()
    {
        gameIsOver = false;

		if (!WorldCupOverride)
		{

			if (PlayerPrefs.GetInt("isTournament") == 1)
			{
				SetTournamentMode(true);
			}

			else if(PlayerPrefs.GetInt("isTournament") != 2)
			{
				oversPerGame = PlayerPrefs.GetInt("TotalOvers");

				uiInfo.oversPerGameText.text = "(" + oversPerGame + ")";

				SetTournamentMode(false);
			}

			if (isTourney)
			{
				TournamentManager.single.Restart(playerTeamIndex);
				SetFieldingTeam(TournamentManager.single.FindFirstOpponent());
			}

			else if (fieldTeam == null)
			{
				// FIXME: this assumes there are 14 teams
				int[] acceptable = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }.Without(playerTeamIndex);
				SetFieldingTeam(acceptable.RandomElement());
			}

		}

		else if (PlayerPrefs.GetInt("isTournament") != 2)
		{
			oversPerGame = 5;

			uiInfo.oversPerGameText.text = "(" + oversPerGame + ")";
		}

		StartMatch();

		if (PlayerPrefs.GetInt("isTournament") == -1) // For Practice Mode
		{
			for (int i = 0; i < UiToDecPractice.Length; i++)
				UiToDecPractice[i].SetActive(false);
		}
	}

    void StartMatch ()
	{
		if (ChallengeOverride)
			StartCoroutine(RunGameForChallenge());

		else if(WorldCupOverride)
			StartCoroutine(RunGameForWorldCup());

		else
			StartCoroutine(RunGame());

	}

	IEnumerator WaitInput ()
	{
		yield return new WaitForSeconds (0.2f);

		while (!Input.anyKey)
			yield return new WaitForSeconds(0.1f);

	}

	int nonTourneyRR = 0;

	IEnumerator RunGame ()
	{
		if (isTourney)
		{
			uiInfo.requiredRuns.text = TournamentManager.RequiredRuns ().ToString ();
		}

		else if (PlayerPrefs.GetInt("isTournament") == -1)
        {
			nonTourneyRR = Random.Range(10000, 15000);
			oversPerGame = 100;
		}

		else if(PlayerPrefs.GetInt("isTournament") == 2)
		{
			nonTourneyRR = scoreToAchieve;
			oversPerGame = oversProvidedInMatch;
			uiInfo.requiredRuns.text = nonTourneyRR.ToString();
		}

		else 
		{
			nonTourneyRR = Random.Range(100, 150);
			uiInfo.requiredRuns.text = nonTourneyRR.ToString ();
		}

		uiInfo.numberOut.text = "0";

		if (ChallengeOverride)
		{
			currentRuns = startRuns;
			currentOver = onGoingOver;
			currentPitchInOver = 0;
			currentBatterIndex = -1;
			otherBatterIndex = -1;
			uiInfo.bowler.text = currentBowler.name;
			currentBowlerIndex = Random.Range(0, fieldTeam.players.Count);

			oversPerGame = oversProvidedInMatch;

			uiInfo.oversPerGameText.text = "(" + oversPerGame + ")";
			if(startingWicketsOverride)
            {
				currentBatterIndex = startingWicketNo;

				otherBatterIndex = startingWicketNo + 1;

				tempWicketFallsCount += startingWicketNo;

				int cnt = 0;

				foreach (var player in playerTeam.players)
				{
					if (cnt == startingWicketNo) 
						break;

					player.isOut = true;
					cnt++;
				}

				int battersLeft = playerTeam.players.Where(x => !x.isOut).Count();
				uiInfo.numberOut.text = (playerTeam.players.Count - battersLeft + startingWicketNo).ToString();

			}
		}

		else
		{
			currentRuns = 0;
			currentOver = 1;
			currentPitchInOver = 0;
			currentBatterIndex = -1;
			otherBatterIndex = -1;
			uiInfo.bowler.text = currentBowler.name;
			currentBowlerIndex = Random.Range(0, fieldTeam.players.Count);
		}

		foreach (var thing in uiInfo.runsPerBowl)
			thing.text = "-";

		foreach (var player in playerTeam.players)
			player.isOut = false;

		foreach (var panel in uiInfo.batterPanels)
		{
			panel.Reset ();
			panel.colorThisToShowOutStatus.color = uiInfo.inColor;
		}

		bool done = false;

		gameIsOver = false;

		won = false;

		if (isTourney) 
		{
			uiInfo.betweenPoolMatchesEnable.SetActive (false);
			uiInfo.betweenFinalsMatchesEnable.SetActive (false);
		}

		uiInfo.ingameScoreboardEnable.SetActive (true);

		while (!done && !gameIsOver) 
		{
			yield return StartCoroutine (NextBatter ());

			while (!currentBatter.isOut && !gameIsOver)
			{
				int battersLeft = playerTeam.players.Where (x => !x.isOut).Count ();

				if (PlayerPrefs.GetInt("isTournament") == 2)
					uiInfo.numberOut.text = (playerTeam.players.Count - battersLeft + startingWicketNo).ToString();
				else
					uiInfo.numberOut.text = (playerTeam.players.Count - battersLeft).ToString();


				if (battersLeft < 2 || currentOver > oversPerGame)
				{
					gameIsOver = true;

					break;
				}

				uiInfo.wicketImage.sprite = uiInfo.wicketNormal;

				yield return StartCoroutine (ShowScorePanel ());

				bowlIsOver = false;
				swung = false;
				uiInfo.gameplayDisplayControl.SetActive (true);

				yield return new WaitForSeconds (Random.Range (1f, 2f));

				RandomPitch ();

				while (!bowlIsOver)
					yield return null;

				uiInfo.gameplayDisplayControl.SetActive (false);
			}

		}

		uiInfo.ingameScoreboardEnable.SetActive (false);
		gameIsOver = true;

		if (isTourney)
		{
			if (currentRuns > TournamentManager.RequiredRuns())
			{
				won = true;

				TournamentManager.wins += 1;
			}
			else
			{
				won = false;

				TournamentManager.losses += 1;
			}
		}

		else if (PlayerPrefs.GetInt("isTournament") == 2)
        {
			if (currentRuns >= nonTourneyRR)
			{
				ChallengeSuccess();
			}
			else
            {
				ChallengeFail();
            }
		}

		else 
		{
			if (currentRuns >= nonTourneyRR )
			{
				won = true;
			}
		}

		if (!won)
		{
			uiInfo.enableToShowLoss.SetActive (true);
		}

		yield return StartCoroutine (WaitInput ());

		uiInfo.enableToShowWin.SetActive (false);
		uiInfo.enableToShowLoss.SetActive (false);

		if (!won) 
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		if (isTourney)
		{
			uiInfo.numberGameWins.text = TournamentManager.wins + "";
			uiInfo.numberGameLosses.text = TournamentManager.losses + "";

			uiInfo.betweenPoolMatchesEnable.SetActive(true);

			yield return WaitInput();

			int nextOpponent = TournamentManager.single.FindNextOpponent (won);

			if (nextOpponent >= 0) 
			{
				SetFieldingTeam (nextOpponent);
				StartMatch ();
				print ("nnnnnnnnn");
			}
			else if (TournamentManager.WonTourney) 
			{
				uiInfo.enableToShowCompleteVictory.SetActive (true);
				yield return StartCoroutine (WaitInput ());

				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
			else 
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
			}
		} 

		else
		{
			if (PlayerPrefs.GetInt("isTournament") == -1)
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

			else
				SceneManager.LoadScene(0);
		}
	}
	
	IEnumerator RunGameForChallenge ()
	{
		nonTourneyRR = scoreToAchieve;

		uiInfo.requiredRuns.text = nonTourneyRR + "";

		uiInfo.numberOut.text = "0";

		currentRuns = startRuns;
		currentOver = onGoingOver;
		currentPitchInOver = 0;
		currentBatterIndex = -1;
		otherBatterIndex = -1;
		uiInfo.bowler.text = currentBowler.name;
		currentBowlerIndex = Random.Range(0, fieldTeam.players.Count);

		oversPerGame = oversProvidedInMatch;

		uiInfo.oversPerGameText.text = "(" + oversPerGame + ")";

		if(startingWicketsOverride)
        {
			currentBatterIndex = startingWicketNo;

			otherBatterIndex = startingWicketNo + 1;

			tempWicketFallsCount += startingWicketNo;

			int cnt = 0;

			foreach (var player in playerTeam.players)
			{
				if (cnt == startingWicketNo) 
					break;

				player.isOut = true;
				cnt++;
			}

			int battersLeft = playerTeam.players.Where(x => !x.isOut).Count();
			uiInfo.numberOut.text = (playerTeam.players.Count - battersLeft + startingWicketNo).ToString();
		}

		foreach (var thing in uiInfo.runsPerBowl)
			thing.text = "-";

		foreach (var player in playerTeam.players)
			player.isOut = false;

		foreach (var panel in uiInfo.batterPanels)
		{
			panel.Reset ();
			panel.colorThisToShowOutStatus.color = uiInfo.inColor;
		}

		bool done = false;

		gameIsOver = false;

		won = false;

		uiInfo.ingameScoreboardEnable.SetActive (true);

		while (!done && !gameIsOver) 
		{
			yield return StartCoroutine (NextBatter ());

			while (!currentBatter.isOut && !gameIsOver)
			{
				int battersLeft = playerTeam.players.Where (x => !x.isOut).Count ();

				uiInfo.numberOut.text = (playerTeam.players.Count - battersLeft + startingWicketNo).ToString();

				if (battersLeft < 2 || currentOver > oversPerGame)
				{
					gameIsOver = true;

					break;
				}

				else if (!scoreHitsOverride) 
				{
					if (currentRuns > scoreToAchieve)
					{
						done = true;

						break;
					}
				}

                else
                {
					if((sixesCount >= sixesToAchieve) && (foursCount >= foursToAchieve))
					{
						done = true;

						break;
					}
                }

				uiInfo.wicketImage.sprite = uiInfo.wicketNormal;

				yield return StartCoroutine (ShowScorePanel ());

				bowlIsOver = false;

				swung = false;

				uiInfo.gameplayDisplayControl.SetActive (true);

				yield return new WaitForSeconds (Random.Range (1f, 2f));

				RandomPitch ();

				while (!bowlIsOver)
					yield return null;

				uiInfo.gameplayDisplayControl.SetActive (false);
				
				if (battersLeft < 2 || currentOver > oversPerGame)
				{
					gameIsOver = true;

					break;
				}

				else if (!scoreHitsOverride)
				{
					if (currentRuns > scoreToAchieve)
					{
						done = true;

						break;
					}
				}

				else
				{
					if ((sixesCount >= sixesToAchieve) && (foursCount >= foursToAchieve))
					{
						done = true;

						break;
					}
				}
			}

		}

		uiInfo.ingameScoreboardEnable.SetActive (false);

		gameIsOver = true;

		if (!scoreHitsOverride)
		{
			if (currentRuns >= nonTourneyRR)
				ChallengeSuccess();

			else
				ChallengeFail();
		}

		else
		{
			if ((sixesCount >= sixesToAchieve) && (foursCount >= foursToAchieve))
				ChallengeSuccess();

			else
				ChallengeFail();
		}
	}
	
	IEnumerator RunGameForWorldCup ()
	{
		ballsThrown = 0;

		oversPerGame = 5;
		nonTourneyRR = Random.Range(1, 5);
		uiInfo.requiredRuns.text = nonTourneyRR.ToString ();
		uiInfo.numberOut.text = "0";

		currentRuns = 0;
		currentOver = 1;
		currentPitchInOver = 0;
		currentBatterIndex = -1;
		otherBatterIndex = -1;
		uiInfo.bowler.text = currentBowler.name;
		currentBowlerIndex = Random.Range(0, fieldTeam.players.Count);


		foreach (var thing in uiInfo.runsPerBowl)
			thing.text = "-";

		foreach (var player in playerTeam.players)
			player.isOut = false;

		foreach (var panel in uiInfo.batterPanels)
		{
			panel.Reset ();
			panel.colorThisToShowOutStatus.color = uiInfo.inColor;
		}

		bool done = false;

		gameIsOver = false;

		won = false;

		uiInfo.ingameScoreboardEnable.SetActive (true);

		while (!done && !gameIsOver) 
		{
			yield return StartCoroutine (NextBatter ());

			while (!currentBatter.isOut && !gameIsOver)
			{
				int battersLeft = playerTeam.players.Where (x => !x.isOut).Count ();

				uiInfo.numberOut.text = (playerTeam.players.Count - battersLeft).ToString();

				if (battersLeft < 2)
				{
					gameIsOver = true;

					break;
				}

				if(currentOver > oversPerGame)
				{
					gameIsOver = true;

					break;
				}

				uiInfo.wicketImage.sprite = uiInfo.wicketNormal;

				yield return StartCoroutine (ShowScorePanel ());

				bowlIsOver = false;
				swung = false;
				uiInfo.gameplayDisplayControl.SetActive (true);

				yield return new WaitForSeconds (Random.Range (1f, 2f));

				RandomPitch ();

				while (!bowlIsOver)
					yield return null;

				uiInfo.gameplayDisplayControl.SetActive (false);
			}

		}

		uiInfo.ingameScoreboardEnable.SetActive (false);
		gameIsOver = true;

		if (currentRuns >= nonTourneyRR )
		{
			won = true;

			WorldCupMatchWon();
		}

		else
        {
			WorldCupMatchLost();
		}
	}

	void WorldCupMatchWon()
	{
		worldCupScheduleManager.WorldCupMatchCompleted(currentWorldCupMatchIndex, battingTeamIndex);

		StopAllCoroutines();

	}
	
	void WorldCupMatchLost()
	{
		worldCupScheduleManager.WorldCupMatchCompleted(currentWorldCupMatchIndex, fieldingTeamIndex);

		StopAllCoroutines();
	}

	void Restart()
    {
		StopCoroutine(RunGame());
		StartCoroutine(RunGame());
    }

	void RandomPitch ()
	{
		int pitchNumber = Random.Range (0, pitchData.pitches.Length);

		currentBatterInfo.Balls++;

		ballsThrown++;

		currentPitch = pitchData.pitches [pitchNumber];

		pitcher.PlayAnimation ("Pitch");
	}

	IEnumerator Out ()
	{
		yield return new WaitForSeconds (1f);

		uiInfo.enableToShowOut.SetActive (true);

		yield return new WaitForSeconds (0.7f);

		uiInfo.enableToShowOut.SetActive (false);
	}

	IEnumerator NextBatter ()
	{
		if (otherBatterIndex.IsValidIndexFor (playerTeam.players))
		{
			if (currentBatter.isOut)
			{
				var batpan = currentBatterInfo;
				batpan.colorThisToShowOutStatus.color = uiInfo.outColor;
			}

			var unusableIndex = currentBatterIndex;
			currentBatterIndex = otherBatterIndex;

			int startIndex = otherBatterIndex;
			otherBatterIndex = (otherBatterIndex + 1) % playerTeam.players.Count;

			while (otherBatter.isOut)
			{ 
				otherBatterIndex = (otherBatterIndex + 1) % playerTeam.players.Count;

				if (otherBatterIndex == unusableIndex)
					continue;

				if (otherBatterIndex == startIndex)
				{
					yield break;
				}
			}
		} 
		else 
		{ 
			// first time through

			currentBatterIndex = 0;
			otherBatterIndex = 1;
		}

		RefreshBatterGraphics ();
	}

	void RefreshBatterGraphics ()
	{
		uiInfo.currentBatter.text = currentBatter.name;
		uiInfo.otherBatter.text = otherBatter.name;
		uiInfo.currentBatterScore.shadowTarget = uiInfo.batterPanels [currentBatterIndex].tRuns;
		uiInfo.otherBatterScore.shadowTarget = uiInfo.batterPanels [otherBatterIndex].tRuns;
	}

	IEnumerator ShowScorePanel ()
	{
		var batterPanel = uiInfo.batterPanels [currentBatterIndex];
		BatterPanel otherPanel = null;
		if (otherBatterIndex.IsValidIndexFor (uiInfo.batterPanels)) {
			otherPanel = uiInfo.batterPanels [otherBatterIndex];
		}
		if (otherPanel)
			otherPanel.enableToHighlight.SetActive (true);
		batterPanel.enableToHighlight.SetActive (true);
		//uiInfo.batOrderDisplayControl.SetActive (true);
		//yield return StartCoroutine (WaitInput ());
		//uiInfo.batOrderDisplayControl.SetActive (false);
		batterPanel.enableToHighlight.SetActive (false);
		if (otherPanel)
			otherPanel.enableToHighlight.SetActive (false);

		yield return null;
	}

	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			SwingRight ();
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			SwingLeft ();
		}
	}

	public void SwingLeft ()
	{
		if (!swung)
		{
			uiInfo.gameplayDisplayControl.SetActive (false);

			currentStrike = Pitch.StrikeType.Left;
			batter.PlayAnimation (swingLeftAnimation);
			swung = true;
		}
	}

	public void SwingRight ()
	{
		if (!swung) 
		{
			uiInfo.gameplayDisplayControl.SetActive (false);

			currentStrike = Pitch.StrikeType.Right;
			batter.PlayAnimation (swingRightAnimation);
			swung = true;
		}
	}

	public static void EndBowl ()
	{
		single.currentPitchInOver++;
		single.currentRuns = single.currentRuns;

		if (single.currentPitchInOver >= 6) 
		{
			single.currentOver++;

			if (single.currentOver > single.oversPerGame) 
			{
				single.EndGame ();
			}
			single.currentPitchInOver = 0;
			single.currentBowlerIndex = (single.currentBowlerIndex + 1) % single.fieldTeam.players.Count;
			single.uiInfo.bowler.text = single.currentBowler.name;

			foreach (var thing in single.uiInfo.runsPerBowl)
				thing.text = "-";
		}

		single.bowlIsOver = true;
	}

	void EndGame ()
	{
		gameIsOver = true;
	}

	public static void PitchNow ()
	{
		single.pitchWasHit = false;
		single.ball.gameObject.SetActive (true);
		single.currentPitch.Execute (single.ball.gameObject);
		if (single.currentPitch.hitsWicket) {
			single.Invoke ("HitWicket", single.currentPitch.hitsWicketAtTime);
		}
	}

	void HitWicket ()
	{
		if (pitchWasHit)
			return;

		if (BallForce.instance.direction.Equals("leg") || BallForce.instance.direction.Equals("off"))
			return;

		if (!WorldCupOverride)
		{
			if (tempWicketFallsCount >= wicketsAllowedToFall)
			{
				ChallengeFail();
			}
		}
		 
		tempWicketFallsCount++;

		uiInfo.wicketImage.sprite = uiInfo.wicketHit;

		StartCoroutine (PutCurrentBatterOut ("Wicket hit"));
	}

	IEnumerator PutCurrentBatterOut (string reason)
	{
		yield return StartCoroutine (Out ());

		currentBatterInfo.tOutReason.text = reason + " by " + currentBowler.name;
		currentBatterInfo.FallOfWicket = currentRuns;
		currentBatter.isOut = true;
	}

	public static void HitNow ()
	{
		single.StartCoroutine (single.AttemptHit ());
	}

	bool pitchWasHit = false;

	IEnumerator AttemptHit ()
	{
		if (currentPitch == null)
			yield break;

		var strike = currentPitch.AttemptHit (currentStrike);

		if (strike != HitData.HitType.NONE) 
		{
			var hit = hitdata.hits [(int)strike];
			hit.ExecuteForceOnly (ball.gameObject);
			pitchWasHit = true;
		}

		var strikeText = strike.ToString ();

		if (strikeText.Contains ("six")) 
		{
			currentBatterInfo.Sixes++;
			sixesCount++;
			yield return StartCoroutine (AddRuns (6));
		}
		else if (strikeText.Contains ("four"))
		{
			currentBatterInfo.Fours++;
			foursCount++;
			yield return StartCoroutine (AddRuns (4));
		} 
		else if (strikeText.Contains ("three")) 
		{
			SwapBatters ();
			yield return StartCoroutine (AddRuns (3));
		} 
		else if (strikeText.Contains ("two")) 
		{
			yield return StartCoroutine (AddRuns (2));
		} 
		else if (strikeText.Contains ("one"))
		{
			SwapBatters ();
			yield return StartCoroutine (AddRuns (1));
		} 
		else if (strikeText.Contains ("out"))
		{
			if (ChallengeOverride)
			{
				if (tempWicketFallsCount >= wicketsAllowedToFall)
				{
					ChallengeFail();
				}
			}

			yield return StartCoroutine (PutCurrentBatterOut ("Bowled"));
		}

		currentRuns = currentRuns;
	}
                                         
	void SwapBatters ()
	{
		var temp = otherBatterIndex;
		otherBatterIndex = currentBatterIndex;
		currentBatterIndex = temp;
	}

	IEnumerator AddRuns (int runs)
	{
		if (uiInfo.leftScreenText == null || uiInfo.RightScreenText == null || runs <= 3)
		{

			yield return new WaitForSeconds(1f);

			uiInfo.scoreThisHit.text = runs.ToString();
			uiInfo.scoreThisHit.gameObject.SetActive(true);

			yield return new WaitForSeconds(3f);

			uiInfo.scoreThisHit.gameObject.SetActive(false);
		}

		else if (runs >= 4) 
        {
			yield return new WaitForSeconds(1f);

			uiInfo.leftScreenText.text = runs.ToString();
			uiInfo.RightScreenText.text = runs.ToString();

			uiInfo.leftScreenText.gameObject.SetActive(true);
			uiInfo.RightScreenText.gameObject.SetActive(true);

			yield return new WaitForSeconds(3f);

			uiInfo.leftScreenText.gameObject.SetActive(false);
			uiInfo.RightScreenText.gameObject.SetActive(false);
		}

		uiInfo.runsPerBowl[currentPitchInOver].text = runs.ToString();

		if (runs % 2 == 1) 
		{
			var x = currentBatterIndex;
			currentBatterIndex = otherBatterIndex;
			otherBatterIndex = x;
			RefreshBatterGraphics ();
		}
		currentRuns += runs;
		currentBatterInfo.Runs += runs;

		if (isTourney)
		{
			if (currentRuns > TournamentManager.RequiredRuns ()) 
			{
				won = true;
				gameIsOver = true;
			}
		} 

		else 
		{
			if (currentRuns > nonTourneyRR) 
			{
				gameIsOver = true;
				won = true;
			}
		}

	}

	void OnGUI ()
	{
		/*
		GUILayout.Label ("Press a number key to throw a pitch.");
		GUILayout.Label ("Press -> or <- to bat.");
		GUILayout.Label ("Press ^ and v to change teams of batter.");

		GUILayout.Label ("===== PITCHES =====");
		for(int i=0; i < pitchData.pitches.Length; i++ ) {
			GUILayout.Label ("- " + i + " : " + pitchData.pitches[i].name);
		}*/
	}

	public void GoToMainMenu()
    {
		Time.timeScale = 1f;

		SceneManager.LoadScene(MainMenuIndex);
    }
	
	public void GoToCustomScene(int index)
    {
		Time.timeScale = 1f;

		SceneManager.LoadScene(index);
    }

    public void ChallengeFail()
    {
		GiantTextsPanel.SetActive(false);

		ChallengeFailPanel.SetActive(true);

		StopAllCoroutines();
    }

    public void ChallengeSuccess()
    {
		GiantTextsPanel.SetActive(false);

		ChallengeSuccessPanel.SetActive(true);

		int selectedChallenge = PlayerPrefs.GetInt("SelectedChallenge");

		selectedChallenge++;

		int challengProgressed = PlayerPrefs.GetInt("ChallengesUnlocked");

		if (selectedChallenge > challengProgressed)
		{
			PlayerPrefs.SetInt("ChallengesUnlocked", selectedChallenge);
		}

		PlayerPrefs.Save();

		StopAllCoroutines();
	}		
}