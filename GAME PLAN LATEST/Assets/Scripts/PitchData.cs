using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class PitchData : MonoBehaviour
{
	public Pitch[] pitches;

#if UNITY_EDITOR
	void Update() 
	{
		GUIUtils.ColorWheelResolution = 16;

		foreach (var pitch in pitches) 
		{
			Vector3 dir = Vector3.down * 2f;

			for (int i = 0; i < pitch.strikeTimes.Length; i++) 
			{
				dir.y *= -1f;

				Color color = GUIUtils.ColorWheelElement(i);

				var sti = pitch.strikeTimes[i];

				int keyIndex = 0;

				var keys = pitch.lineSegments.Keys.ToArray();

				for(keyIndex = 0; keyIndex < keys.Length; keyIndex++)
				{
					var key = keys[keyIndex];

					if ( key >= sti.time.min ) 
					{
						Debug.DrawLine(pitch.lineSegments[key].from, pitch.lineSegments[key].from + dir, color);
						break;
					}
				}

				for( ; keyIndex < keys.Length; keyIndex++ ) 
				{
					var key = keys[keyIndex];

					if ( keyIndex == keys.Length-1 )
					{
						Debug.DrawLine(pitch.lineSegments[key].from, pitch.lineSegments[key].from + dir, color);
					}
					else if ( key >= sti.time.max && keyIndex > 0 ) 
					{
						key = keys[keyIndex-1];

						Debug.DrawLine(pitch.lineSegments[key].from, pitch.lineSegments[key].from + dir, color);

						break;
					}
				}
			}
		}
	}

#endif
}

[System.Serializable]
public class BallForce 
{
	public enum BallSpeedMode
	{
		Easy, MediumSpin, MediumFast, Hard
	}

	public BallSpeedMode speedDifficulty;

	[Header("Others \n ")]
	public static BallForce instance;

	public string name;

	public int FastForce;

    [Tooltip(" Give Negative Value To Slow Ball and Positive To Speed up the Ball")]
	public int NormalForce;

	[Tooltip(" Give Negative Value To Slow Ball and Positive To Speed up the Ball")]
	public int SlowForce;

	[CallFunction("Simulate")]
	public bool simulate;
	
	public Vector3 ballOrigin;
	public Vector3 ballVelocity;
	public Vector3 ballForce;

	public string direction = "leg";

	public Dictionary<float, PersistentDebugLine.Segment> lineSegments = new Dictionary<float, PersistentDebugLine.Segment>();
	
	public virtual void Execute(GameObject ball) 
	{
		instance = this;

		ball.transform.position = ballOrigin;

		ExecuteForceOnly(ball);

		SleightOfCode.StartFloatingCoroutine (EndBowlSoon());
	}

	IEnumerator EndBowlSoon() 
	{
		yield return new WaitForSeconds(5f);

		GameManager.EndBowl();
	}

