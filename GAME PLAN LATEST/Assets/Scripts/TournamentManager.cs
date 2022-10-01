using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using UnityEngine.UI;

public class TournamentManager : MonoBehaviour {

	[System.Serializable]
	public class UIStuff {
		public BracketUI finalBracket;
		public BracketUI[] semiFinalBrackets, quarterFinalBrackets;

		[System.Serializable]
		public class BracketUI {
			public Text teamName1, teamName2;
			public GameObject enableOnLoss1, enableOnWin1;
			public GameObject enableOnLoss2, enableOnWin2;
		}
	}

	public int[] runsRequiredProgression = new int[]{
		// within pool (6):
		8, 9, 11, 14, 18, 23,
		// finals (3):
		29, 36, 44, 53
	};

	public UIStuff uiStuff;

	public Pool[] pools;

	public static bool WonTourney { get; private set; }
	public static bool GotSecond { get; private set; }

	[System.Serializable]
	public class Pool {
		public int[] teams;


		public List<int> winners;
	}

	class Bracket {
		public Bracket(UIStuff.BracketUI ui, int t1, int t2) {
			myUI = ui;
			team1 = t1; team2 = t2;
			ui.teamName1.text = TeamData.single.teams[t1].name;
			ui.teamName2.text = TeamData.single.teams[t2].name;
		}
		public int team1, team2;
		public UIStuff.BracketUI myUI;

	}

	Bracket finals;
	Bracket[] semiFinals, quarterFinals;

	static int playerTeam;
	static Pool playerPool;
	static int currentGameInSeries = 0;
	static int playerPlaceInBrackets = 0;
	public static int wins, losses;
	static List<int> poolCombatOrder;

	public static bool InFinals { get; private set; }

	public static TournamentManager single;

	void Awake() { single = this; }

	public static int GetCurrentOpponent() {
		return poolCombatOrder[0];
	}

	public void Restart(int playerUsingTeam) 
	{
		wins = losses = 0;
		InFinals = false;
		playerTeam = playerUsingTeam;
		currentGameInSeries = 0;

		playerPool = single.pools.Where(x=>x.teams.Contains(playerTeam)).First();
		var copy = playerPool.teams.ToList();
		copy.Remove(playerTeam);
		poolCombatOrder = new List<int>();

		while(copy.Count > 0) 
		{
			int element = copy.RandomElement();
			copy.Remove(element);
			poolCombatOrder.Add(element);
		}
		int infinitePrevention = 0;

		playerPool.winners = new List<int>();
		copy = playerPool.teams.ToList();
		copy.Remove(playerTeam);

		for(int i=0; i < 4; i++) {
			int element = copy.RandomElement();
			var team = TeamData.single.teams[element];
			if ( Random.Range (0,100) < team.skill + infinitePrevention ) {
//				Debug.Log ("Winner " + element);
				copy.Remove (element);
				playerPool.winners.Add(element);
			} else {
				i--;
			}
		}

		playerPool.winners.Reverse(); // the first one you fight is the last-place winner

		var otherPool = single.pools.Where(x=>x!=playerPool).First();
		copy = otherPool.teams.ToList();
		otherPool.winners = new List<int>();
		infinitePrevention = 0;

		//string otherpoolteams = "";
		//foreach(var thing in copy) {
		//	otherpoolteams += thing + " , ";
		//}
		//Debug.Log("other pool: " + otherpoolteams);

		for(int i=0; i < 4; i++) {
			int element = copy.RandomElement();
			var team = TeamData.single.teams[element];
			if ( Random.Range (0,100) < team.skill + infinitePrevention) {
//				Debug.Log ("Winner " + element);
				copy.Remove (element);
				otherPool.winners.Add(element);
			} else { 
				i--;
			}
		}
	}

