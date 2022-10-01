#if UNITY_EDITOR
#else
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
#define TOUCH_REQUIRED
#endif
#endif
using UnityEngine;
using System.Collections;

public class TouchProjector : MonoBehaviour {
#if TOUCH_REQUIRED
	static int touchBuffer = 20;

	MouseMessageRelay[] lastRelays;
	MouseMessageRelay[] listeners;
	bool[] buttonTracker;
	LayerMask castMask;

	public void AddRelay(MouseMessageRelay mmr) {
		listeners = listeners.With(mmr);
	}

	public void RemoveRelay(MouseMessageRelay mmr) {
		listeners = listeners.Without(mmr);
	}

	Camera myCam;

	void Awake() {
		listeners = new MouseMessageRelay[0];
		lastRelays = new MouseMessageRelay[touchBuffer];
		buttonTracker = new bool[touchBuffer];
		myCam = GetComponent<Camera>();
		castMask = myCam.cullingMask;
	}

	void Update() {
		RaycastHit rch;

		for (int ix=0; ix<Input.touches.Length; ix++) {
			Touch t = Input.touches[ix];

			if ( Physics.Raycast(myCam.ScreenPointToRay(t.position), out rch, Mathf.Infinity, castMask) ) {
				MouseMessageRelay mmr = rch.collider.GetComponent<MouseMessageRelay>();
				if ( mmr ) {
					mmr.touchIndex = ix;
					MouseMessageRelay lastMMR = lastRelays[ix];
					lastRelays[ix] = mmr;
					switch( t.phase ) {
						case TouchPhase.Began:
							mmr.OnTouchDown();
							mmr.OnTouchOver();
							mmr.OnTouchEnter();
							buttonTracker[ix] = true;
							break;
						case TouchPhase.Canceled:
						case TouchPhase.Ended:
							if ( lastMMR ) {
								lastMMR.OnTouchUp();
								if ( buttonTracker[ix] ) lastMMR.OnTouchUpAsButton();
								lastMMR.OnTouchExit();	
							}
							lastRelays[ix] = null;
							break;
						case TouchPhase.Moved:
							
							mmr.OnTouchOver();
							mmr.OnTouchDrag();
							if ( lastMMR!=lastRelays[ix] ) {
								mmr.OnTouchEnter();
								buttonTracker[ix] = false;
								if ( lastMMR ) {
									lastMMR.OnTouchExit();
								}
							}
							break;
						case TouchPhase.Stationary:
							mmr.OnTouchOver();
							break;
					}
				} else {
					buttonTracker[ix] = false;
					if ( lastRelays[ix] ) {
						lastRelays[ix].OnTouchExit();
						lastRelays[ix] = null;
					}
				}
			} else {
				buttonTracker[ix] = false;
				if ( lastRelays[ix] ) {
					lastRelays[ix].OnTouchExit();
					lastRelays[ix] = null;
				}
			}
		}
	}
#endif
}

