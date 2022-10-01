#if UNITY_EDITOR
#else
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
#define TOUCH_REQUIRED
#endif
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ControlLink {
	public ControlLink() { }
	public ControlLink(string set, string control) { 
		this.set = set; this.control = control;
	}

	[ChooseStringFromList("GetSets",true)]
	public string set = "";
	[ChooseStringFromList("GetControls",true)]
	public string control = "";

	public string[] GetSets {
		get {
			CustomInput instance = (CustomInput)GameObject.FindObjectOfType(typeof(CustomInput));
			if ( !instance ) return new string[]{};
			string[] ret = new string[0];
			foreach(var x in instance.controlSets) ret = ret.With(x.name);
			return ret;
		}
	}
	public string[] GetControls {
		get {
			if ( set == "" ) return new string[]{};
			CustomInput instance = (CustomInput)GameObject.FindObjectOfType(typeof(CustomInput));
			if ( !instance ) return new string[]{};
			string[] ret = new string[0];
			foreach(var x in instance.controlSets) {
				if ( x.name == set ) {
					foreach(var c in x.controls) {
						ret = ret.With(c.name);
					}
				}
			}
			return ret;
		}
	}

	[Tooltip("Only used in Game Menu Items. Determines which direction of an axis will trigger the Game Menu Item.")]
	public int axisEdge = 0;
}

public class CustomInput : MonoBehaviour {

	/// <summary>Disable all or part of a control.</summary>
	/// <param name="setName">Set name.</param>
	/// <param name="controlName">Control name.</param>
	/// <param name="axisDirection">Axis direction. If zero, disable entire control. -1 disables negative axis input, +1 disables positive axis input and Keys controls.</param>
	public static void DisableControl(string setName, string controlName, int axisDirection = 0) {
		Control c = control[setName][controlName];
		if ( axisDirection == 0 ) c.SetActive(false);
		else c.DisableDirection(axisDirection);
	}

	/// <summary>Completely re-enable a control.</summary>
	/// <param name="setName">Set name.</param>
	/// <param name="controlName">Control name.</param>
	public static void EnableControl(string setName, string controlName) {
		Control c = control[setName][controlName];
		c.SetActive(true);
		c.DisableDirection(0);
	}

	public static Control GetControl(string setName, string controlName) {
		return control[setName][controlName];
	}

	[System.Serializable]
	public class Control : InspectorHelper {
		public bool nameLocked = false;

		public enum AxisType {
			Axis, Keys
		}
		public AxisType type;
		public Translatable visName, description;
		public string[] axes = new string[0];
		public KeyCode[] positiveKeys = new KeyCode[0];
		public KeyCode[] negativeKeys = new KeyCode[0];
		public float gravity = 2f;
		public float acceleration = 4f;
		public float axisSensitivity = 1f;
		public float deadZone = 0f;
		public bool inverted;

		bool active = true;
		int disabledDirection = 0;

		int
			axisFireFrame;
		bool
			axisReset = true;
		float 
			lastAxisTime,
			lastAxisValue;

		/// <summary>Disables the entire control.</summary>
		public void SetActive(bool active) { this.active = active; }

		/// <summary>Disables one direction of an axis (positive/negative). Set to zero to re-enable all directions.</summary>
		public void DisableDirection(int active) { this.disabledDirection = active; }