	public virtual void ExecuteForceOnly(GameObject ball) 
	{
		var rb = ball.transform.GetOrAddComponent<Rigidbody>();

		rb.velocity = ballVelocity;

		var cf = ball.transform.GetOrAddComponent<ConstantForce>();

		cf.force = ballForce;

		int newForce, r;

		switch (speedDifficulty)
        {
			case BallSpeedMode.Easy:

				direction = "out";

				rb.AddForce(Vector3.forward * NormalForce);

				break;


			case BallSpeedMode.MediumSpin:

				r = Random.Range(-1, 2);

				if (r == 1)
				{
					rb.AddRelativeForce(Vector3.right * 30);
					direction = "off";
				}

				else if (r == -1)
				{
					rb.AddRelativeForce(Vector3.right * -30);
					direction = "leg";
				}

				else
					direction = "out";

				rb.AddForce(Vector3.forward * NormalForce);

				break;

			case BallSpeedMode.MediumFast :

				newForce = Random.Range(-1, 2);

				if (newForce > 0)
					rb.AddForce(Vector3.forward * FastForce);

				else if (newForce < 0)
					rb.AddForce(Vector3.forward * SlowForce);

				else
					rb.AddForce(Vector3.forward * NormalForce);

				direction = "out";

				break;


			case BallSpeedMode.Hard:

				r = Random.Range(-1, 2);

				newForce = Random.Range(-1, 2);

				if (r == 1)
				{
					rb.AddRelativeForce(Vector3.right * 30);
					direction = "off";
				}

				else if (r == -1)
				{
					rb.AddRelativeForce(Vector3.right * -30);
					direction = "leg";
				}

				else
					direction = "out";


				if (newForce > 0)
					rb.AddForce(Vector3.forward * FastForce);

				else if (newForce < 0)
					rb.AddForce(Vector3.forward * SlowForce);

				else
					rb.AddForce(Vector3.forward * NormalForce);

				break;
		}
	}

	
	public void Simulate() 
	{
#if UNITY_EDITOR
		if ( !UnityEditor.EditorApplication.isPlaying )
		{
			Debug.LogError("Can only simulate while in play mode");
			return;
		}
		if ( HitData.single )
		{
			if ( HitData.single.ballTemplate ) 
			{
				var ball = HitData.single.ballTemplate.Instantiate();
				SleightOfCode.StartFloatingCoroutine(RunSimulate(ball.gameObject));
			} 
			else 
			{
				Debug.Log ("HitData needs a ball template");
				return;
			}
		} 
		else
		{
			Debug.LogError ("HitData not found");
			return;
		}
#endif
	}
	
	bool simming = false, stopsim = false;
	
	private IEnumerator RunSimulate(GameObject ball) 
	{
		Execute(ball);

		Vector3 lastPosition = ball.transform.position;
		
		while(simming)
		{
			stopsim = true;
			yield return null;
		}

		stopsim = false;
		simming = true;
		
		PersistentDebugLine.UnregisterLine(lineSegments.Values.ToArray());

		lineSegments.Clear();

		for (float t = 0f; t < 3f; t += Time.deltaTime) 
		{
			lastPosition = ball.transform.position;

			yield return null;

			lineSegments.Add(t,PersistentDebugLine.RegisterLine(lastPosition, ball.transform.position));

			if ( stopsim ) break;
		}
		
		simming = false;

		GameObject.Destroy(ball.gameObject);
	}
	
}

[System.Serializable]
public class Pitch : BallForce 
{
	public TextMeshProUGUI TimingText;

	public enum StrikeType 
	{
		Either, Left, Right
	}

	public StrikeTime[] strikeTimes;

	[System.Serializable]
	public class StrikeTime 
	{
		public FloatRange time;

		public StrikeType onInputType;

		public HitData.HitType hitType;

		public string TimingText;

		public string direction;
	}

	float pitchStartTime = 0f;

	public bool hitsWicket;

	[DisplayIf("hitsWicket",true)]
	public float hitsWicketAtTime = 1f;

	public override void Execute (GameObject ball) 
	{
		base.Execute (ball);

		pitchStartTime = Time.time;
	}

	public HitData.HitType AttemptHit(StrikeType strike)
	{
		float checkTime = Time.time - pitchStartTime;

		foreach(var time in strikeTimes) 
		{
			if ( time.onInputType != strike && time.onInputType != StrikeType.Either )
				continue;

			if (checkTime.BetweenInclusive(time.time.min, time.time.max))
			{
				if ((time.direction.Equals(BallForce.instance.direction)) || (BallForce.instance.direction.Equals("out")))
				{
					Debug.Log("Striking To Direction = " + time.direction + " With Ball Having Direction " + BallForce.instance.direction);

					if (TimingText != null)
					{
						TimingText.gameObject.SetActive(true);
						TimingText.text = time.TimingText;
					}
					return time.hitType;
				}
				else
				{
					if (TimingText != null)
					{
						TimingText.gameObject.SetActive(true);
						TimingText.text = "Missed";
					}

					return HitData.HitType.NONE;
				}
			}
		}

		return HitData.HitType.NONE;
	}


}

