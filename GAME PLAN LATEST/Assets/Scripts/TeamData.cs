using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamData : MonoBehaviour {

	public static TeamData single;

	public TextAsset readFrom;

	void Awake() { 
		single = this;
	
		string[] text = readFrom.text.Split('\n');

		Team current = null;

		teams = new List<Team>();

		for(int i=0; i < text.Length; i++) {
			var line = text[i];
			if ( line.Length == 0 ) continue;

			if ( line[0] == '#' ) {
				current = new Team();
				current.skill = 100;
				current.name = line.Without("#");
				if ( current.name.Contains("/") )
                {
					int index = current.name.IndexOf("/");
					current.shortName = current.name.Substring(index+1);
					current.name = current.name.Substring(0,index);
				} else {
					current.shortName = current.name.Substring(0,3).ToUpper();
				}
				teams.Add(current);
				current.skin = Color.white;
			} else if ( null == current ) {
				continue;
			} else if ( line.Contains("Skill-") ) {
				current.skill = Parse(line.Without("Skill-"));
			} else if ( line.Contains("Sleeve-") ) {
				var vals = line.Without("Sleeve-").Split(',');
				current.sleeve = new Color(Parse(vals[0])/255f,Parse(vals[1])/255f,Parse(vals[2])/255f,1f);
			} else if ( line.Contains("Skin-") ) {
				var vals = line.Without("Skin-").Split(',');
				current.skin = new Color(Parse(vals[0])/255f,Parse(vals[1])/255f,Parse(vals[2])/255f,1f);
			} else if ( line.Contains("Jersey-") ) {
				var vals = line.Without("Jersey-").Split(',');
				current.jersey = new Color(Parse(vals[0])/255f,Parse(vals[1])/255f,Parse(vals[2])/255f,1f);
				if ( current.sleeve.a == 0f ) current.sleeve = current.jersey;
			} else if ( line.Contains(".") ) {
				int nameStartsAt = line.IndexOf(".") - 1;
				Player player = new Player();
				player.name = line.Substring(nameStartsAt);
				current.players.Add(player);
			}
		}
	} 

	int Parse(string val) { return System.Int32.Parse(val); }

	public List<Team> teams;
}

[System.Serializable]
public class Team {
	public string name = "";
	public string shortName = "";
	public int skill;
	public Color jersey;
	public Color sleeve;
	public Color skin;

	public List<Player> players = new List<Player>();
}

[System.Serializable]
public class Player {
	public string name = "";

	public bool isOut = false;
//	public Color skin;
}