		public float GetAxis() {
			if ( !active ) return 0f;
			float value = GetAxisRaw();
			if ( 0f == lastAxisTime ) lastAxisTime = Time.realtimeSinceStartup;
			if ( Mathf.Abs(value) > Mathf.Abs(lastAxisValue) ) {
				value = Mathf.MoveTowards(lastAxisValue, value, (Time.realtimeSinceStartup - lastAxisTime) * acceleration);
			} else {
				value = Mathf.MoveTowards(lastAxisValue, value, (Time.realtimeSinceStartup - lastAxisTime) * gravity);
			}
			lastAxisTime = Time.realtimeSinceStartup;
			lastAxisValue = value;
			if ( Mathf.Abs(value) <= deadZone ) return 0f;
			if ( Mathf.Sign(value) == disabledDirection ) return 0f;
			return value;
		}
		public float GetAxisEdge() {
			if ( !active ) return 0f;
			float value = GetAxisRaw();
			bool isTriggerFrame = Time.frameCount == axisFireFrame;
			if ( (isTriggerFrame || axisReset ) && value != 0f ) {
				if ( !isTriggerFrame ) {
					axisFireFrame = Time.frameCount;
					axisReset = false;
				}
				if ( Mathf.Sign(value) == disabledDirection ) return 0f;
				return Mathf.Sign(value);
			}
			return 0f;
		}
		public float GetAxisRaw() {
			if ( !active ) return 0f;
			float value = 0f;
			for(int i=0; i < positiveKeys.Length; i++) if ( Input.GetKey(positiveKeys[i]) ) { value = 1f; break; }
			for(int i=0; i < negativeKeys.Length; i++) if ( Input.GetKey(negativeKeys[i]) ) { value = -1f; break; }
			float total = 0f;
			if ( type == AxisType.Axis ) {
				for(int i=0; i < axes.Length; i++) {
					total = Input.GetAxisRaw(axes[i]);
					if ( total != 0f ) value = total;
				}
			}
			if ( inverted ) value *= -1f;
			if ( value == 0 ) {
				axisReset = true;
			}
			if ( Mathf.Sign(value) == disabledDirection ) return 0f;
			return value;
		}
		public bool Any() {
			if ( !active || disabledDirection == 1 ) return false;
			for(int i=0; i < positiveKeys.Length; i++) {
				if ( Input.GetKey(positiveKeys[i]) ) return true;
			}
			return false;
		}
		public bool All() {
			if ( !active || disabledDirection == 1 ) return false;
			for(int i=0; i < positiveKeys.Length; i++) {
				if ( !Input.GetKey(positiveKeys[i]) ) return false;
			}
			return true;
		}
		public int[] Which() {
			int[] list = new int[0];
			if ( !active || disabledDirection == 1 ) return list;
			for(int i=0; i < positiveKeys.Length; i++) {
				if ( Input.GetKey(positiveKeys[i]) ) list = list.With(i);
			}
			return list;
		}
		public int[] WhichDown() {
			int[] list = new int[0];
			if ( !active || disabledDirection == 1 ) return list;
			for(int i=0; i < positiveKeys.Length; i++) {
				if ( Input.GetKeyDown(positiveKeys[i]) ) list = list.With(i);
			}
			return list;
		}
		public int[] WhichUp() {
			int[] list = new int[0];
			if ( !active || disabledDirection == 1 ) return list;
			for(int i=0; i < positiveKeys.Length; i++) {
				if ( Input.GetKeyUp(positiveKeys[i]) ) list = list.With(i);
			}
			return list;
		}
		public bool AnyUp() {
			if ( !active || disabledDirection == 1 ) return false;
			for(int i=0; i < positiveKeys.Length; i++) {
				if ( Input.GetKeyUp(positiveKeys[i]) ) return true;
			}
			return false;
		}
		public bool AnyDown() {
			if ( !active || disabledDirection == 1 ) return false;
			for(int i=0; i < positiveKeys.Length; i++) {
				if ( Input.GetKeyDown(positiveKeys[i]) ) return true;
			}
			return false;
		}
	}
	
	[System.Serializable]
	public class Set : InspectorHelper {
		public bool nameLocked = false;
		public Control[] controls = new Control[1];
		Dictionary<string, Control> dictionary;

		public Control this[string index] {
			get {
				return dictionary[index];
			}
		}
		public void SetUpDictionary() {
			dictionary = new Dictionary<string, Control>();
			foreach(Control control in controls) {
				try {
					dictionary.Add(control.name, control);
				} catch ( System.Exception e ) {
					Debug.Log(control.name + " not added; " + e.Message);
				}
			}
		}

	}
	public Set[] controlSets = new Set[1];
	public string[] knownAxes = new string[0];

