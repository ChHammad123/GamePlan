using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BatterPanel : SOCBehaviour {
	public Text tName, tOutReason, tRuns, tBalls, tSR, tFours, tSixes, tFOW;
	public Image colorThisToShowOutStatus;
	public GameObject enableToHighlight;

	private int iRuns, iBalls, iFours, iSixes;

	public void Reset() {
		FallOfWicket = Runs = Balls = Fours = Sixes = 0;
		tSR.text = "0";
		tOutReason.text = "";
	}

	public int Fours { get { return iFours; } set { iFours = value; tFours.text = value.ToString(); } }
	public int Sixes { get { return iSixes; } set { iSixes = value; tSixes.text = value.ToString(); } }

	public int FallOfWicket {
		set { tFOW.text = value.ToString(); }
	}

	public int Balls {
		get { return iBalls; }
		set {
			iBalls = value;
			tBalls.text = "(" + iBalls.ToString() + ")";
			if ( iBalls > 0 ) {
				tSR.text = ((float)iRuns / (float)iBalls * 100f).ToString("N");
			}
		}
	}

	public int Runs {
		get { return iRuns; }
		set {
			iRuns = value;
			tRuns.text = iRuns.ToString();
			if ( iBalls > 0 ) {
				tSR.text = ((float)iRuns / (float)iBalls * 100f).ToString("N");
			}
		}
	}
}

