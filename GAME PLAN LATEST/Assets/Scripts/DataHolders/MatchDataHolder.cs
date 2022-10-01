using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MatchDataHolder
{
    // Visual Fields

    public Image BG;
    public TextMeshProUGUI FirstTeam;
    public TextMeshProUGUI SecondTeam;
    public TextMeshProUGUI Date;
    public TextMeshProUGUI Type;

    // Data Fields

    public int FirstTeamIndex;
    public int SecondTeamIndex;
    public bool StatusValue;

}
