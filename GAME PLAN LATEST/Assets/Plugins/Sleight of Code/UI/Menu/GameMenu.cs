using UnityEngine;
using System.Collections;

public class GameMenu : SOCBehaviour {
	static GameMenu _current;
	public static GameMenu currentGameMenu {
		get { return _current; }
		set {
			if ( _current == value ) return;
			if ( null != _current && _current ) _current.OnBecameInactive();
			_current = value;
			if ( null != _current ) _current.OnBecameActive();
		}
	}

	public GameMenuItem currentItem;

	protected virtual void OnBecameActive() {
	}
	protected virtual void OnBecameInactive() {
	}
}

