using UnityEngine;
using System.Collections;

public class PersonMessageHandler : MonoBehaviour 
{
	public bool WorldCupOverride = false;

	public void HitNow() 
	{
		if (WorldCupOverride)
			GameManagerForWorldCup.HitNow();
		else
			GameManager.HitNow();
	}
	public void PitchNow() 
	{

		if(WorldCupOverride)
			GameManagerForWorldCup.PitchNow();
		else
			GameManager.PitchNow();
	}
}

