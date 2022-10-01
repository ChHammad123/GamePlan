using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MatchInfoHolder 
{
    public int FirstTeamIndex;

    public int SecondTeamIndex;

    public int wonBy = -1;

    public bool isComplete = false;

    public string date;

    public MatchType matchType;
}

public enum MatchType
{
    League, Semi , Final 
}