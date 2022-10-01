using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamListPanel : SOCBehaviour {
	public TeamPanel teamPanelTemplate;

	TeamPanel[] panels;

	void Start() {
		int requiredPanels = TeamData.single.teams.Count;
		panels = new TeamPanel[requiredPanels];
		for(int i=0; i < requiredPanels; i++)
        {
			var panel = teamPanelTemplate.Instantiate();
			panel.rt.SetParent(rt);
			panel.rt.localScale = Vector3.one;
			panel.rt.localPosition = Vector3.right * (i * panel.rt.rect.width + panel.rt.rect.width / 2f);
			panel.SetDisplayedTeam(i);

			panels[i] = panel;
		}
		GameObject.Destroy(teamPanelTemplate.gameObject);
	}
}