	static CustomInput single = null;
	public static Dictionary<string, Set> control = new Dictionary<string,Set>();

	void Start() {
		if ( null != single ) {
			Debug.Log(name + " is a CustomInput but " + single.name + " is already the CustomInput");
			Destroy(gameObject);
			return;
		}
		single = this;
		foreach(Set cc in controlSets) {
			try {
				cc.SetUpDictionary();
				control.Add(cc.name, cc);
			} catch ( System.Exception e ) {
				Debug.LogWarning(cc.name + " control not added ("+e.Message+")");
			}
		}
		DontDestroyOnLoad(gameObject);
	}
	public static void SetControlSetActive(string set, bool active) {
		foreach(Control c in control[set].controls) {
			c.SetActive(active);
		}
	}

	public static float GetAxis			(ControlLink link) {return GetAxis(link.set,link.control);}
	public static float GetAxis			(string set, string axis) {return control[set][axis].GetAxis();}
	public static float GetAxisRaw		(ControlLink link) {return GetAxisRaw(link.set,link.control);}
	public static float GetAxisRaw		(string set, string axis) {return control[set][axis].GetAxisRaw();}
	public static float	GetAxisEdge		(ControlLink link) {return GetAxisEdge(link.set,link.control);}
	public static float	GetAxisEdge		(string set, string axis) {return control[set][axis].GetAxisEdge();}
	public static bool	GetKeyUp		(ControlLink link) {return GetKeyUp(link.set,link.control);}
	public static bool 	GetKeyUp		(string set, string keys) {return control[set][keys].AnyUp();}
	public static bool	GetKeyDown		(ControlLink link) {return GetKeyDown(link.set,link.control);}
	public static bool 	GetKeyDown		(string set, string keys) {return control[set][keys].AnyDown();}
	public static bool	GetKey			(ControlLink link) {return GetKey(link.set,link.control);}
	public static bool 	GetKey			(string set, string keys) {return control[set][keys].Any();}
	public static int[] WhichKeys		(ControlLink link) {return WhichKeys(link.set,link.control);}
	public static int[] WhichKeys		(string set, string keys) {return control[set][keys].Which();}
	public static int[] WhichKeysDown	(ControlLink link) {return WhichKeysDown(link.set,link.control);}
	public static int[] WhichKeysDown	(string set, string keys) {return control[set][keys].WhichDown();}
	public static int[] WhichKeysUp		(ControlLink link) {return WhichKeysUp(link.set,link.control);}
	public static int[] WhichKeysUp		(string set, string keys) {return control[set][keys].WhichUp();}
	public static bool	AllKeys			(ControlLink link) {return AllKeys(link.set,link.control);}
	public static bool 	AllKeys			(string set, string keys) {return control[set][keys].All();}

	public static bool Touching(int index = 0) {
#if TOUCH_REQUIRED
		return Input.touches.Length > index && 
			(Input.GetTouch(index).phase == TouchPhase.Began ||
			 Input.GetTouch(index).phase == TouchPhase.Moved ||
			 Input.GetTouch(index).phase == TouchPhase.Stationary);
#else
		return Input.GetKey((KeyCode)((int)KeyCode.Mouse0 + index));
#endif
	}

	public static bool Touched(int index = 0) {
#if TOUCH_REQUIRED
		return Input.touches.Length > index && Input.GetTouch(index).phase == TouchPhase.Began;
#else
		return Input.GetKeyDown((KeyCode)((int)KeyCode.Mouse0 + index));
#endif
	}

	public static Vector3 TouchPosition(int index = 0) {
#if TOUCH_REQUIRED
		if ( Input.touches.Length > index ) return Input.GetTouch(index).position;
		return Vector3.zero;
#else
		return Input.mousePosition;
#endif
	}


}

