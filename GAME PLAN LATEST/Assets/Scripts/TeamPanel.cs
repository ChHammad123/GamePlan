using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TeamPanel : SOCBehaviour {
	public Image setJerseyColor;
	public Text teamName;

	int teamIndex;
	public void SetDisplayedTeam(int index) {
		teamIndex = index;
		var team = TeamData.single.teams[index];
		teamName.text = team.name;
		setJerseyColor.color = team.jersey;
		//teamName.color = team.sleeve;
//		teamName.GetComponent<Outline>().effectColor = team.jersey.Inverse();
	}

	public void SetFieldingTeamToThis() {
		GameManager.single.SetFieldingTeam(teamIndex);
	}
	public void SetBattingTeamToThis() {
		GameManager.single.SetBattingTeam(teamIndex);
	}

}

