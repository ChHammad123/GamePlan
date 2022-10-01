#if UNITY_EDITOR
#else
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
#define TOUCH_REQUIRED
#endif
#endif

using UnityEngine;

public class MouseMessageRelay : MonoBehaviour {
	public delegate void Relay(MouseMessageRelay relayer);
	public event Relay
		OnDown, OnUp, OnDrag, OnExit, OnEnter, OnOver, OnUpAsButton;

	public bool allowMultitouch = false;
	public int touchIndex;

#if TOUCH_REQUIRED
	void Start() {
		foreach(Camera cam in GameObject.FindObjectsOfType<Camera>()) {
			cam.GetOrAddComponent<TouchProjector>();
		}
	}
	public void OnTouchDown() { CallIfNotNull(OnDown); }
	public void OnTouchUp() { CallIfNotNull(OnUp); }
	public void OnTouchDrag() { CallIfNotNull(OnDrag); }
	public void OnTouchExit() { CallIfNotNull(OnExit); }
	public void OnTouchEnter() { CallIfNotNull(OnEnter); }
	public void OnTouchOver() { CallIfNotNull(OnOver); }
	public void OnTouchUpAsButton() { CallIfNotNull(OnUpAsButton);}
#else
	void OnMouseDown() { CallIfNotNull(OnDown); }
	void OnMouseUp() { CallIfNotNull(OnUp); }
	void OnMouseDrag() { CallIfNotNull(OnDrag); }
	void OnMouseExit() { CallIfNotNull(OnExit); }
	void OnMouseEnter() { CallIfNotNull(OnEnter); }
	void OnMouseOver() { CallIfNotNull(OnOver); }
	void OnMouseUpAsButton() { CallIfNotNull(OnUpAsButton); }
#endif

	void CallIfNotNull(Relay relay) {
		if ( null != relay ) relay(this);
	}

#if UNITY_EDITOR
#endif
}

