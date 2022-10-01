#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif
using UnityEngine;
using System.Collections;

// Not working? Are you using a wrong-direction-facing plane collider instead of a box collider?
public class GameMenuItem : MonoBehaviour {
	public GameMenuItem
		itemLeft,
		itemAbove,
		itemBelow,
		itemRight,
		itemWhenActivated;

	public GameMenu
		menuWhenActivated;

//	public bool changeColors = true;

	[DisplayIf("changeColors",true)]
	public Renderer coloredObject;

//	bool isDown = false;

	[System.Serializable]
	public class Message {
		public string name = "";
		public enum Type {
			None, String, Int, Float, Bool, Object
		}
		public Type type;

		[DisplayIf("type","String",true)]
		public string sarg = "";
		[DisplayIf("type","Int",true)]
		public int iarg = 0;
		[DisplayIf("type","Float",true)]
		public float farg = 0f;
		[DisplayIf("type","Bool",true)]
		public bool barg = false;
		[DisplayIf("type","Object",true)]
		public Object oarg = null;

		public void SendFrom(MonoBehaviour mb) {
			switch(type) {
				case Type.Bool:
					mb.SendMessageUpwards(name,barg);
					break;
				case Type.Float:
					mb.SendMessageUpwards(name,farg);
					break;
				case Type.Int:
					mb.SendMessageUpwards(name,iarg);
					break;
				case Type.None:
					mb.SendMessageUpwards(name);
					break;
				case Type.String:
					mb.SendMessageUpwards(name,sarg);
					break;
				case Type.Object:
					mb.SendMessageUpwards(name,oarg);
					break;
			}
		}
	}

	public Message[] messages = new Message[0];


	public ControlLink hotControl = new ControlLink();

	GameMenu myMenu;

	protected virtual void Start() {
		myMenu = this.GetComponentInParents<GameMenu>();
		if ( null == myMenu ) {
			Debug.Log(name + " menuitem has no menu parent");
			Destroy(this);
			return;
		}
		if ( null == coloredObject ) coloredObject = GetComponentInChildren<Renderer>();
		Collider c = GetComponentInChildren<Collider>();
		if ( null == c ) {
			Debug.LogError(name + " must have a collider child to be a menu item!");
		} else {
			MouseMessageRelay mmr = c.GetOrAddComponent<MouseMessageRelay>();
			mmr.OnDown += HandleOnDown;
			mmr.OnUp += HandleOnUp;
			mmr.OnUpAsButton += HandleOnUpAsButton;
			mmr.OnEnter += HandleOnEnter;
			mmr.OnExit += HandleOnExit;
		}
	}

	protected virtual void Update() {
		if ( hotControl.set != "" ) {
			if ( hotControl.axisEdge == 0 ) {
				if ( CustomInput.GetKeyDown(hotControl) ) {
					HandleOnUpAsButton(null);
				}
			} else {
				if ( CustomInput.GetAxisEdge(hotControl) == hotControl.axisEdge ) {
					HandleOnUpAsButton(null);
				}
			}
		}
	}

	void HandleOnEnter (MouseMessageRelay relayer) {
		myMenu.currentItem = this;
	}

	void HandleOnUpAsButton (MouseMessageRelay relayer) {
//		isDown = false;
		foreach(Message message in messages) {
			message.SendFrom(this);
		}
		if ( menuWhenActivated ) {
			GameMenu.currentGameMenu = menuWhenActivated;
			if ( itemWhenActivated ) GameMenu.currentGameMenu.currentItem = itemWhenActivated;
		} else {
			if ( itemWhenActivated ) myMenu.currentItem = itemWhenActivated;
		}
	}

	void HandleOnUp (MouseMessageRelay relayer) {
//		isDown = false;
	}

	void HandleOnExit (MouseMessageRelay relayer) {
//		isDown = false;
	}
	
	void HandleOnDown (MouseMessageRelay relayer) {
//		isDown = true;
		myMenu.currentItem = this;
	}

#if UNITY_EDITOR
	public bool showText = true;

	protected virtual void OnDrawGizmos() {
		if ( !showText ) return;
		if ( !myMenu ) myMenu = this.GetComponentInParents<GameMenu>();
		if ( !myMenu ) return;
		//BindingFlags search = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		Vector3 offset = Vector3.zero;
		GUIUtils.ColorWheelResolution = 10;
		Transform cam = Camera.current.transform;
		float distance = (cam.position - transform.position).magnitude * 0.05f;
		if ( Vector3.Angle(cam.forward, transform.position - cam.position ) > 90f ) return;

		Vector3 perEach = distance * Vector3.up;

		foreach(Message m in messages) {
//			MethodInfo mi = myMenu.GetType().GetMethod(m.name, search);
//			if ( null != mi ) {
//				Gizmos.color = Handles.color = GUIUtils.ColorWheelElement(m.name.GetHashCode());
//				Gizmos.DrawLine(transform.position + offset, myMenu.transform.position);
//				Handles.Label(transform.position+offset, m.name, GUIUtils.StyleLeftButton);
//			} else {
				Handles.Label(transform.position+offset,m.name, GUIUtils.StyleLeftButton);
//			}
			offset += perEach;
		}
	}
#endif
}

