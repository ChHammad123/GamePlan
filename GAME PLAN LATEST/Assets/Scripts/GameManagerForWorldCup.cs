using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManagerForWorldCup : MonoBehaviour
{
	public static GameManagerForWorldCup single;

	public int MainMenuIndex = 0;

	public Material batterMaterial, fielderMaterial;

	public PitchData pitchData;

	public HitData hitdata;

	public Ball ball;

	public AnimatedSprite batter, pitcher, fieldFarLeft, fieldLeft, fieldRight, fieldFarRight;

	public string swingLeftAnimation, swingRightAnimation;

	public int oversPerGame = 5;

	[Tooltip("The time during the animation at which the bat hits the ball. If the bat 'hits' at one second into the animation, set this to 1.")]
	public float ballHitTime;

	Pitch currentPitch;
	Pitch.StrikeType currentStrike;

	Team playerTeam, fieldTeam;

	int playerTeamIndex;

	int currentBatterIndex = 0, otherBatterIndex = 1;
	int currentBowlerIndex = 0;
	int currentOver, _currentPitchInOver;

	int ballsThrown = 0;

	int currentPitchInOver
	{
		get { return _currentPitchInOver; }
		set
		{
			_currentPitchInOver = value;
			if (uiInfo.overStatus != null)
				uiInfo.overStatus.text = (currentOver - 1) + "." + (_currentPitchInOver);
		}
	}

	int _currentRuns = 0;

	int currentRuns
	{
		get { return _currentRuns; }
		set
		{
			_currentRuns = value;
			uiInfo.currentRuns.text = _currentRuns.ToString();
			if (ballsThrown == 0)
			{
				uiInfo.runRate.text = "0";
			}
			else
			{
				float rate = (float)_currentRuns / (float)(ballsThrown);
				rate *= 6;

				if (uiInfo.runRate != null)
					uiInfo.runRate.text = rate.ToString("N1");
			}
		}
	}

	Player currentBatter { get { return playerTeam.players[currentBatterIndex]; } }

	Player otherBatter { get { return playerTeam.players[otherBatterIndex]; } }

	BatterPanel currentBatterInfo { get { return uiInfo.batterPanels[currentBatterIndex]; } }

	Player currentBowler { get { return fieldTeam.players[currentBowlerIndex]; } }

	bool gameIsOver = false;
	bool bowlIsOver = false;
	bool swung = true;

	int tempWicketFallsCount = 0;

	public UIInfo uiInfo;

	[System.Serializable]
	public class UIInfo
	{
		public Text teamName, opposingTeamName;
		public Text currentBatter, otherBatter, bowler;

		public Text oversPerGameText;
		public Text overStatus;
		public Text currentRuns;

		public Text scoreThisHit;

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

		public Color outColor = Color.red.Desaturated(), inColor = Color.white;

		public Behaviour[] enableWhileMenuOpen;

		public SpriteRenderer wicketImage;
		public Sprite wicketNormal, wicketHit;

		[HideInInspector]
		public BatterPanel[] batterPanels;

		public void Init()
		{
			float max = 1f;
			float interval = 1f / (float)(requiredBatterPanels);

			batterPanels = new BatterPanel[requiredBatterPanels];

			for (int i = 0; i < requiredBatterPanels; i++)

			{
				float min = max - interval;
				var panel = batterPanelTemplate.Instantiate();
				panel.rt.SetParent(batterPanelContainer);
				panel.rt.localScale = Vector3.one;
				panel.rt.anchorMax = panel.rt.anchorMax.WithY(max);
				panel.rt.anchorMin = panel.rt.anchorMin.WithY(min);
				panel.rt.offsetMin = batterPanelTemplate.rt.offsetMin;
				panel.rt.offsetMax = batterPanelTemplate.rt.offsetMax;
				panel.enableToHighlight.SetActive(false);
				batterPanels[i] = panel;
				max = min;

			}

			GameObject.Destroy(batterPanelTemplate.gameObject);
		}
	}

	// Functions
	void Awake()
	{
		single = this;
	}

	void Start()
	{
		tempWicketFallsCount = 0;

		uiInfo.Init();
		uiInfo.batOrderDisplayControl.SetActive(false);
		uiInfo.gameplayDisplayControl.SetActive(false);
		uiInfo.ingameScoreboardEnable.SetActive(false);

	}

	public void SetBattingTeam(int teamIndex)
	{
		playerTeamIndex = teamIndex;

		var team = TeamData.single.teams[teamIndex];
		uiInfo.teamName.text = team.name;
		playerTeam = team;
		SetTeamMaterial(single.batterMaterial, team);

		for (int i = 0; i < team.players.Count; i++)
		{
			var batter = team.players[i];
			var ui = uiInfo.batterPanels[i];
			ui.Reset();
			ui.tName.text = batter.name;
			batter.isOut = false;
		}
		uiInfo.teamShorthandName.text = team.shortName;
	}

	public void SetFieldingTeam(int teamIndex)
	{
		//fieldTeamIndex = teamIndex;
		var team = TeamData.single.teams[teamIndex];
		fieldTeam = team;
		uiInfo.opposingTeamName.text = team.name;
		uiInfo.opposingTeamShorthandName.text = team.shortName;
		SetTeamMaterial(single.fielderMaterial, team);
	}

	void SetTeamMaterial(Material mat, Team team)
	{
		mat.SetColor("_Primary", team.jersey);
		mat.SetColor("_Secondary", team.sleeve);
		mat.SetColor("_Skin", team.skin);
	}

	bool isTourney = false;

	bool won = false;

	public void SetTournamentMode(bool on)
	{
		isTourney = on;
	}

	public void StartGame()
	{
		gameIsOver = false;

		if (PlayerPrefs.GetInt("isTournament") == 1)
		{
			SetTournamentMode(true);
		}

		else
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

		StartMatch();

	}

	void StartMatch()
	{
		StartCoroutine(RunGame());

		StartCoroutine(RunGame());
	}

	IEnumerator WaitInput()
	{
		yield return new WaitForSeconds(0.2f);

		while (!Input.anyKeyDown)// && Input.touchCount < 1) 
			yield return new WaitForSeconds(0.1f);

	}

	int nonTourneyRR = 0;

	IEnumerator RunGame()
	{
		ballsThrown = 0;

		nonTourneyRR = Random.Range(100, 150);
		uiInfo.requiredRuns.text = nonTourneyRR.ToString();

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
			panel.Reset();
			panel.colorThisToShowOutStatus.color = uiInfo.inColor;
		}

		bool done = false;

		gameIsOver = false;

		won = false;

		if (isTourney)
		{
			uiInfo.betweenPoolMatchesEnable.SetActive(false);
			uiInfo.betweenFinalsMatchesEnable.SetActive(false);
		}

		uiInfo.ingameScoreboardEnable.SetActive(true);

		while (!done && !gameIsOver)
		{
			yield return StartCoroutine(NextBatter());

			while (!currentBatter.isOut && !gameIsOver)
			{
				int battersLeft = playerTeam.players.Where(x => !x.isOut).Count();


				uiInfo.numberOut.text = (playerTeam.players.Count - battersLeft).ToString();


				if (battersLeft < 2 || currentOver > oversPerGame)
				{
					gameIsOver = true;

					break;
				}

				uiInfo.wicketImage.sprite = uiInfo.wicketNormal;

				yield return StartCoroutine(ShowScorePanel());

				bowlIsOver = false;
				swung = false;
				uiInfo.gameplayDisplayControl.SetActive(true);

				yield return new WaitForSeconds(Random.Range(1f, 2f));

				RandomPitch();

				while (!bowlIsOver)
					yield return null;

				uiInfo.gameplayDisplayControl.SetActive(false);
			}

		}

		uiInfo.ingameScoreboardEnable.SetActive(false);
		gameIsOver = true;


		if (currentRuns >= nonTourneyRR)
		{
			won = true;
		}
		

		if (!won)
		{
			uiInfo.enableToShowLoss.SetActive(true);
		}

		yield return StartCoroutine(WaitInput());

		uiInfo.enableToShowWin.SetActive(false);
		uiInfo.enableToShowLoss.SetActive(false);

		if (!won)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

	}
	
	IEnumerator RunGameWorldCup()
	{
		ballsThrown = 0;

		nonTourneyRR = Random.Range(100, 150);
		uiInfo.requiredRuns.text = nonTourneyRR.ToString();

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
			panel.Reset();
			panel.colorThisToShowOutStatus.color = uiInfo.inColor;
		}

		bool done = false;

		gameIsOver = false;

		won = false;

		if (isTourney)
		{
			uiInfo.betweenPoolMatchesEnable.SetActive(false);
			uiInfo.betweenFinalsMatchesEnable.SetActive(false);
		}

		uiInfo.ingameScoreboardEnable.SetActive(true);

		while (!done && !gameIsOver)
		{
			yield return StartCoroutine(NextBatter());

			while (!currentBatter.isOut && !gameIsOver)
			{
				int battersLeft = playerTeam.players.Where(x => !x.isOut).Count();


				uiInfo.numberOut.text = (playerTeam.players.Count - battersLeft).ToString();


				if (battersLeft < 2 || currentOver > oversPerGame)
				{
					gameIsOver = true;

					break;
				}

				uiInfo.wicketImage.sprite = uiInfo.wicketNormal;

				yield return StartCoroutine(ShowScorePanel());

				bowlIsOver = false;
				swung = false;
				uiInfo.gameplayDisplayControl.SetActive(true);

				yield return new WaitForSeconds(Random.Range(1f, 2f));

				RandomPitch();

				while (!bowlIsOver)
					yield return null;

				uiInfo.gameplayDisplayControl.SetActive(false);
			}

		}

		uiInfo.ingameScoreboardEnable.SetActive(false);
		gameIsOver = true;


		if (currentRuns >= nonTourneyRR)
		{
			won = true;
		}
		

		if (!won)
		{
			uiInfo.enableToShowLoss.SetActive(true);
		}

		yield return StartCoroutine(WaitInput());

		uiInfo.enableToShowWin.SetActive(false);
		uiInfo.enableToShowLoss.SetActive(false);

		if (!won)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

	}

	void Restart()
	{
		StopCoroutine(RunGame());
		StartCoroutine(RunGame());
	}

	void RandomPitch()
	{
		int pitchNumber = Random.Range(0, pitchData.pitches.Length);
		currentBatterInfo.Balls++;
		ballsThrown++;
		currentPitch = pitchData.pitches[pitchNumber];
		pitcher.PlayAnimation("Pitch");
	}

	IEnumerator Out()
	{
		yield return new WaitForSeconds(1f);

		uiInfo.enableToShowOut.SetActive(true);

		yield return new WaitForSeconds(0.7f);

		uiInfo.enableToShowOut.SetActive(false);
	}

	IEnumerator NextBatter()
	{
		if (otherBatterIndex.IsValidIndexFor(playerTeam.players))
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
			{ //|| otherBatterIndex==unusableIndex) {
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
		{ // first time through
			currentBatterIndex = 0;
			otherBatterIndex = 1;
		}
		RefreshBatterGraphics();
	}

	void RefreshBatterGraphics()
	{
		uiInfo.currentBatter.text = currentBatter.name;
		uiInfo.otherBatter.text = otherBatter.name;
		uiInfo.currentBatterScore.shadowTarget = uiInfo.batterPanels[currentBatterIndex].tRuns;
		uiInfo.otherBatterScore.shadowTarget = uiInfo.batterPanels[otherBatterIndex].tRuns;
	}

	IEnumerator ShowScorePanel()
	{
		var batterPanel = uiInfo.batterPanels[currentBatterIndex];
		BatterPanel otherPanel = null;
		if (otherBatterIndex.IsValidIndexFor(uiInfo.batterPanels))
		{
			otherPanel = uiInfo.batterPanels[otherBatterIndex];
		}
		if (otherPanel)
			otherPanel.enableToHighlight.SetActive(true);
		batterPanel.enableToHighlight.SetActive(true);
		//uiInfo.batOrderDisplayControl.SetActive (true);
		//yield return StartCoroutine (WaitInput ());
		//uiInfo.batOrderDisplayControl.SetActive (false);
		batterPanel.enableToHighlight.SetActive(false);
		if (otherPanel)
			otherPanel.enableToHighlight.SetActive(false);

		yield return null;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			SwingRight();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			SwingLeft();
		}
	}

	public void SwingLeft()
	{
		if (!swung)
		{
			uiInfo.gameplayDisplayControl.SetActive(false);
			
			batter.PlayAnimation(swingLeftAnimation);

			swung = true;
		}
	}

	public void SwingRight()
	{
		if (!swung)
		{
			uiInfo.gameplayDisplayControl.SetActive(false);

			currentStrike = Pitch.StrikeType.Right;

			batter.PlayAnimation(swingRightAnimation);

			swung = true;
		}
	}

	public static void EndBowl()
	{
		single.currentPitchInOver++;
		single.currentRuns = single.currentRuns;

		if (single.currentPitchInOver >= 6)
		{
			single.currentOver++;

			if (single.currentOver > single.oversPerGame)
			{
				single.EndGame();
			}

			single.currentPitchInOver = 0;
			single.currentBowlerIndex = (single.currentBowlerIndex + 1) % single.fieldTeam.players.Count;
			single.uiInfo.bowler.text = single.currentBowler.name;

			foreach (var thing in single.uiInfo.runsPerBowl)
				thing.text = "-";
		}

		single.bowlIsOver = true;
	}

	void EndGame()
	{
		gameIsOver = true;
	}

	public static void PitchNow()
	{
		single.pitchWasHit = false;
		single.ball.gameObject.SetActive(true);
		single.currentPitch.Execute(single.ball.gameObject);
		if (single.currentPitch.hitsWicket)
		{
			single.Invoke("HitWicket", single.currentPitch.hitsWicketAtTime);
		}
	}

	void HitWicket()
	{
		if (pitchWasHit)
			return;

		tempWicketFallsCount++;

		uiInfo.wicketImage.sprite = uiInfo.wicketHit;

		StartCoroutine(PutCurrentBatterOut("Wicket hit"));
	}

	IEnumerator PutCurrentBatterOut(string reason)
	{
		yield return StartCoroutine(Out());

		currentBatterInfo.tOutReason.text = reason + " by " + currentBowler.name;
		currentBatterInfo.FallOfWicket = currentRuns;
		currentBatter.isOut = true;
	}

	public static void HitNow()
	{
		single.StartCoroutine(single.AttemptHit());
	}

	bool pitchWasHit = false;

	IEnumerator AttemptHit()
	{
		if (currentPitch == null)
			yield break;


		var strike = currentPitch.AttemptHit(currentStrike);


		if (strike != HitData.HitType.NONE)
		{
			var hit = hitdata.hits[(int)strike];
			hit.ExecuteForceOnly(ball.gameObject);
			pitchWasHit = true;
		}
		var strikeText = strike.ToString();
		if (strikeText.Contains("six"))
		{
			currentBatterInfo.Sixes++;
			yield return StartCoroutine(AddRuns(6));
		}
		else if (strikeText.Contains("four"))
		{
			currentBatterInfo.Fours++;
			yield return StartCoroutine(AddRuns(4));
		}
		else if (strikeText.Contains("three"))
		{
			SwapBatters();
			yield return StartCoroutine(AddRuns(3));
		}
		else if (strikeText.Contains("two"))
		{
			yield return StartCoroutine(AddRuns(2));
		}
		else if (strikeText.Contains("one"))
		{
			SwapBatters();
			yield return StartCoroutine(AddRuns(1));
		}
		else if (strikeText.Contains("out"))
		{
			yield return StartCoroutine(PutCurrentBatterOut("Bowled"));
		}
		currentRuns = currentRuns;
	}

	void SwapBatters()
	{
		var temp = otherBatterIndex;
		otherBatterIndex = currentBatterIndex;
		currentBatterIndex = temp;
	}

	IEnumerator AddRuns(int runs)
	{
		yield return new WaitForSeconds(1f);
		uiInfo.scoreThisHit.text = runs.ToString();
		uiInfo.scoreThisHit.gameObject.SetActive(true);
		yield return new WaitForSeconds(1f);
		uiInfo.scoreThisHit.gameObject.SetActive(false);
		uiInfo.runsPerBowl[currentPitchInOver].text = runs.ToString();
		if (runs % 2 == 1)
		{
			var x = currentBatterIndex;
			currentBatterIndex = otherBatterIndex;
			otherBatterIndex = x;
			RefreshBatterGraphics();
		}
		currentRuns += runs;
		currentBatterInfo.Runs += runs;

		if (isTourney)
		{
			if (currentRuns > TournamentManager.RequiredRuns())
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

	void OnGUI()
	{
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

}
