using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SceneComment : MonoBehaviour {
	// this class does nothing outside the editor
#if UNITY_EDITOR
	public string comment = "";
	public string permanentNotice = "";
	public RefV3[] indicators = new RefV3[0];
	public Color32 color = new Color32(255,255,255,255);
	public Color32 noticeColor = new Color32(255,0,0,255);
	public bool overrideTextColor = false;
	public Color32 overrideColor = new Color32(0,0,0,255);

	[System.Serializable]
	public class RefV3 {
		public float x=0f, y=0f, z=0f;
		public RefV3() { }
		public RefV3(float a, float b, float c) { x=a;y=b;z=c; }
		public static implicit operator Vector3(RefV3 rv3) { return new Vector3(rv3.x,rv3.y,rv3.z); }
		public static implicit operator RefV3(Vector3 v3) { 
			return new RefV3(v3.x,v3.y,v3.z);
		}
	}
	[System.NonSerialized] public static GUIStyle style=null;
	void OnGUI() {
		if ( null == style ) {
			style = new GUIStyle(GUI.skin.box);
			Texture2D tex = new Texture2D(1,1); // this leaks one texture
			tex.SetPixel(0,0,new Color32(255,255,255,255));
			style.normal.background = tex;
			style.normal.textColor = new Color32(0,0,0,255);
			style.alignment = TextAnchor.UpperLeft;
		}
	}
	void OnDrawGizmos() {
		Gizmos.color = color;
		foreach(Vector3 indicator in indicators) {
			Gizmos.DrawLine(transform.position, indicator);
		}
		if ( null != style ) {
			Vector3 pos = Camera.current.WorldToScreenPoint(transform.position);
			bool show = false;
			if ( gameObject == Selection.activeGameObject ) {
				show = true;
			} else {
				Vector2 screenPoint = new Vector2(pos.x, Screen.height-pos.y);
				Vector2 mousePoint = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);
				float dif = (mousePoint - screenPoint).magnitude;
				if ( dif < 60f ) {
					show = true;
				}
			}
			if ( show ) {
				if ( null == style.normal.background ) style.normal.background = new Texture2D(1,1);
				style.normal.background.SetPixel(0,0,color);
				style.normal.background.Apply();
				if ( overrideTextColor ) style.normal.textColor = overrideColor;
				else style.normal.textColor = color.HighContrast();
				GUIUtils.LabelHandle(transform.position,"    "+comment, style);
			} else {
				if ( "" != permanentNotice ) {
					if ( null == style.normal.background ) style.normal.background = new Texture2D(1,1);
					style.normal.background.SetPixel(0,0,noticeColor);
					style.normal.background.Apply();
					if ( overrideTextColor ) style.normal.textColor = overrideColor;
					else style.normal.textColor = color.HighContrast();
					GUIUtils.LabelHandle(transform.position,"    "+permanentNotice,style);
				}
			}
		}
		Gizmos.DrawIcon(transform.position, "Arakion/Comment.png");
		SceneView.RepaintAll();
	}
#endif
}