	public static int RequiredRuns()
	{
		if ( !currentGameInSeries.IsValidIndexFor(single.runsRequiredProgression))
		{
			return System.Int32.MaxValue;
		}

		return Random.Range(100, 200);
		//return single.runsRequiredProgression[currentGameInSeries];
	}

	public int FindFirstOpponent() {
		//wins--; // haha this is terrible
		currentGameInSeries--;
		return FindNextOpponent(true);
	}

	public int FindNextOpponent(bool playerWon) {
		if ( playerWon ) 
		{
			//wins++;
		} else 
		{
			//losses++;
			playerPlaceInBrackets++;
			if ( losses > 3 ) return -1;
		}

		currentGameInSeries++;
		int nextTeam = -1;

		if ( poolCombatOrder.Count > 0 ) {
			InFinals = false;
			nextTeam = poolCombatOrder[0];
			poolCombatOrder.RemoveAt(0);
		} else { // in brackets now
			InFinals = true;
			if ( null == quarterFinals ) {
				// the final qualification just ended, let's put the player in the right place
				playerPool.winners.Insert(playerPlaceInBrackets, playerTeam);

				quarterFinals = new Bracket[4];
				quarterFinals[0] = new Bracket(uiStuff.quarterFinalBrackets[0], pools[0].winners[0], pools[1].winners[3]);
				quarterFinals[1] = new Bracket(uiStuff.quarterFinalBrackets[1], pools[0].winners[1], pools[1].winners[2]);
				quarterFinals[2] = new Bracket(uiStuff.quarterFinalBrackets[2], pools[0].winners[2], pools[1].winners[1]);
				quarterFinals[3] = new Bracket(uiStuff.quarterFinalBrackets[3], pools[0].winners[3], pools[1].winners[0]);

				foreach(var bracket in quarterFinals) {
					if ( bracket.team1 == playerTeam ) nextTeam = bracket.team2;
					else if ( bracket.team2 == playerTeam ) nextTeam = bracket.team1;
				}
			} else if ( null == semiFinals ) {
				var winners = BracketWinners(quarterFinals, playerWon);
				semiFinals = new Bracket[2];
				semiFinals[0] = new Bracket(uiStuff.semiFinalBrackets[0], winners[0], winners[1]);
				semiFinals[1] = new Bracket(uiStuff.semiFinalBrackets[1], winners[2], winners[3]);

				var playerAt = winners.IndexOf(playerTeam);
				if ( playerAt % 2 == 0 ) nextTeam = winners[playerAt+1];
				else nextTeam = winners[playerAt-1];

			} else if ( null == finals ) {
				var winners = BracketWinners (semiFinals, playerWon);
				finals = new Bracket(uiStuff.finalBracket, winners[0], winners[1]);
				nextTeam = winners.Where(x=>x!=playerTeam).First();
			} else {
				if ( playerWon ) {
					WonTourney = true;
				} else {
					GotSecond = true;
				}
			}
		}

		return nextTeam;
	}

	List<int> BracketWinners(Bracket[] brackets, bool playerWon) {
		List<int> winners = new List<int>();
		foreach(var bracket in brackets) {
			bool won1 = false;
			if ( bracket.team1 == playerTeam ) {
				if ( playerWon ) { won1 = true; winners.Add(playerTeam); }
				else winners.Add(bracket.team2);
			} else if ( bracket.team2 == playerTeam ) {
				if ( playerWon ) winners.Add(playerTeam);
				else { won1 = true; winners.Add(bracket.team1); }
			} else {
				won1 = Random.Range (0f,1f)<0.5f;

				winners.Add( won1 ? bracket.team1 : bracket.team2 );
			}
			if ( won1 ) {
				bracket.myUI.enableOnWin1.SetActive(true);
				bracket.myUI.enableOnLoss2.SetActive(true);
			} else {
				bracket.myUI.enableOnWin2.SetActive(true);
				bracket.myUI.enableOnLoss1.SetActive(true);
			}
		}
		return winners;
	}
	
}

