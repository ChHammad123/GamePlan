using UnityEngine;
using System.Collections;

public class LoadScene : MonoBehaviour {
	// default Update so things can run their awake/start nonsense
	public enum Timestyle {
		Update, Start, Awake, LateUpdate
	}
	public Timestyle when;
	public string loadName = "";
	public int loadIndex = -1;
	public bool asynchronous;

	void Load() {
		Destroy(this);
		if ( loadIndex > -1 ) {
			if ( asynchronous ) {
				Application.LoadLevelAsync(loadIndex);
			} else {
				Application.LoadLevel(loadIndex);
			}
		} else {
			if ( asynchronous ) {
				Application.LoadLevelAsync(loadName);
			} else {
				Application.LoadLevel(loadName);
			}
		}
	}

	void Start() { if ( when == Timestyle.Start ) Load(); }
	void Update() { if ( when == Timestyle.Update ) Load(); }
	void Awake() { if ( when == Timestyle.Awake ) Load(); }
	void LateUpdate() { if ( when == Timestyle.LateUpdate ) Load(); }
}

