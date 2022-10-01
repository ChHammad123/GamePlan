using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class TeamInfoHolder
{
    public int TeamIndex = 0;
    
    public string TeamName = "";

    public int TotalMatchesPlayed = 0;

    public int MatchesWon = 0;

    public int MatchesLost = 0;

    public int MatchesTied = 0;
    
    public int MatchesNR = 0;

    public int points = 0;

    public Pool pool;

    public Sprite countryFlag;
}

public enum Pool
{
    A, B
}
