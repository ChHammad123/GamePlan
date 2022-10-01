using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PointsTableDataHolder
{
    public TextMeshProUGUI teamName;
    
    public TextMeshProUGUI Mat;
    
    public TextMeshProUGUI Won;

    public TextMeshProUGUI Lost;

    public TextMeshProUGUI Tied;

    public TextMeshProUGUI NR;

    public TextMeshProUGUI points;
} 

public class WorldCupScheduleManager : MonoBehaviour
{ 
    public GameManager gameManager;

    public static WorldCupScheduleManager instance;

    [Header("\n--------------- Panels --------------------\n ")]

    public GameObject SchedulePanel;
    public GameObject Ok, OkSemi, OkFinal;

    public GameObject PointsDataPanel;
    public GameObject SemiFinalPanel, YouWonSemiFinalPanel, YouLostSemiFinalPanel, YouDidNotQualifySemiFinalPanel , SemiFinalQualifiedTeams;
    public GameObject FinalPanel, YouWonFinalPanel, YouLostFinalPanel, YouDidNotQualifyFinalPanel , FinalQualifiedTeams;

    [Header("\n------ UI FOR SEMI & FINAL QUALIFIED TEAMS ------ \n")]
    public Image SemiFinal1FirstTeam;
    public Image SemiFinal1SecondTeam;
    public Image SemiFinal2FirstTeam;
    public Image SemiFinal2SecondTeam;

    public Image FinalFirstTeam;
    public Image FinalSecondTeam;

    public Button SemiFinalStartBtn;
    public Button FinalStartBtn;

    public TextMeshProUGUI SemiFinalStatusText;
    public TextMeshProUGUI FinalStatusText;

    [Tooltip("Names Indexes Must be in Synch With Array of Teams in Main Menu Script")]
    public string[] AllCountriesNames;

    [Header("\n--- ARRAYS RELATED TO REAL TIME CALCULATIONS --- \n")]
    public MatchInfoHolder[] WorldCupSchedule;

    public TeamInfoHolder[] AllTeamsInfo;

    [Header("\n--- ARRAYS RELATED TO UI --- \n")]
    public MatchDataHolder[] totalSelectedTeamMatches;

    public PointsTableDataHolder[] PointTableDataPoolA;
    
    public PointsTableDataHolder[] PointTableDataPoolB;

    public List<int> PoolATopTeamsIndexes;

    public List<int> PoolBTopTeamsIndexes;

    protected int onGoingMatchIndex = 0;

    protected int selectedTeamIndex = 0;

    public int progressCount = 0;

    int matchNumber = 1;

    int totalLeagueMatchesToPlay = 0;

    private void Awake()
    {
        instance = this;

        PoolATopTeamsIndexes = new List<int>();
        PoolBTopTeamsIndexes = new List<int>();
    }

    public void SetSelectedTeamIndex(int val)
    {
        selectedTeamIndex = val;
        int cnt = 0;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            switch (WorldCupSchedule[i].matchType)
            {
                case MatchType.League:

                    if (WorldCupSchedule[i].FirstTeamIndex == selectedTeamIndex)
                        cnt++;
                    
                    else if (WorldCupSchedule[i].SecondTeamIndex == selectedTeamIndex)
                        cnt++;
                    
                    break;
            }
        }

