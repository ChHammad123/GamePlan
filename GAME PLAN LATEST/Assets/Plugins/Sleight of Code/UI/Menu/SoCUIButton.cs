using UnityEngine;
using System.Collections;

public class SoCUIButton : MonoBehaviour {
	public Vector2[] uvsUp, uvsDown;
	public Rect ninesliceUp, ninesliceDown;

	public bool auto = true;

	public delegate void Callback(SoCUIButton button);

	public event Callback
		onDown,
		onUp,
		onHold,
		onUpAsButton;

	bool _down = false;
	bool Down {
		get { return _down; }
		set {
			if ( value == _down ) return;
			if ( _down && null != onUp ) onUp(this);
			if ( !_down && null != onDown ) onDown(this);
			_down = value;
		}
	}

	UVQuad quad;
	NinesliceMeshRenderer nmr;

	public void SetUp() {
		if ( quad) quad.SetUVS(uvsUp);
		if ( nmr ) { nmr.uvRegion = ninesliceUp; nmr.refresh = true; }
	}
	public void SetDown() {
		if ( quad) quad.SetUVS(uvsDown);
		if ( nmr ) { nmr.uvRegion = ninesliceDown; nmr.refresh = true; }
	}

	void Start() {
		quad = GetComponent<UVQuad>();
		nmr = GetComponent<NinesliceMeshRenderer>();
		if ( GetComponent<Collider>() ) {
			MouseMessageRelay mmr = this.GetOrAddComponent<MouseMessageRelay>();
			mmr.OnDown += OnDown;
			mmr.OnExit += OnExit;
			mmr.OnUp += OnUp;
			mmr.OnUpAsButton += OnUpAsButton;
		}
		if ( auto ) SetUp();
	}

	void Update() {
		if ( Down && null != onHold ) onHold(this);
	}

	void OnUpAsButton (MouseMessageRelay relayer) {
		if ( auto ) SetUp();
		if ( null != onUpAsButton ) onUpAsButton(this);
		Down = false;
	}

	void OnDown(MouseMessageRelay mmr) {
		if ( auto ) SetDown();
		Down = true;
	}
	void OnExit(MouseMessageRelay mmr) {
		if ( auto ) SetUp();
		Down = false;
	}
	void OnUp(MouseMessageRelay mmr) {
		if ( auto ) SetUp();
		Down = false;
	}

}

