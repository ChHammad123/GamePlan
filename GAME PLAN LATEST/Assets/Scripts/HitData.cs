using UnityEngine;
using System.Collections;

public class HitData : MonoBehaviour 
{
	public static HitData single;
	public Ball ballTemplate;

	void Awake() { single = this; }

	public static Hit HitForType(HitType type) 
	{
		return single.hits[(int)type];
	}

	[CallFunction("SimulateAll","Sim All")]
	public bool nothing;

	public void SimulateAll()
	{
#if UNITY_EDITOR
		if ( !UnityEditor.EditorApplication.isPlaying ) {
			Debug.LogError ("Can only simulate in play mode");
			return;
		}
		SleightOfCode.StartFloatingCoroutine(SimAll());
#endif
	}

	IEnumerator SimAll() 
	{
		foreach(var hit in hits)
		{
			hit.Simulate();
			yield return new WaitForSeconds(0.2f);
		}
	}

	public enum HitType 
	{
		sixRightBack, sixLeftBack,
		sixRightFront, sixLeftFront,
		fourRightWall, fourLeftWall,
		threeRight, threeLeft,
		twoRightFielder, twoLeftFielder,
		outLeft, outRight, outRight2, outLeft2,
		NONE
	}

	public Hit[] hits;
}

[System.Serializable]
public class Hit : BallForce 
{
	public override void ExecuteForceOnly (GameObject ball)
	{
		ball.transform.position = HitData.single.transform.position;
		base.ExecuteForceOnly (ball);
	}
}