        totalLeagueMatchesToPlay = cnt;
    }

    public void SynchDataOnPanelSchedule()
    {
        bool areLeagueMatchesComplete = true;
        bool areSemisComplete = true;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            switch (WorldCupSchedule[i].matchType)
            {
                case MatchType.League:

                    if (!WorldCupSchedule[i].isComplete)
                    {
                        if (WorldCupSchedule[i].FirstTeamIndex == selectedTeamIndex || WorldCupSchedule[i].SecondTeamIndex == selectedTeamIndex)
                        {
                            areLeagueMatchesComplete = false;
                        }
                    }
                    break;


                case MatchType.Semi:

                    if (!WorldCupSchedule[i].isComplete)
                    {
                        areSemisComplete = false;
                    }

                    break;
            }
        }

        if (areLeagueMatchesComplete && !areSemisComplete)
        {
            Ok.SetActive(false);
            OkSemi.SetActive(true);

            int tempCnt = 0;

            for (int i = 0; i < WorldCupSchedule.Length; i++)
            {
                switch (WorldCupSchedule[i].matchType)
                {
                    case MatchType.Semi:
                        WorldCupSchedule[i].FirstTeamIndex = PoolATopTeamsIndexes[tempCnt];
                        WorldCupSchedule[i].SecondTeamIndex = PoolBTopTeamsIndexes[tempCnt];
                        tempCnt++;

                        if (selectedTeamIndex != WorldCupSchedule[i].FirstTeamIndex && selectedTeamIndex != WorldCupSchedule[i].SecondTeamIndex)
                        {
                            int fate = UnityEngine.Random.Range(1, 3);

                            WorldCupSchedule[i].isComplete = true;

                            if (fate == 1)
                            {
                                WorldCupSchedule[i].wonBy = WorldCupSchedule[i].FirstTeamIndex;
                            }

                            else
                            {
                                WorldCupSchedule[i].wonBy = WorldCupSchedule[i].SecondTeamIndex;
                            }

                        }

                        break;
                }
            }
        }

        else if (areSemisComplete)
        {
            Ok.SetActive(false);
            OkFinal.SetActive(true);

            PoolATopTeamsIndexes.Clear();
            PoolBTopTeamsIndexes.Clear();

            bool isFirstflag = true;

            for (int i = 0; i < WorldCupSchedule.Length; i++)
            {
                switch (WorldCupSchedule[i].matchType)
                {
                    case MatchType.Semi:

                        if(isFirstflag)
                        {
                            WorldCupSchedule[WorldCupSchedule.Length - 1].FirstTeamIndex = WorldCupSchedule[i].wonBy;

                            isFirstflag = false;
                        }
                        else
                        {
                            WorldCupSchedule[WorldCupSchedule.Length - 1].SecondTeamIndex = WorldCupSchedule[i].wonBy;
                        }
                        break;
                }
            }

        }

        progressCount += 5;

        int batterIndex = selectedTeamIndex;

        // Assigning Data to UI

        int cnt = 0;

        bool flag = false;

        bool BGFlag = false;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            // BG Setting For Shown Matches (Red Green Cyan)

            if (WorldCupSchedule[i].FirstTeamIndex == batterIndex || WorldCupSchedule[i].SecondTeamIndex == batterIndex)
            {
                if (WorldCupSchedule[i].isComplete)
                {
                    totalSelectedTeamMatches[cnt].BG.gameObject.SetActive(true);

                    if (WorldCupSchedule[i].wonBy == batterIndex)
                        totalSelectedTeamMatches[cnt].BG.color = Color.green;
                    else
                        totalSelectedTeamMatches[cnt].BG.color = Color.red;
                }

                else
                {
                    if (!BGFlag)
                    {
                        BGFlag = true;
                        totalSelectedTeamMatches[cnt].BG.gameObject.SetActive(true);
                        totalSelectedTeamMatches[cnt].BG.color = Color.cyan;
                    }
                    else
                        totalSelectedTeamMatches[cnt].BG.gameObject.SetActive(false);
                }

            }

            switch (WorldCupSchedule[i].matchType)
            {
                case MatchType.League:

                    if (WorldCupSchedule[i].FirstTeamIndex == batterIndex)
                    {
                        totalSelectedTeamMatches[cnt].FirstTeam.text = AllCountriesNames[WorldCupSchedule[i].FirstTeamIndex] + "";
                        totalSelectedTeamMatches[cnt].SecondTeam.text = AllCountriesNames[WorldCupSchedule[i].SecondTeamIndex] + "";

                        totalSelectedTeamMatches[cnt].Date.text = WorldCupSchedule[i].date + "";
                        totalSelectedTeamMatches[cnt].Type.text = WorldCupSchedule[i].matchType + "";

                        cnt++;

                    }

                    else if (WorldCupSchedule[i].SecondTeamIndex == batterIndex)
                    {
                        totalSelectedTeamMatches[cnt].FirstTeam.text = AllCountriesNames[WorldCupSchedule[i].SecondTeamIndex] + "";
                        totalSelectedTeamMatches[cnt].SecondTeam.text = AllCountriesNames[WorldCupSchedule[i].FirstTeamIndex] + "";

                        totalSelectedTeamMatches[cnt].Date.text = WorldCupSchedule[i].date + "";
                        totalSelectedTeamMatches[cnt].Type.text = WorldCupSchedule[i].matchType + "";

                        cnt++;
                    }

                    break;


                case MatchType.Semi:

                    if (!areLeagueMatchesComplete)
                    {
                        if (flag)
                            continue;

                        flag = true;

                        totalSelectedTeamMatches[cnt].FirstTeam.text = "-";
                        totalSelectedTeamMatches[cnt].SecondTeam.text = "-";

                        totalSelectedTeamMatches[cnt].Date.text = WorldCupSchedule[i].date + "";
                        totalSelectedTeamMatches[cnt].Type.text = WorldCupSchedule[i].matchType + "";

                        cnt++;
                    }

                    else
                    {
                        if (flag)
                            continue;

                        if (WorldCupSchedule[i].FirstTeamIndex == batterIndex)
                        {
                            totalSelectedTeamMatches[cnt].FirstTeam.text = AllCountriesNames[WorldCupSchedule[i].FirstTeamIndex] + "";
                            totalSelectedTeamMatches[cnt].SecondTeam.text = AllCountriesNames[WorldCupSchedule[i].SecondTeamIndex] + "";

                            totalSelectedTeamMatches[cnt].Date.text = WorldCupSchedule[i].date + "";
                            totalSelectedTeamMatches[cnt].Type.text = WorldCupSchedule[i].matchType + "";

                            cnt++;

                            flag = true;
                        }

                        else if (WorldCupSchedule[i].SecondTeamIndex == batterIndex)
                        {
                            totalSelectedTeamMatches[cnt].FirstTeam.text = AllCountriesNames[WorldCupSchedule[i].SecondTeamIndex] + "";
                            totalSelectedTeamMatches[cnt].SecondTeam.text = AllCountriesNames[WorldCupSchedule[i].FirstTeamIndex] + "";

                            totalSelectedTeamMatches[cnt].Date.text = WorldCupSchedule[i].date + "";
                            totalSelectedTeamMatches[cnt].Type.text = WorldCupSchedule[i].matchType + "";

                            cnt++;

                            flag = true;
                        }
                    }

                    break;

                case MatchType.Final:

                    if(!areLeagueMatchesComplete)
                    {
                        totalSelectedTeamMatches[cnt].FirstTeam.text = "-";
                        totalSelectedTeamMatches[cnt].SecondTeam.text = "-";

                        totalSelectedTeamMatches[cnt].Date.text = WorldCupSchedule[i].date + "";
                        totalSelectedTeamMatches[cnt].Type.text = WorldCupSchedule[i].matchType + "";
                        cnt++;
                    }

                    else if (areSemisComplete)
                    {
                        if (WorldCupSchedule[i].FirstTeamIndex == batterIndex)
                        {
                            totalSelectedTeamMatches[cnt].FirstTeam.text = AllCountriesNames[WorldCupSchedule[i].FirstTeamIndex] + "";
                            totalSelectedTeamMatches[cnt].SecondTeam.text = AllCountriesNames[WorldCupSchedule[i].SecondTeamIndex] + "";

                            totalSelectedTeamMatches[cnt].Date.text = WorldCupSchedule[i].date + "";
                            totalSelectedTeamMatches[cnt].Type.text = WorldCupSchedule[i].matchType + "";

                            cnt++;
                        }
                        else
                        {
                            totalSelectedTeamMatches[cnt].FirstTeam.text = AllCountriesNames[WorldCupSchedule[i].SecondTeamIndex] + "";
                            totalSelectedTeamMatches[cnt].SecondTeam.text = AllCountriesNames[WorldCupSchedule[i].FirstTeamIndex] + "";

                            totalSelectedTeamMatches[cnt].Date.text = WorldCupSchedule[i].date + "";
                            totalSelectedTeamMatches[cnt].Type.text = WorldCupSchedule[i].matchType + "";
                        }                  
                    }

                    break;
            }
        }

    }

    public void SynchDataOnPanelPointsTable()
    {
        int attacker = selectedTeamIndex;

        // Deciding Fate of matches played not by player

        if (progressCount > (WorldCupSchedule.Length - 1)) 
            progressCount =  WorldCupSchedule.Length - 1;

        for (int i = 0; i <= progressCount; i++)
        {
            if (attacker != WorldCupSchedule[i].FirstTeamIndex && attacker != WorldCupSchedule[i].SecondTeamIndex)
            {
                if (WorldCupSchedule[i].isComplete)
                    continue;

                switch(WorldCupSchedule[i].matchType)
                {
                    case MatchType.League:

                        int fate = UnityEngine.Random.Range(1, 3);

                        WorldCupSchedule[i].isComplete = true;

                        if (fate == 1)
                        {
                            WorldCupSchedule[i].wonBy = WorldCupSchedule[i].FirstTeamIndex;

                            AllTeamsInfo[WorldCupSchedule[i].FirstTeamIndex].points += 2;
                            AllTeamsInfo[WorldCupSchedule[i].FirstTeamIndex].TotalMatchesPlayed++;
                            AllTeamsInfo[WorldCupSchedule[i].FirstTeamIndex].MatchesWon++;


                            AllTeamsInfo[WorldCupSchedule[i].SecondTeamIndex].TotalMatchesPlayed++;
                            AllTeamsInfo[WorldCupSchedule[i].SecondTeamIndex].MatchesLost++;
                        }

                        else
                        {
                            WorldCupSchedule[i].wonBy = WorldCupSchedule[i].SecondTeamIndex;

                            AllTeamsInfo[WorldCupSchedule[i].SecondTeamIndex].points += 2;
                            AllTeamsInfo[WorldCupSchedule[i].SecondTeamIndex].TotalMatchesPlayed++;
                            AllTeamsInfo[WorldCupSchedule[i].SecondTeamIndex].MatchesWon++;


                            AllTeamsInfo[WorldCupSchedule[i].FirstTeamIndex].TotalMatchesPlayed++;
                            AllTeamsInfo[WorldCupSchedule[i].FirstTeamIndex].MatchesLost++;
                        }

                        break;
                }
            }
        }


        TeamInfoHolder[] Temp = new TeamInfoHolder[10];

        for (int i = 0; i < Temp.Length; i++) 
            Temp[i] = AllTeamsInfo[i];

        // Sorting Pool A Teams For Points Table
        for (int i = 0; i < AllTeamsInfo.Length; i++)
        {
            switch(AllTeamsInfo[i].pool)
            {
                case Pool.A:
                    for (int j = 0; j < (AllTeamsInfo.Length); j++)
                    {
                        switch (AllTeamsInfo[j].pool)
                        {
                            case Pool.A:
                                if (AllTeamsInfo[i].points > AllTeamsInfo[j].points)
                                {
                                    TeamInfoHolder temp = new TeamInfoHolder();

                                    temp = AllTeamsInfo[i];
                                    AllTeamsInfo[i] = AllTeamsInfo[j];
                                    AllTeamsInfo[j] = temp;
                                }

                                break;
                        }
                    }
                    break;
            }
        }

        // Sorting Pool B Teams For Points Table
        for (int i = 0; i < AllTeamsInfo.Length; i++)
        {
            switch(AllTeamsInfo[i].pool)
            {
                case Pool.B:
                    for (int j = 0; j < (AllTeamsInfo.Length); j++)
                    {
                        switch (AllTeamsInfo[j].pool)
                        {
                            case Pool.B:
                                if (AllTeamsInfo[i].points > AllTeamsInfo[j].points)
                                {
                                    TeamInfoHolder temp = new TeamInfoHolder();

                                    temp = AllTeamsInfo[i];
                                    AllTeamsInfo[i] = AllTeamsInfo[j];
                                    AllTeamsInfo[j] = temp;
                                }

                                break;
                        }
                    }
                    break;
            }
        }

        int cnt = 0;

        PoolATopTeamsIndexes.Clear();
        PoolBTopTeamsIndexes.Clear();

        // Assigning Pool A Values To Text Fields
        for (int i = 0; i < AllTeamsInfo.Length; i++)
        {
            switch(AllTeamsInfo[i].pool)
            {
                case Pool.A:

                    if (PoolATopTeamsIndexes.Count < 2)
                        PoolATopTeamsIndexes.Add(AllTeamsInfo[i].TeamIndex);

                    PointTableDataPoolA[cnt].teamName.text = AllTeamsInfo[i].TeamName + "";
                    PointTableDataPoolA[cnt].Mat.text = AllTeamsInfo[i].TotalMatchesPlayed + "";
                    PointTableDataPoolA[cnt].Won.text = AllTeamsInfo[i].MatchesWon + "";
                    PointTableDataPoolA[cnt].Lost.text = AllTeamsInfo[i].MatchesLost + "";
                    PointTableDataPoolA[cnt].Tied.text = AllTeamsInfo[i].MatchesTied + "";
                    PointTableDataPoolA[cnt].NR.text = AllTeamsInfo[i].MatchesNR + "";
                    PointTableDataPoolA[cnt].points.text = AllTeamsInfo[i].points + "";

                    cnt++;
                    break;
            }
        }

        cnt = 0;

        // Assigning Pool B Values To Text Fields
        for (int i = 0; i < AllTeamsInfo.Length; i++)
        {
            switch(AllTeamsInfo[i].pool)
            {
                case Pool.B:

                    if (PoolBTopTeamsIndexes.Count < 2)
                        PoolBTopTeamsIndexes.Add(AllTeamsInfo[i].TeamIndex);

                    PointTableDataPoolB[cnt].teamName.text = AllTeamsInfo[i].TeamName + "";
                    PointTableDataPoolB[cnt].Mat.text = AllTeamsInfo[i].TotalMatchesPlayed + "";
                    PointTableDataPoolB[cnt].Won.text = AllTeamsInfo[i].MatchesWon + "";
                    PointTableDataPoolB[cnt].Lost.text = AllTeamsInfo[i].MatchesLost + "";
                    PointTableDataPoolB[cnt].Tied.text = AllTeamsInfo[i].MatchesTied + "";
                    PointTableDataPoolB[cnt].NR.text = AllTeamsInfo[i].MatchesNR + "";
                    PointTableDataPoolB[cnt].points.text = AllTeamsInfo[i].points + "";

                    cnt++;
                    break;
            }
        }

        for (int i = 0; i < Temp.Length; i++)
            AllTeamsInfo[i] = Temp[i];

    }

    public void StartWorldCupMatch()
    {
        bool areLeagueMatchesComplete = true;
        bool areSemisComplete = true;
        bool didPlayerQualifyToSemiFinal = false;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            switch(WorldCupSchedule[i].matchType)
            {
                case MatchType.League:

                    if (!WorldCupSchedule[i].isComplete)
                    {
                        if (WorldCupSchedule[i].FirstTeamIndex == selectedTeamIndex || WorldCupSchedule[i].SecondTeamIndex == selectedTeamIndex)
                        {
                            areLeagueMatchesComplete = false;
                        }
                    }
                    break;


                case MatchType.Semi:

                    if (!WorldCupSchedule[i].isComplete)
                    {
                        if (WorldCupSchedule[i].FirstTeamIndex == selectedTeamIndex || WorldCupSchedule[i].SecondTeamIndex == selectedTeamIndex)
                            didPlayerQualifyToSemiFinal = true;
                        
                        areSemisComplete = false;
                    }
                    else
                    {
                        if (WorldCupSchedule[i].FirstTeamIndex == selectedTeamIndex || WorldCupSchedule[i].SecondTeamIndex == selectedTeamIndex)
                            didPlayerQualifyToSemiFinal = true;
                    }
                    break;
            }
        }

        if (!areLeagueMatchesComplete)
            StartLeagueMatch();
        
        // For Semi Final
        else if (!areSemisComplete)     
        {
            SchedulePanel.SetActive(false);

            PointsDataPanel.SetActive(false);

            bool isQualified = false;

            int attacker = selectedTeamIndex;

            for (int i = 0; i < WorldCupSchedule.Length; i++)
            {
                switch (WorldCupSchedule[i].matchType)
                {
                    case MatchType.Semi:

                        if (attacker == WorldCupSchedule[i].FirstTeamIndex || attacker == WorldCupSchedule[i].SecondTeamIndex)
                            isQualified = true;

                        break;
                }
            }

            if (isQualified)
                SemiFinalPanel.SetActive(true);

            else
                YouDidNotQualifySemiFinalPanel.SetActive(true);
        }

        // For Semi Final (Player Didnt Qualified)
        else if(areSemisComplete && !didPlayerQualifyToSemiFinal)
        {
            Debug.Log("Are Semis Complete = " + areSemisComplete + " and did Player Qualified For Semi Final " + didPlayerQualifyToSemiFinal);


            YouDidNotQualifySemiFinalPanel.SetActive(true);
        }

        // For Final
        else
        {
            SchedulePanel.SetActive(false);

            PointsDataPanel.SetActive(false);

            bool isQualified = false;

            int attacker = selectedTeamIndex;

            for (int i = 0; i < WorldCupSchedule.Length; i++)
            {
                switch (WorldCupSchedule[i].matchType)
                {
                    case MatchType.Final:

                        if (attacker == WorldCupSchedule[i].FirstTeamIndex || attacker == WorldCupSchedule[i].SecondTeamIndex)
                            isQualified = true;

                        break;
                }
            }

            if (isQualified)
            {
                FinalPanel.SetActive(true);
            }
            else
            {
                YouDidNotQualifyFinalPanel.SetActive(true);
            }
        }
    }

    public void StartLeagueMatch()
    {
        int attacker = selectedTeamIndex;
        int defender = 0;

        onGoingMatchIndex = 0;

        int skips = matchNumber;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            if (attacker == WorldCupSchedule[i].FirstTeamIndex)
            {
                if (skips != 1)
                {
                    skips--;
                    continue;
                }

                onGoingMatchIndex = i;
                defender = WorldCupSchedule[i].SecondTeamIndex;

                break;
            }

            else if (attacker == WorldCupSchedule[i].SecondTeamIndex)
            {
                if (skips != 1)
                {
                    skips--;
                    continue;
                }

                onGoingMatchIndex = i;
                defender = WorldCupSchedule[i].FirstTeamIndex;
                break;
            }
        }

        gameManager.SetBattingTeam(attacker);
        gameManager.SetFieldingTeam(defender);

        gameManager.currentWorldCupMatchIndex = onGoingMatchIndex;
        gameManager.StartGame();
    }

    public void StartSemiFinalMatch()
    {
        int attacker = selectedTeamIndex;
        int defender = 0;

        onGoingMatchIndex = 0;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            switch (WorldCupSchedule[i].matchType)
            {
                case MatchType.Semi:

                    if (attacker == WorldCupSchedule[i].FirstTeamIndex)
                    {
                        onGoingMatchIndex = i;
                        defender = WorldCupSchedule[i].SecondTeamIndex;
                    }
                    else if (attacker == WorldCupSchedule[i].SecondTeamIndex)
                    {
                        onGoingMatchIndex = i;
                        defender = WorldCupSchedule[i].FirstTeamIndex;
                    }
                    break;
            }
        }

        gameManager.SetBattingTeam(attacker);
        gameManager.SetFieldingTeam(defender);

        gameManager.currentWorldCupMatchIndex = onGoingMatchIndex;
        gameManager.StartGame();
    }
    
    public void StartFinalMatch()
    {
        int attacker = selectedTeamIndex;
        int defender = 0;

        onGoingMatchIndex = 0;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            switch (WorldCupSchedule[i].matchType)
            {
                case MatchType.Final:

                    if (attacker == WorldCupSchedule[i].FirstTeamIndex)
                    {
                        onGoingMatchIndex = i;
                        defender = WorldCupSchedule[i].SecondTeamIndex;
                    }
                    else if (attacker == WorldCupSchedule[i].SecondTeamIndex)
                    {
                        onGoingMatchIndex = i;
                        defender = WorldCupSchedule[i].FirstTeamIndex;
                    }
                    break;
            }
        }


        gameManager.SetBattingTeam(attacker);
        gameManager.SetFieldingTeam(defender);

        gameManager.currentWorldCupMatchIndex = onGoingMatchIndex;
        gameManager.StartGame();

    }

    public void WorldCupMatchCompleted(int matchIndex, int winningTeamIndex)
    {
        if (matchIndex < 0 || matchIndex >= WorldCupSchedule.Length)
        {
            Debug.LogError("Match Index Exceeded");
            return;
        }
        
        WorldCupSchedule[matchIndex].wonBy = winningTeamIndex;
        WorldCupSchedule[matchIndex].isComplete = true;

        switch (WorldCupSchedule[matchIndex].matchType)
        {
            case MatchType.League:
                if (winningTeamIndex == selectedTeamIndex)
                {
                    GameManager.single.uiInfo.enableToShowLoss.SetActive(false);
                    GameManager.single.uiInfo.enableToShowWin.SetActive(true);
                }
                else
                {
                    GameManager.single.uiInfo.enableToShowLoss.SetActive(true);
                    GameManager.single.uiInfo.enableToShowWin.SetActive(false);
                }
                if (WorldCupSchedule[matchIndex].FirstTeamIndex == winningTeamIndex)
                {

                    AllTeamsInfo[WorldCupSchedule[matchIndex].FirstTeamIndex].points += 2;
                    AllTeamsInfo[WorldCupSchedule[matchIndex].FirstTeamIndex].TotalMatchesPlayed++;
                    AllTeamsInfo[WorldCupSchedule[matchIndex].FirstTeamIndex].MatchesWon++;


                    AllTeamsInfo[WorldCupSchedule[matchIndex].SecondTeamIndex].TotalMatchesPlayed++;
                    AllTeamsInfo[WorldCupSchedule[matchIndex].SecondTeamIndex].MatchesLost++;
                }

                else
                {
                    AllTeamsInfo[WorldCupSchedule[matchIndex].SecondTeamIndex].points += 2;
                    AllTeamsInfo[WorldCupSchedule[matchIndex].SecondTeamIndex].TotalMatchesPlayed++;
                    AllTeamsInfo[WorldCupSchedule[matchIndex].SecondTeamIndex].MatchesWon++;


                    AllTeamsInfo[WorldCupSchedule[matchIndex].FirstTeamIndex].TotalMatchesPlayed++;
                    AllTeamsInfo[WorldCupSchedule[matchIndex].FirstTeamIndex].MatchesLost++;
                }

                break;

            case MatchType.Semi:
                if (winningTeamIndex == selectedTeamIndex)
                    YouWonSemiFinalPanel.SetActive(true);
                else
                {
                    YouLostSemiFinalPanel.SetActive(true);
                }
                break;

            case MatchType.Final:

                if (winningTeamIndex == selectedTeamIndex)
                    YouWonFinalPanel.SetActive(true);
                else
                    YouLostFinalPanel.SetActive(true);
                break;

        }

        SynchDataOnPanelPointsTable();

        SynchDataOnPanelSchedule();

        matchNumber++;
    }

    public void SetDataOnPanelSemiFinalQualifiers()
    {
        bool flag = true;

        int didPlayerQualifiedCnt = 0;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            switch(WorldCupSchedule[i].matchType)
            {
                case MatchType.Semi: 
                    
                    if (flag)
                    {
                        if (WorldCupSchedule[i].FirstTeamIndex != selectedTeamIndex && WorldCupSchedule[i].SecondTeamIndex != selectedTeamIndex)
                        {
                            didPlayerQualifiedCnt++;
                        }

                        SemiFinal1FirstTeam.sprite = AllTeamsInfo[WorldCupSchedule[i].FirstTeamIndex].countryFlag;
                        SemiFinal1SecondTeam.sprite = AllTeamsInfo[WorldCupSchedule[i].SecondTeamIndex].countryFlag;

                        flag = false;
                    }

                    else
                    {
                        if (WorldCupSchedule[i].FirstTeamIndex != selectedTeamIndex && WorldCupSchedule[i].SecondTeamIndex != selectedTeamIndex)
                        {
                            didPlayerQualifiedCnt++;
                        }

                        SemiFinal2FirstTeam.sprite = AllTeamsInfo[WorldCupSchedule[i].FirstTeamIndex].countryFlag;
                        SemiFinal2SecondTeam.sprite = AllTeamsInfo[WorldCupSchedule[i].SecondTeamIndex].countryFlag;
                    }

                    break;
            }
        }

        if(didPlayerQualifiedCnt == 2)
        {
            SemiFinalStatusText.text = "You Did Not Qualified For Semi Final";
            SemiFinalStatusText.color = Color.yellow;

            SemiFinalStartBtn.gameObject.SetActive(false);
            SemiFinalStatusText.gameObject.SetActive(true);
        }
    }

    public void SetDataOnPanelFinalQualifiers()
    {
        bool isComplete = false;
        bool didPlayerQualified = true;

        for (int i = 0; i < WorldCupSchedule.Length; i++)
        {
            switch (WorldCupSchedule[i].matchType)
            {
                case MatchType.Final:

                    if (WorldCupSchedule[i].isComplete)
                    {
                        if(WorldCupSchedule[i].FirstTeamIndex == selectedTeamIndex || WorldCupSchedule[i].SecondTeamIndex == selectedTeamIndex)
                        {
                            if (WorldCupSchedule[i].wonBy == selectedTeamIndex)
                            {
                                FinalStatusText.text = "You Won";
                                FinalStatusText.color = Color.cyan;
                            }
                            else
                            {
                                FinalStatusText.text = "You Lost";
                                FinalStatusText.color = Color.red;
                            }
                        }
                        else
                        {
                            FinalStatusText.text = "You Did Not Qualified For FINAL";
                            FinalStatusText.color = Color.yellow;

                            FinalStartBtn.gameObject.SetActive(false);
                            FinalStatusText.gameObject.SetActive(true);
                        }
                        isComplete = true;
                    }
                    else if (WorldCupSchedule[i].FirstTeamIndex != selectedTeamIndex && WorldCupSchedule[i].SecondTeamIndex != selectedTeamIndex)
                    {
                        didPlayerQualified = false;
                    }

                    FinalFirstTeam.sprite = AllTeamsInfo[WorldCupSchedule[i].FirstTeamIndex].countryFlag;
                    FinalSecondTeam.sprite = AllTeamsInfo[WorldCupSchedule[i].SecondTeamIndex].countryFlag;

                    break;
            }
        }

        if (isComplete)
        { 
            FinalStartBtn.gameObject.SetActive(false);
            FinalStatusText.gameObject.SetActive(true);
        }
        else if(!didPlayerQualified)
        {
            FinalStatusText.text = "You Did Not Qualified For FINAL";
            FinalStatusText.color = Color.yellow;

            FinalStartBtn.gameObject.SetActive(false);
            FinalStatusText.gameObject.SetActive(true);
        }
    }
}
