/*
* (C) 2013 Andrew Milne
* 
* GUIUtils.cs
* 
*** Lots of utilities for writing custom Inspector scripts with minimal headache.
* 
* Use:
* 
* import GUIUtils.cs; // JS/US
* using GUIUtils.cs; // C#
* 
* 
* Stringpopup commands:
* char 9 is right-justify?
* char 47 is a separator in popups
* {item} 47 {sub} creates a submenu!! clicking results in item/sub return
* 
/**/

// TODO: See PercentBar for how to find "current control id". May make Begin obsolete!
// Also see this:
/*
public static FieldInfo LastControlIdField=typeof(EditorGUI).GetField("lastControlID", BindingFlags.Static|BindingFlags.NonPublic);
        public static int GetLastControlId(){
            if(LastControlIdField==null){
                Debug.LogError("Compatibility with Unity broke: can't find lastControlId field in EditorGUI");
                return 0;
            }
            return (int)LastControlIdField.GetValue(null);
        }

 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public static class GUIUtils {
	public static bool allowFolderBuilders = false;
	
	static GUIUtils() {
		gcEditValueByHand = new GUIContent("","Toggle hand-editing");
		gcRefresh = new GUIContent(Icon("GUIURefresh"),"Refresh options");
		gcCopy = new GUIContent(Icon("GUIUToBin"),"Copy");
		gcPaste = new GUIContent(Icon("GUIUFromBin"),"Paste");
		gcDownArrow = new GUIContent(Icon("GUIUDownTriangle"),"Move Down");
		gcUpArrow = new GUIContent(Icon("GUIUUpTriangle"),"Move Up");
		gcDuplicate = new GUIContent(Icon("GUIUDuplicate"),"Duplicate");
		gcInsert = new GUIContent(Icon("GUIUInsertNew"),"Insert new before");
		gcRemove = new GUIContent(Icon("GUIURemove"),"Remove");
		colorWheels = new Dictionary<ColorWheelData, Color[]>();
		SetColorWheelData(new ColorWheelData(8,0));
	} 
	
	private class ColorWheelData {
		public int resolution;
		public int desaturation;
		
		public ColorWheelData(int res, int desat) {
			resolution = res;
			desaturation = desat;
		}
		
		public override bool Equals(object obj) {
			ColorWheelData cwd = obj as ColorWheelData;
			return cwd.resolution == resolution && cwd.desaturation == desaturation;
		}
		public override int GetHashCode() {
			return resolution.GetHashCode() ^ desaturation.GetHashCode();
		}
	}
	
	private static int currentControl;
	
	#region ColorWheel
	static Dictionary<ColorWheelData, Color[]> colorWheels;
	static ColorWheelData currentColorWheelResolution;
	
	public static Color ColorWheelElement(int index) {
		return colorWheels[currentColorWheelResolution].IndexModLength(index);
	}
	
	public static void DisposeColorWheel() {
		colorWheels.Remove(currentColorWheelResolution);
		SetColorWheelData(new ColorWheelData(8,0));
	}
	
	static void SetColorWheelData(ColorWheelData cwd) {
		int reso = cwd.resolution;
		currentColorWheelResolution = cwd;
		if ( !colorWheels.ContainsKey(cwd) ) {
			float r = 1f, g = 0f, b = 0f;
			Color[] newWheel = new Color[reso];
			
			// total value change:
			// r1-0/g0-1, g1-0/b0-1, b1-0/r0-1
			// 3f
			float changePerCount = 3f / (float)reso;
			float cc, rem;
			for(int i=0; i < reso; i++) {
				while(r > 0f) {
					cc = Mathf.Min(r, changePerCount);
					r-= cc;
					g+= cc;
					rem = changePerCount-cc;
					if ( rem > 0f ) {
						g-=rem; b+=rem;
					}
					newWheel[i] = new Color(r,g,b).Desaturated(cwd.desaturation);
					i++;
				}
				while(g > 0f) {
					cc = Mathf.Min(g, changePerCount);
					g-= cc;
					b+= cc;
					rem = changePerCount-cc;
					if ( rem > 0f ) {
						b-=rem; r+=rem;
					}
					newWheel[i] = new Color(r,g,b).Desaturated(cwd.desaturation);
					i++;
				}
				while(b > 0f && i < reso) {
					cc = Mathf.Min(r, changePerCount);
					b-= cc;
					r+= cc;
					newWheel[i] = new Color(r,g,b).Desaturated(cwd.desaturation);
					i++;
				}
			}
			
			colorWheels.Add(cwd, newWheel);
		}
		
	}
	
	public static int ColorWheelDesaturation {
		get { return currentColorWheelResolution.desaturation; }
		set {
			if ( value < 0 ) {
				Debug.LogError("Can't set color wheel desaturation below 0.");
				return;
			}
			SetColorWheelData(new ColorWheelData(currentColorWheelResolution.resolution, value));
		}
	}
	
	public static int ColorWheelResolution {
		get { return currentColorWheelResolution.resolution; }
		set {
			if ( value < 3 ) {
				Debug.LogError("Can't set color wheel resolution below 3.");
				return;
			}
			SetColorWheelData(new ColorWheelData(value, currentColorWheelResolution.desaturation));
		}
	}
	
	#endregion
	
	#region Gizmos/Handles
	public static class Gizmos {
		public static void DrawArrowRatio(Vector3 from, Vector3 to, float capRatio = 0.2f, float capAngle = 15f, int capCorners = 15) {
			DrawArrow(from, to, (from-to).magnitude * capRatio, capAngle, capCorners);
		}
		public static void DrawArrow(Vector3 from, Vector3 to, float capLength = 0.2f, float capAngle = 15f, int capCorners = 15) {
			GUIUtils.DrawArrow(from, to, UnityEngine.Gizmos.DrawLine, capLength, capAngle, capCorners);
		}
	}
	public static void DrawArrow(Vector3 from, Vector3 to, TwoVector3Args func, float capLength=0.2f, float capAngle=15f, int capCorners=3) {
		Vector3 capLine = (to-from) * capLength;
		Vector3 getNormalFrom = ((from-to)==Vector3.up)?Vector3.right:Vector3.up;
		Vector3 norm = Vector3.Cross(capLine, getNormalFrom);
		capLine = Quaternion.AngleAxis(180f-capAngle,norm) * capLine;
		func(from, to);
		if ( capCorners < 2 ) capCorners = 2;
		Quaternion angle = Quaternion.AngleAxis(360f / (float)capCorners, to-from);
		for(int corner=0; corner < capCorners; corner++) {
			func(to,to+capLine);
			capLine = angle * capLine;
		}
	}
	#if UNITY_EDITOR
	public static class Handles {
		public static void DrawArrowRatio(Vector3 from, Vector3 to, float capRatio = 0.2f, float capAngle = 15f, int capCorners = 15) {
			DrawArrow(from, to, (from-to).magnitude * capRatio, capAngle, capCorners);
		}
		public static void DrawArrow(Vector3 from, Vector3 to, float capLength = 0.2f, float capAngle = 15f, int capCorners = 15) {
			GUIUtils.DrawArrow(from, to, UnityEditor.Handles.DrawLine, capLength, capAngle, capCorners);
		}
	}
	#endif
	#endregion
	
	#region Using-able layouters
	public class LabelWidth : IDisposable {
		int oldfix;
		public LabelWidth(string sizer, bool onlyExpand = false) {
			#if UNITY_EDITOR
			if ( _minimumFixedWidthLabelSize==0f ) _minimumFixedWidthLabelSize = (int)EditorGUIUtility.labelWidth;
			#endif
			
			oldfix = _minimumFixedWidthLabelSize;
			if ( onlyExpand ) {
				_minimumFixedWidthLabelSize = (int)Mathf.Max(_minimumFixedWidthLabelSize,GUI.skin.label.CalcSize(new GUIContent(sizer)).x);
			} else {
				_minimumFixedWidthLabelSize = (int)GUI.skin.label.CalcSize(new GUIContent(sizer)).x;
			}
			#if UNITY_EDITOR
			EditorGUIUtility.labelWidth = _minimumFixedWidthLabelSize;
			#endif
		}
		public void Dispose() {
			_minimumFixedWidthLabelSize = oldfix;
			#if UNITY_EDITOR
			EditorGUIUtility.labelWidth = _minimumFixedWidthLabelSize;
			#endif
		}
	}
	
	public class ColoredRegion : IDisposable {
		Color oldColor;
		public static Color red = new Color(1f,0.8f,0.8f);
		public static Color yellow = new Color(1f,1f,0.6f);
		
		public ColoredRegion(bool check, Color color) {
			oldColor = GUI.color;
			if ( check ) GUI.color = color;
		}
		public ColoredRegion(bool check) {
			oldColor = GUI.color;
			if ( check ) GUI.color = red;
		}
		public void Dispose() { GUI.color = oldColor; }
	}
	public class Indent : IDisposable {
		public Indent() { GUIUtils.BeginIndent(GUIStyle.none); }
		public Indent(GUIStyle style) { GUIUtils.BeginIndent(style); }
		public Indent(string style) { 
			if ( style == "" ) GUIUtils.BeginIndent(GUIStyle.none);
			else GUIUtils.BeginIndent(style);
		}
		public void Dispose() { GUIUtils.EndIndent(); }
	}
	public class Horizontal : IDisposable {
		public Horizontal() { GUILayout.BeginHorizontal(); }
		public Horizontal(params GUILayoutOption[] options) { GUILayout.BeginHorizontal(options); }
		public Horizontal(string style, params GUILayoutOption[] options) {
			if ( style=="" ) GUILayout.BeginHorizontal(GUIStyle.none,options);
			else GUILayout.BeginHorizontal(style,options);
		}
		public Horizontal(GUIStyle style, params GUILayoutOption[] options) { GUILayout.BeginHorizontal(style,options); }
		public void Dispose() { GUILayout.EndHorizontal (); }		
	}
	public class Area : IDisposable {
		public Area(Rect r) { GUILayout.BeginArea(r); }
		public void Dispose() { GUILayout.EndArea(); }
	}
	public class Vertical : IDisposable {
		public Vertical() { GUILayout.BeginVertical(); }
		public Vertical(params GUILayoutOption[] options) { GUILayout.BeginVertical(options); }
		public Vertical(string style, params GUILayoutOption[] options) { 
			if ( style == "" ) GUILayout.BeginVertical(GUIStyle.none,options);
			else GUILayout.BeginVertical(style,options);
		}
		public Vertical(GUIStyle style, params GUILayoutOption[] options) { GUILayout.BeginVertical(style,options); }
		public void Dispose() { GUILayout.EndVertical (); }		
	}
	public class Scroll : IDisposable {
		public Scroll(ref Vector2 scroll, params GUILayoutOption[] options) { scroll=GUILayout.BeginScrollView(scroll, options); }
		public Scroll(ref Vector2 scroll, string style, params GUILayoutOption[] options ) { 
			if ( style == "" ) scroll=GUILayout.BeginScrollView(scroll,GUIStyle.none,options);
			else scroll=GUILayout.BeginScrollView(scroll,style,options);
		}
		public Scroll(ref Vector2 scroll, GUIStyle style, params GUILayoutOption[] options ) { scroll=GUILayout.BeginScrollView(scroll,style,options); }
		public Scroll(ref Vector2 scroll, GUIStyle horizontalStyle, GUIStyle verticalStyle, params GUILayoutOption[] options ) {scroll = GUILayout.BeginScrollView(scroll,horizontalStyle,verticalStyle,options); }
		public void Dispose() { GUILayout.EndScrollView(); }
	}
	
	#if UNITY_EDITOR
	public class UndoArea : IDisposable {
		private UnityEngine.Object targe;
		private string name;
		public UndoArea(UnityEngine.Object target, string s="") {
			if ( s == "" ) s = target.GetType().Name + " change";
			StartUndo(target,s);
			targe = target;
			name = s;
		}
		public void Dispose() {
			EndUndo(targe, name);
		}
	}
	#endif
	#endregion
	
	#region Good GUI Button
	// http://forum.unity3d.com/threads/96563-corrected-GUI-Button-code-(works-properly-with-layered-controls)?p=629284#post629284
	public static bool Button(Rect bounds, string label) { return Button(bounds, new GUIContent(label)); }
	public static bool Button(Rect bounds, GUIContent label) {
		GUIStyle btnStyle = GUI.skin.FindStyle("button");
		int controlID = GUIUtility.GetControlID(bounds.GetHashCode(), FocusType.Passive);
		
		bool isMouseOver = bounds.Contains(Event.current.mousePosition);
		bool isDown = GUIUtility.hotControl == controlID;
		
		if (GUIUtility.hotControl != 0 && !isDown) {
			// ignore mouse while some other control has it
			// (this is the key bit that GUI.Button appears to be missing)
			isMouseOver = false;
		}
		
		if (Event.current.type == EventType.Repaint) {
			btnStyle.Draw(bounds, label, isMouseOver, isDown, false, false);
		}
		
		switch (Event.current.GetTypeForControl(controlID)) {
		case EventType.MouseDown:
			if (isMouseOver) {  // (note: isMouseOver will be false when another control is hot)
				GUIUtility.hotControl = controlID;
			}
			break;
			
		case EventType.MouseUp:
			if (GUIUtility.hotControl == controlID) GUIUtility.hotControl = 0;
			if (isMouseOver && bounds.Contains(Event.current.mousePosition)) return true;
			break;
		}
		
		return false;
	}	
	#endregion
	
	#region Icons
	private static Dictionary<string, Texture2D> icons=null;
	public static Texture2D Icon(string name) {
		if ( null == icons ) icons = new Dictionary<string, Texture2D>();
		if ( !icons.ContainsKey(name) ) {
			icons.Add(name, Resources.Load<Texture2D>("Icons/"+name));
		}
		if ( null == icons[name] ) {
			icons[name] = Resources.Load<Texture2D>("Icons/"+name);
		}
		#if UNITY_EDITOR
		if ( null == icons[name] ) {
			icons[name] = (Texture2D)EditorGUIUtility.Load("Icons/"+name+".png");
		}
		#endif
		if ( null == icons[name] ) {
			Debug.LogError("Couldn't load icon " + name + " from a Resources folder");
			icons[name] = new Texture2D(1,1);
			icons[name].name = "%%%#";
			icons[name].SetPixel(1,1,Color.red);
			icons[name].Apply();
		}
		#if UNITY_EDITOR
		if ( icons[name].name == "%%%#" ) {
			Texture2D attempt = Resources.Load<Texture2D>("Icons/"+name);
			if ( null == attempt ) attempt = (Texture2D)EditorGUIUtility.Load("Icons/"+name+".png");
			if ( null != attempt ) {
				GameObject.DestroyImmediate(icons[name]);
				icons[name] = attempt;
			}
		}
		#endif
		return icons[name];
	}
	static GUIContent 
		gcEditValueByHand,
		gcRefresh, gcUpArrow, gcDownArrow, gcDuplicate, gcCopy, gcPaste, gcRemove, gcInsert;
	
	#endregion
	
	#region Draggable Bars
	#if UNITY_EDITOR
	private static int heldDragBar=-1;
	public class ResizableColumnsObject {
		
		public IndexNotify[] functions;
		public float[] widths;
		public float minWidth, maxWidth;
		public ResizableColumnsObject(IndexNotify[] columnFunctions, float columnMinWidth, float[] defaultWidths) {
			functions = columnFunctions;
			widths = defaultWidths;
			minWidth = columnMinWidth;
			maxWidth = Mathf.Infinity;
		}
		/// <summary>Show this instance. Returns true if a refresh is required (EditorWindows are automatically refreshed, but the value can still be queried).</summary>
		public bool Show(int from, int to) {
			bool ret = false;
			float min = minWidth, max = maxWidth;
			using(new Vertical(GUILayout.ExpandHeight(false))) {
				using(new Horizontal()) {
					for(int i=0; i < functions.Length; i++) {
						using(new Vertical(GUILayout.Width(widths[i]))) {
							for(int ix = from; (from<to?ix<=to:ix>=from); ix+=(from<to?1:-1)) {
								functions[i](ix);
							}
						}
						if ( i < functions.Length-1 ) {
							Rect drawTo = GUILayoutUtility.GetRect(6f,1f,GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false));
							if ( ResizableGridBar(drawTo, ref widths[i], min, max, false) ) {
								ret = true;
							}
						}
						min += widths[i]; max += widths[i];
					}
				}
			}
			return ret;
		}
		bool ResizableGridBar(Rect drawTo, ref float current, float min, float max, bool horizontal) {
			NextControl();
			bool ret = false;
			EditorGUIUtility.AddCursorRect(drawTo,horizontal?MouseCursor.ResizeVertical:MouseCursor.ResizeHorizontal);
			if ( Event.current.type == EventType.MouseDown && drawTo.Contains(Event.current.mousePosition) ) {
				heldDragBar = currentControl;
			}
			if ( Event.current.type == EventType.MouseDrag && heldDragBar == currentControl ) {
				if ( horizontal ) current+= Event.current.delta.y;
				else current+= Event.current.delta.x;
				if ( null != EditorWindow.mouseOverWindow ) EditorWindow.mouseOverWindow.Repaint();
				ret = true;
			}
			current = Mathf.Clamp(current,min,max);
			if ( Event.current.type == EventType.MouseUp ) heldDragBar = -1;
			GUI.Box (drawTo,"");
			return ret;
		}
		
	}
	
	public static bool ResizableGrid(Rect within, ref float[] verticals, ref float[] horizontals, float horizontalMinimumSize = 60f, float verticalMinimumSize = 60f) {
		float min, max = within.width-horizontalMinimumSize;
		GUI.Box(within,"");
		bool ret = false;
		for(int vertical=0; vertical<verticals.Length;vertical++) {
			if ( vertical==0 ) { min = horizontalMinimumSize; }
			else {
				min = verticals[vertical-1]+horizontalMinimumSize;
			}
			max = within.width;
			Debug.Log(max);
			//			GUI.Box(new Rect(min,within.y,max-min,within.height),"");
			if ( ResizableGridBar(within, ref verticals[vertical], min,max,false) ) ret = true;
		}
		for(int horizontal=0; horizontal<horizontals.Length;horizontal++) {
			if ( horizontal==0 ) { min = verticalMinimumSize; }
			else { 
				min = horizontals[horizontal-1]+verticalMinimumSize;
			}
			max = within.height - verticalMinimumSize * (horizontals.Length-horizontal-1);
			if ( ResizableGridBar(within, ref horizontals[horizontal], min,max,true) ) ret = true;
		}
		return ret;
	}
	static bool ResizableGridBar(Rect within, ref float current, float min, float max, bool horizontal) {
		NextControl();
		bool ret = false;
		Rect drawTo = new Rect(within);
		if ( horizontal ) { drawTo.height = 4; drawTo.y+=current; }
		else { drawTo.width = 4; drawTo.x += current; }
		EditorGUIUtility.AddCursorRect(drawTo,horizontal?MouseCursor.ResizeVertical:MouseCursor.ResizeHorizontal);
		if ( Event.current.type == EventType.MouseDown && drawTo.Contains(Event.current.mousePosition) ) {
			heldDragBar = currentControl;
		}
		if ( Event.current.type == EventType.MouseDrag && heldDragBar == currentControl ) {
			if ( horizontal ) current+= Event.current.delta.y;
			else current+= Event.current.delta.x;
			ret = true;
		}
		current = Mathf.Clamp(current,min,max);
		if ( Event.current.type == EventType.MouseUp ) heldDragBar = -1;
		GUI.Box (drawTo,"");
		return ret;
	}
	
	public static bool ResizableColumns(ref float[] positions, int count=0, float beginAt=0f, float height=0f, float minPosition=0f, float maxPosition=200f, float buffer = 60f) {
		bool changed = false;
		if ( null==positions || (count != 0 && positions.Length != count) ) {
			positions = new float[count];
			for(int i=0; i<count; i++) {
				positions[i]=(maxPosition/(float)count)*((float)i+0.5f);
			}
		}
		if ( null==columnWidth || columnWidth.Length != count+1 ) columnWidth = new float[count+1];
		float min,max;
		for(int ix=0; ix<count; ix++) {
			if ( ix==0 ) min = minPosition+buffer; else min=positions[ix-1]+buffer;
			if ( ix==count-1 ) max=maxPosition-buffer; else max=positions[ix+1]-buffer;
			float x = positions[ix];
			positions[ix] = Mathf.Clamp(DraggableBar(positions[ix],beginAt,height),min,max);
			if ( positions[ix] != x ) changed = true;
			columnWidth[ix] = positions[ix]-min+buffer;
		}
		columnWidth[count]=maxPosition-positions[count-1];
		return changed;
	}
	public static float[] columnWidth;
	
	public static float DraggableBar(float pos, float start, float length, bool vertical = true) {
		NextControl();
		Rect r;
		length-= start;
		if ( vertical ) {
			r = new Rect(pos-2,start,4,length);
			EditorGUIUtility.AddCursorRect(r,MouseCursor.ResizeHorizontal);
		} else {
			r = new Rect(start,pos-2,length,4);
			EditorGUIUtility.AddCursorRect(r,MouseCursor.ResizeVertical);
		}
		
		if ( Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition) ) {
			heldDragBar = currentControl;
		}
		if ( Event.current.type == EventType.MouseDrag && heldDragBar == currentControl ) {
			if ( vertical ) pos+= Event.current.delta.x;
			else pos+= Event.current.delta.y;
		}
		if ( Event.current.type == EventType.MouseUp ) heldDragBar = -1;
		GUI.Box (r,"");
		return pos;
	}
	#endif
	#endregion
	
	#region Custom Styles
	private static GUIStyle[] customStyles=new GUIStyle[20];
	public static GUIStyle StyleDepressedButton {
		get {
			if ( null==customStyles[0] ) {
				customStyles[0] = new GUIStyle(GUI.skin.button);
				customStyles[0].normal = customStyles[0].active;
			}
			return customStyles[0];
		}
	}
	public static GUIStyle StyleWrappingLabel {
		get { 
			if ( null == customStyles[1] ) {
				customStyles[1] = new GUIStyle(GUI.skin.label);
				customStyles[1].wordWrap = true;
				customStyles[1].stretchWidth = false;
			}
			return customStyles[1];
		}
	}
	public static GUIStyle StyleLeftButton {
		get { 
			if ( null==customStyles[2] ) {
				customStyles[2] = new GUIStyle(GUI.skin.button);
				customStyles[2].alignment = TextAnchor.MiddleLeft;
			}
			return customStyles[2];
		}
	}
	public static GUIStyle StyleWrappingTextArea {
		get {
			if ( null == customStyles[3] ) {
				customStyles[3] = new GUIStyle(GUI.skin.textArea);
				customStyles[3].wordWrap = true;
				customStyles[3].stretchWidth = false;
			}
			return customStyles[3];
		}
	}
	public static void WrappedLabel(string text) {
		GUILayout.Label(text, StyleWrappingLabel);
	}
	#endregion
	
	#region Editor Window Extensions
	#if UNITY_EDITOR
	private static GUIContent GetEWTitleContent(EditorWindow ew) {
		PropertyInfo p = typeof(EditorWindow).GetProperty("cachedTitleContent",BindingFlags.Instance|BindingFlags.NonPublic);
		GUIContent gc = p.GetValue(ew,null) as GUIContent;
		return gc;
	}
	
	public static void SetIcon(this EditorWindow ew, Texture2D icon) {
		GetEWTitleContent(ew).image = icon;
	}
	public static void SetTitle(this EditorWindow ew, string text) {
		GetEWTitleContent(ew).text = text;
	}
	#endif
	#endregion
	
	#region Internals
	private static bool 
		drawPoly, mouseUp, mouseDown, polypointEat;
	
	private static int maxControlCount = 0;
	private static void NextControl() {
		currentControl++;
		if (currentControl>10000) { 
			if (currentControl > maxControlCount) {
				maxControlCount = currentControl;
				Debug.LogError(maxControlCount + " GUIUtils controls! Did you GUIUtils.BeginGUI?");
			}
		}
	}
	public static void BeginGUI() { 
		activePopup.listContent=null;
		_minimumFixedWidthLabelSize=120; currentControl = 0;
		mouseUp = Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseUp;
		mouseDown = Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDown ;
		polypointEat = 
			Event.current.modifiers == (EventModifiers)0 &&
				Event.current.type != EventType.Repaint &&
				Event.current.type != EventType.MouseDrag &&
				Event.current.type != EventType.Layout &&
				Event.current.type != EventType.KeyUp &&
				Event.current.type != EventType.KeyDown &&
				Event.current.type != EventType.ScrollWheel &&
				Event.current.type != EventType.DragExited &&
				Event.current.type != EventType.DragPerform;
	}
	
	#endregion
	
	#region Scene-View Handles
	#if UNITY_EDITOR
	public static Rect LabelHandle(Vector3 world, string label) { return LabelHandle(world,label,GUI.skin.label); }
	public static Rect LabelHandle(Vector3 world, string label, GUIStyle style) {
		UnityEditor.Handles.BeginGUI();
		Rect ret = new Rect();
		if ( Vector3.Angle(world-Camera.current.transform.position,Camera.current.transform.forward) < 90 ) {
			Vector2 pos2d = HandleUtility.WorldToGUIPoint(world);
			Vector2 v2 = style.CalcSize(new GUIContent(label));
			GUI.Label(new Rect(pos2d.x,pos2d.y,v2.x, v2.y),label, style);
			ret.x = pos2d.x; ret.y = pos2d.y; ret.width = v2.x; ret.height = v2.y;
		}
		UnityEditor.Handles.EndGUI();
		return ret;
	}
	#endif
	#endregion
	
	#region Layout Fields

	public static bool SpriteButton(int width, int height, Texture sourceTexture, Sprite sprite, bool isDepressed, params GUILayoutOption[] options) {
		Rect r;
		if ( sprite ) r = new Rect(sprite.rect);
		else r = new Rect();
		Rect draw = GUILayoutUtility.GetRect(width, height, options);
		if ( sourceTexture ) {
			r.width /= sourceTexture.width;
			r.height /= sourceTexture.height;
			r.x /= sourceTexture.width;
			r.y /= sourceTexture.height;
			if ( GUI.Button(draw, "", isDepressed?GUIUtils.StyleDepressedButton:GUI.skin.button) ) {
				return true;
			}
			draw.x+=3; draw.width-=6; draw.height-=6; draw.y+=3;
			GUI.DrawTextureWithTexCoords(draw, sourceTexture, r);
		}
		return false;
	}

	#if UNITY_EDITOR
	public static float PercentBar(float current) {
		Rect r = GUILayoutUtility.GetRect(40,1000,20,20);
		return PercentBar(current,r);
	}
	static int editingPercentBar = -1;
	public static float PercentBar(float current, Rect rect) {
		Rect r = new Rect(rect);
		int nextControlID = GUIUtility.GetControlID(FocusType.Passive) + 1;
		rect.width-=10;
		if ( editingPercentBar == nextControlID ) {
			current = EditorGUI.FloatField(rect, current);
		} else {
			EditorGUI.ProgressBar(rect, current, Mathf.RoundToInt(current*100f)+"%");
		}
		r.x += r.width-10;
		r.width = 10;
		r.height -= 2;
		if ( GUI.Button(r,gcEditValueByHand) ) {
			if ( editingPercentBar == nextControlID ) editingPercentBar = -1;
			else editingPercentBar = nextControlID;
		}
		if ( Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) {
			if ( EditorWindow.focusedWindow ) EditorWindow.focusedWindow.Repaint();
			else if ( EditorWindow.mouseOverWindow ) EditorWindow.mouseOverWindow.Repaint();
			Vector2 mp = Event.current.mousePosition;
			if ( rect.Contains(mp) ) {
				current = (mp.x-rect.x) / (rect.width);
				GUI.changed = true;
			} else if ( editingPercentBar == nextControlID ) {
				if ( rect.x > mp.x ) current = 0f;
				else if ( rect.x + rect.width < mp.x ) current = 1f;
			}
		}
		return current;
	}

	public static bool ToggleButton(ref bool value, string label) {
		if ( GUILayout.Button(label,value?StyleDepressedButton:GUI.skin.button) ) value = !value;
		return value;
	}

	public static T EnumPopup<T>(T thing, string label) {
		return EnumPopup(thing, new GUIContent(label));
	}
	public static T EnumPopup<T>(T thing, GUIContent label) {
		if ( !typeof(T).IsEnum ) {
			Debug.LogError (typeof(T) + " is not an enum.");
			return thing;
		}

		EnumInfo info = EnumInfo.For(typeof(T));
		int i = System.Convert.ToInt32(thing);
		if ( i >= 0 ) i = System.Array.IndexOf(info.values, i);
		if ( null == label ) {
			i = EditorGUILayout.Popup(i, info.names);
		} else {
			i = EditorGUILayout.Popup(label.text, i, info.names);
		}
		if ( i >= 0 ) i = info.values[i];
		return (T)System.Enum.ToObject(typeof(T),i);
	}

	public static T CategorizedEnumPopup<T>(T thing, GUIContent label) {
		if ( !typeof(T).IsEnum ) {
			Debug.LogError (typeof(T) + " is not an enum.");
			return thing;
		}
		EnumInfo info = EnumInfo.For(typeof(T));
		int i = System.Convert.ToInt32(thing);
		if ( i >= 0 ) i = System.Array.IndexOf(info.values, i);
		if ( null == label ) {
			i = EditorGUILayout.Popup(i, info.categorizedNames);
		} else {
			i = EditorGUILayout.Popup(label.text, i, info.categorizedNames);
		}
		if ( i >= 0 ) i = info.values[i];
		return (T)System.Enum.ToObject(typeof(T),i);
	}
	
	public static int CategorizedPopup(int current, string[] options) {
		string[] realOptions = options.Select(x=>x.AsSpacedCamelCase().Replace(" ","/")).ToArray();
		int selected = EditorGUILayout.Popup(current, realOptions);
		if ( selected < 0 || selected > realOptions.Length ) return current;
		return Array.IndexOf(options, realOptions[selected]);
	}
	
	static string searchString = "";
	public static int SearchablePopup(int current, string[] options) {
		using(new Vertical("box")) {
			using(new Horizontal()) {
				GUILayout.Label("Search:", GUILayout.ExpandWidth(false));
				searchString = EditorGUILayout.TextField(searchString).ToLower();
			}
			int selected = current;
			List<string> listRealOptions = new List<string>();
			for(int i=0; i<options.Length; i++) {
				if ( options[i].ToLower().Contains(searchString) ) listRealOptions.Add(options[i]);
				else if (i < current) selected--;
				else if (i==current) selected = -1;
			}
			string[] realOptions = listRealOptions.ToArray();
			selected = EditorGUILayout.Popup(selected, realOptions);
			if ( selected < 0 || selected > realOptions.Length ) return current;
			return Array.IndexOf(options, realOptions[selected]);
		}
	}
	
	public static T EnumSpread<T>(T value, params T[] forbidden) {
		EnumInfo info = EnumInfo.For(typeof(T));
		int ix=0;
		foreach (string s in info.categorizedNames) {
			bool has = value.EnumMaskContains( info.values[ix] );
			GUIUtils.DefaultField( ref has, s );
			if ( has ) value=value.EnumMaskAdd( info.values[ ix ] );
			else value=value.EnumMaskRemove( info.values[ ix ] );
			ix++;
		}
		return value;
	}
	#endif
	
	public static int ButtonList(int current, string[] names)  {
		int ix=0;
		foreach (string s in names) {
			if ( GUILayout.Button(s, (current==ix)?StyleDepressedButton:GUI.skin.button)) current = ix;
			ix++;
		}
		return current;
	}
	
	public static int LayoutIntField(int i, params GUILayoutOption[] opts) {
		string s = GUILayout.TextField(""+(i==0?"":i.ToString()), opts);
		int o = i;
		try { 
			o = Convert.ToInt32 (s);
		} catch ( Exception ) { o = 0; }
		i = o;
		return i;
	}
	public static float LayoutFloatField(float i, params GUILayoutOption[] opts) {
		string s = GUILayout.TextField(""+(i==0f?"":i.ToString()), opts);
		float o = i;
		try { 
			o = Convert.ToSingle (s);
		} catch ( Exception ) { o = 0f; }
		i = o;
		return i;
	}
	
	#region Popups
	static int popupListHash = "PopupList".GetHashCode();
	static Dictionary<int,bool> popupsOpen = new Dictionary<int, bool>();
	static ActivePopup activePopup = new ActivePopup();
	class ActivePopup {
		public Rect position;
		public GUIContent[] listContent;
		public int listEntry;
		public GUIStyle listStyle, boxStyle;
	}
	
	public static void RewriteActivePopup() {
		if ( null == activePopup.listContent ) return;
		Rect listRect = new Rect(activePopup.position.x, activePopup.position.y, activePopup.position.width, activePopup.listStyle.CalcHeight(activePopup.listContent[0], 1.0f)*activePopup.listContent.Length);
		GUI.Box(listRect, "", activePopup.boxStyle);
		activePopup.listEntry = GUI.SelectionGrid(listRect, activePopup.listEntry, activePopup.listContent, 1, activePopup.listStyle);
	}
	public static bool Popup(Rect position, ref string current, GUIContent button, GUIContent[] list, GUIStyle listStyle) {
		int ix = System.Array.IndexOf(list.Select(x=>x.text).ToArray(),current);
		bool ret = Popup(position, ref ix,button,list,listStyle);
		if ( ix > -1 ) current = list[ix].text; 
		return ret;
	}
	public static bool Popup(Rect position, ref int listEntry, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle) {
		return Popup(position, ref listEntry, buttonContent, listContent, "button", "box", listStyle);
	}
	public static bool Popup(Rect position, ref int listEntry, GUIContent buttonContent, GUIContent[] listContent,
	                         GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle) {
		NextControl();
		if ( !popupsOpen.ContainsKey(currentControl) ) popupsOpen.Add(currentControl,false);
		int controlID = GUIUtility.GetControlID(popupListHash, FocusType.Passive);
		bool done = false;
		switch (Event.current.GetTypeForControl(controlID)) {
		case EventType.MouseDown:
			if (position.Contains(Event.current.mousePosition)) {
				GUIUtility.hotControl = controlID;
				popupsOpen[currentControl] = true;
			}
			break;
		case EventType.MouseUp:
			if (popupsOpen[currentControl]) {
				done = true;
			}
			break;
		}
		
		GUI.Label(position, buttonContent, buttonStyle);
		if (popupsOpen[currentControl]) {
			activePopup.position = position; activePopup.boxStyle = boxStyle; activePopup.listContent = listContent;
			activePopup.listEntry = listEntry; activePopup.listStyle = listStyle;
			Rect listRect = new Rect(position.x, position.y, position.width, listStyle.CalcHeight(listContent[0], 1.0f)*listContent.Length);
			GUI.Box(listRect, "", boxStyle);
			listEntry = GUI.SelectionGrid(listRect, listEntry, listContent, 1, listStyle);
		}
		if (done) {
			popupsOpen[currentControl] = false;
		}
		return done;
	}
	
	#if UNITY_EDITOR
	public static void StringPopup(ref string current, string[] options) {
		int index = -1;
		if ( null == options ) {GUILayout.Label("Null option list"); return;}
		if ( !string.IsNullOrEmpty(current) ) {
			index = System.Array.IndexOf(options,current);
		}
		index = EditorGUILayout.Popup(index, options);
		if ( index > -1 ) current = options[index];
	}
	
	public static void TextFieldWithDefaults(ref string current, string label, string[] defaults, string newOption = "... Other") { TextFieldWithDefaults(ref current, new GUIContent(label), defaults, newOption); }
	public static void TextFieldWithDefaults(ref string current, GUIContent label, string[] defaults, string newOption = "... Other") {
		using ( new GUIUtils.Horizontal() ) {
			if ( null==current ) current = defaults[0];
			FixedWidthLabel(label);
			int i = System.Array.IndexOf(defaults,current);
			if ( i == -1 ) {
				current = EditorGUILayout.TextArea(current);
				i = EditorGUILayout.Popup(i,defaults,GUILayout.Width(20));
			} else { 
				i = EditorGUILayout.Popup(i,defaults.With(newOption));
			}
			if ( i == defaults.Length ) current="";
			else if ( i > -1 ) current = defaults[i];
			
		}
	}
	
	#region Refreshable Popups
	public static string[] ListOfLayerNames() {
		string[] layerNames = new string[0];
		layerNames= new string[0];
		
		for(int i=0; i<32; i++) { 
			string s = LayerMask.LayerToName(i);
			if ( s != "" && s != null ) layerNames = layerNames.With(s);
			else layerNames = layerNames.With("... "+i);
		}
		
		return layerNames;
	}
	public static string[] ListOfBuiltSceneNames() {
		string[] ret = new string[0];
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
		foreach(EditorBuildSettingsScene scene in scenes) {
			ret = ret.With(scene.path.GetFileNameOnly());
		}
		return ret;
	}
	
	static Dictionary<RefreshPopupOptions,string[]> refreshablePopupOptions = new Dictionary<RefreshPopupOptions, string[]>();
	public static void RefreshablePopup(ref int current, string label, RefreshPopupOptions rpo){ RefreshablePopup(ref current,new GUIContent(label),rpo); }
	public static void RefreshablePopup(ref int current, GUIContent label, RefreshPopupOptions rpo) {
		using ( new Horizontal() ) {
			if ( !refreshablePopupOptions.ContainsKey(rpo) ) {
				refreshablePopupOptions.Add(rpo, rpo());
			}
			FixedWidthLabel(label);
			current = EditorGUILayout.Popup(current,refreshablePopupOptions[rpo]);
			if ( GUILayout.Button(gcRefresh,GUILayout.ExpandWidth(false))) {
				refreshablePopupOptions[rpo] = rpo();
			}
		}
	}
	public static void RefreshableStringPopup(ref string current, string label, RefreshPopupOptions rpo){ RefreshableStringPopup(ref current,new GUIContent(label),rpo); }
	public static void RefreshableStringPopup(ref string current, GUIContent label, RefreshPopupOptions rpo) {
		using ( new Horizontal() ) {
			if ( !refreshablePopupOptions.ContainsKey(rpo) ) {
				refreshablePopupOptions.Add(rpo, rpo());
			}
			FixedWidthLabel(label);
			StringPopup(ref current, refreshablePopupOptions[rpo]);
			if ( GUILayout.Button(gcRefresh,GUILayout.ExpandWidth(false))) {
				refreshablePopupOptions[rpo] = rpo();
			}
		}
	}
	#endregion
	
	#endif
	#endregion
	
	
	#endregion
	
	#region Mouse In Poly
	#if UNITY_EDITOR
	public static bool PointInPoly(Vector2 pt, Vector2[] pts) {
		bool c = false;
		int i, j;
		for (i = 0, j = pts.Length-1; i < pts.Length; j = i++) {
			if ( ((pts[i].y>pt.y) != (pts[j].y>pt.y)) &&
			    (pt.x < (pts[j].x-pts[i].x) * (pt.y-pts[i].y) / (pts[j].y-pts[i].y) + pts[i].x) )
				c =	!c;
		}
		return c;
	}
	
	public static void EatSceneEvent() {
		Event.current.Use();
	}
	
	private static int mouseDownOnControlID;
	
	public static bool RectangleHandleButton( Vector3[] verts, Color fill, Color outline, Color hoverFill, Color hoverOutline ) {
		NextControl();
		Vector2[] bounds = new Vector2[4];
		bounds[0] = HandleUtility.WorldToGUIPoint(verts[0]);
		bounds[1] = HandleUtility.WorldToGUIPoint(verts[1]);
		bounds[2] = HandleUtility.WorldToGUIPoint(verts[2]);
		bounds[3] = HandleUtility.WorldToGUIPoint(verts[3]);
		bool mouseInBounds = PointInPoly(Event.current.mousePosition, bounds);
		bool ret = false;
		if ( mouseInBounds ) {
			if ( Event.current.type == EventType.Repaint ) UnityEditor.Handles.DrawSolidRectangleWithOutline(verts, hoverFill, hoverOutline);
			if ( (int)Event.current.modifiers != 0 ) return false;
			
			if (mouseUp) {
				if ( currentControl == mouseDownOnControlID) {
					ret = true;
				} else {
					polypointEat = false;
				}
			}
			if (mouseDown) mouseDownOnControlID = currentControl;
			if (polypointEat ) EatSceneEvent();
			
		} else {
			if ( Event.current.type == EventType.Repaint ) UnityEditor.Handles.DrawSolidRectangleWithOutline(verts, fill, outline);
		}
		return ret;
	}
	#endif
	#endregion
	
	#region Undo
	#if UNITY_EDITOR
	/* Starts an area to check for Undoability */
	public static void StartUndo( UnityEngine.Object target, string s ) {
		EditorGUI.BeginChangeCheck();
		Undo.RecordObject(target, s);
	}
	/* End an Undo-check area */
	public static bool EndUndo( UnityEngine.Object target, string s ) {
		bool ret = EditorGUI.EndChangeCheck();
		if ( ret ) {
			EditorUtility.SetDirty(target);
		}
		return ret;
	}
	/* Directly register an Undo */
	public static void RegisterUndo( this UnityEngine.Object target, string s ) { Undo.RecordObject( target, s ); }
	#endif
	#endregion
	
	#region Default Fields
	/* Default fields */
	/*
	* Usage:
	* 
	* DefaultField( bool/int/float/string variable, string/GUIContent label )
	*  Provides the EditorGUI's default field for the variable, and gives it the indicated label.
	* 
	* LimitField( int/float variable, string/GUIContent label, int/float low, [int/float high] )
	*  Does the same as DefaultField, but prevents the variable from going below the 'low' value. Optionally prevents the value from going above the 'high' value.
	*  variable, low, and high must be the same type (int or float).
	*/
	
	private static int _minimumFixedWidthLabelSize = 80;
	public static void FixedWidthLabel(string label) { FixedWidthLabel(new GUIContent(label)); }
	public static void FixedWidthLabel(GUIContent label) {
		int size = (System.Math.Max(_minimumFixedWidthLabelSize,(int)(GUI.skin.label.CalcSize(label).x)));
		GUILayout.Label(label, GUILayout.Width(size));
	}
	#if UNITY_EDITOR
	class EnumInfo {
		static Dictionary<System.Type, EnumInfo> dict = new Dictionary<System.Type,EnumInfo>();
		public static EnumInfo For(System.Type type) {
			if ( !dict.ContainsKey(type)) {
				EnumInfo newInfo = new EnumInfo();
				newInfo.names = System.Enum.GetNames(type);
				newInfo.categorizedNames =
					System.Enum.GetNames(type)
						.Select(x=>x.AsSpacedCamelCase().Replace(" ","/")).ToArray();
				newInfo.values = (int[])
					System.Enum.GetValues(type);
				dict.Add(type,newInfo);
			}
			return dict[type];
		}
		public string[] names;
		public string[] categorizedNames; public int[] values;
	}
	
	public static void DefaultField( ref bool   arg, string label ) { DefaultField(ref arg, new GUIContent(label)); }
	public static void DefaultField( ref int    arg, string label ) { DefaultField(ref arg, new GUIContent(label)); }
	public static void DefaultField( ref float  arg, string label ) { DefaultField(ref arg, new GUIContent(label)); }
	public static void DefaultField( ref string arg, string label ) { DefaultField(ref arg, new GUIContent(label)); }
	public static void DefaultField<T>( ref T arg, string label, bool allowSceneObjects ) where T : UnityEngine.Object {DefaultField(ref arg, new GUIContent(label), allowSceneObjects);}
	
	public static void LimitField( ref int   arg, string label, int   low, int   high ) { LimitField(ref arg, new GUIContent(label),low,high); }
	public static void LimitField( ref float arg, string label, float low, float high ) { LimitField(ref arg, new GUIContent(label),low,high); }
	public static void LimitField( ref int   arg, string label, int   low) { LimitField(ref arg,label,low,int.MaxValue); }
	public static void LimitField( ref float arg, string label, float low) { LimitField(ref arg,label,low,Mathf.Infinity); }
	
	public static void DefaultField( ref bool   arg, GUIContent label ) { using(new GUIUtils.LabelWidth(label.text,true)) arg = EditorGUILayout.Toggle(label,arg);}
	public static void DefaultField( ref int    arg, GUIContent label ) { using(new GUIUtils.LabelWidth(label.text,true)) arg = EditorGUILayout.IntField(label,arg);}
	public static void DefaultField( ref float  arg, GUIContent label ) { using(new GUIUtils.LabelWidth(label.text,true)) arg = EditorGUILayout.FloatField(label,arg);}
	public static void DefaultField( ref string arg, GUIContent label ) { if ( arg==null ) arg = ""; using(new GUIUtils.LabelWidth(label.text,true)) arg = EditorGUILayout.TextField(label,arg); }
	public static void DefaultField<T>( ref T arg, GUIContent label, bool allowSceneObjects ) where T : UnityEngine.Object { using(new GUIUtils.LabelWidth(label.text,true)) arg = (T)EditorGUILayout.ObjectField(label,arg,typeof(T),allowSceneObjects); }
	
	public static void LimitField(ref int   arg, GUIContent label, int   low, int   high) { using(new GUIUtils.LabelWidth(label.text,true)) arg = System.Math.Min(high,System.Math.Max(low,EditorGUILayout.IntField(label,arg))); }
	public static void LimitField(ref float arg, GUIContent label, float low, float high) { using(new GUIUtils.LabelWidth(label.text,true)) arg = System.Math.Min(high,System.Math.Max(low,EditorGUILayout.FloatField(label,arg))); }
	public static void LimitField(ref int   arg, GUIContent label, int   low) { LimitField(ref arg,label,low,int.MaxValue); }
	public static void LimitField(ref float arg, GUIContent label, float low) { LimitField(ref arg,label,low,Mathf.Infinity); }
	
	public static void PercentField(ref float arg, string label, float low, float high) {PercentField(ref arg, new GUIContent(label,""), low, high);}
	public static void PercentField(ref float arg, string label, float low) {PercentField(ref arg, new GUIContent(label,""), low);}
	
	public static void PercentField(ref float arg, GUIContent label, float low, float high) { 
		if ( high == Mathf.Infinity ) PercentField(ref arg, label, low);
		else {
			int perc = Mathf.RoundToInt(arg*100f); GUIUtils.LimitField(ref perc, label, Mathf.RoundToInt(low*100f), Mathf.RoundToInt(high*100f));
			arg = perc/100f;
		}
	}
	public static void PercentField(ref float arg, GUIContent label, float low) { 
		int perc = Mathf.RoundToInt(arg*100f); GUIUtils.LimitField(ref perc, label, Mathf.RoundToInt(low*100f));
		arg = perc/100f;
	}
	
	#endif
	#endregion
	
	#region Formatting
	/* These functions begin and end a horizontal indentation */
	/*
	* Usage:
	* 
	* BeginIndent();
	* DefaultField(var1,"Var1");
	* EndIndent();
	* 
	*/
	public static void BeginIndent(GUIStyle style) { GUILayout.BeginHorizontal(); GUILayout.Space(20); GUILayout.BeginVertical(style); }
	public static void BeginIndent(string style) { GUILayout.BeginHorizontal(); GUILayout.Space(20); GUILayout.BeginVertical(style); }
	public static void BeginIndent() { BeginIndent(GUIStyle.none); }
	public static void EndIndent() { GUILayout.EndVertical(); GUILayout.EndHorizontal(); }
	#endregion
	
	#region Arrays
	/* Array helpers */
	// Insert 'newOne' into array 'arr' at index 'ix'
	public static void ArrayInsert<T>( ref T[] arr, int ix, T newOne )	{
		int ix2;
		T[] temp;
		
		temp = new T[arr.Length+1];
		for ( ix2 = arr.Length; ix2 > ix; ix2-- ) { temp[ix2] = arr[ix2-1]; }
		temp[ix] = newOne;
		for ( ix2 = ix-1; ix2 >= 0; ix2-- ) { temp[ix2] = arr[ix2]; }
		arr = temp;
		temp = null;
	}
	
	// Swap elements at index 'ix' and 'ix2' of array 'arr'
	public static void ArraySwap<T>( ref T[] arr, int ix, int ix2 ) {
		T temp;
		temp = arr[ix];
		arr[ix] = arr[ix2];
		arr[ix2] = temp;
	}
	
	// Remove element at index 'rem' of array 'arr'
	public static void ArrayRemove<T>( ref T[] arr, int rem ) {
		int ix2;
		T[] temp;
		
		temp = new T[arr.Length-1];
		for ( ix2=0; ix2<rem;ix2++ ) { temp[ix2]=arr[ix2]; }
		for ( ix2=rem+1; ix2<arr.Length;ix2++) { temp[ix2-1]=arr[ix2]; }
		arr = temp;
	}
	
	public delegate string[] RefreshPopupOptions();
	public delegate void EmptyDelegate();
	public delegate void TwoVector3Args(Vector3 a, Vector3 b);
	// Delegates for ArrangableArray functions.
	public delegate bool RefAndIndexQuery<T>(ref T arg, int ix); // True if contents should be displayed
	public delegate void RefAndIndexNotify<T>(ref T arg, int ix);
	public delegate void IndexNotify(int ix);
	public delegate bool ElementAndIndexQuery<T>(T arg, int ix);
	public delegate void ElementAndIndexNotify<T>(T arg,int ix);
	public delegate bool DualIndicesQuery(int ix1, int ix2);
	public delegate void DualIndicesNotify(int ix1, int ix2);
	public delegate T    ElementCopier<T>(T t); // return new value
	public delegate T    ElementBuilder<T>();
	
	public static class Clipboard<T>{public static T value=default(T);}
	
	static T GetNewArrArrElement<T>() {
		T ret = (T)System.Activator.CreateInstance(typeof(T));
		return ret;
	}
	
	/// <summary>Provides a GUI to edit a [] array. Assumes that new T() is an appropriate new value.</summary>
	/// <param name='ina'>The [] array.</param>
	/// <param name='header'>Header function</param>
	/// <param name='body'>Body.</param>
	/// <param name='preRemove'>Called before removing an element. Return false to cancel removal.</param>
	/// <param name='removed'>Called when an element is Removed.</param>
	/// <param name='added'>Called when an element is Added.</param>
	/// <param name='preSwap'>Called before two elements are swapped. Return false to prevent swap.</param>
	/// <param name='swapped'>Called after two elements are Swapped.</param>
	/// <param name='duplicator'>Called to duplicate an element.</param>
	/// <param name='forbidRearrange'>Forbid rearrange.</param>
	/// <param name='forbidDelete'>Forbid delete.</param>
	/// <param name='elementName'>Create new (element name) instead of new (type name).</param>
	/// <typeparam name='T'>Type of the array.</typeparam>
	public static void ArrangableArray<T>( 
	                                      ref T[] ina,
	                                      RefAndIndexQuery<T> 	header, 
	                                      RefAndIndexNotify<T> 	body=null, 
	                                      ElementAndIndexQuery<T>	preRemove=null,
	                                      IndexNotify 			removed=null,
	                                      IndexNotify				added=null, 
	                                      DualIndicesQuery		preSwap=null,
	                                      DualIndicesNotify	 	swapped=null,
	                                      ElementCopier<T> 		duplicator=null,
	                                      ElementCopier<T>		copier=null,
	                                      bool					forbidRearrange=false,
	                                      bool					forbidDelete=false,
	                                      bool					forbidAdd=false,
	                                      string					elementName=null,
	                                      string					elementStyle="box",
	                                      string					contentsBorderStyle="textarea"
	                                      ) where T : class, new()
	{
		ArrangableArrayWithConstructor(ref ina, header, GetNewArrArrElement<T>, body, preRemove, removed, added, preSwap, swapped, duplicator, copier, forbidRearrange, forbidDelete, forbidAdd, elementName, elementStyle, contentsBorderStyle);
	}
	
	public static void ArrangableArrayWithConstructor<T>(
		ref T[] ina,
		RefAndIndexQuery<T> 	header,
		ElementBuilder<T>		constructor,
		RefAndIndexNotify<T> 	body=null, 
		ElementAndIndexQuery<T>	preRemove=null,
		IndexNotify 			removed=null,
		IndexNotify 			added=null, 
		DualIndicesQuery		preSwap=null,
		DualIndicesNotify	 	swapped=null,
		ElementCopier<T> 		duplicator=null,
		ElementCopier<T>		copier=null,
		bool					forbidRearrange=false,
		bool					forbidDelete=false,
		bool					forbidAdd=false,
		string					elementName=null,
		string					elementStyle="box",
		string					contentsBorderStyle="textarea"
		) {
		ArrangableArrayInternal(ref ina,header,default(T),constructor,body,preRemove,removed,added,preSwap,swapped,duplicator,copier,forbidRearrange,forbidDelete,forbidAdd,elementName,elementStyle,contentsBorderStyle);
	}
	
	/// <summary>Provides a GUI to edit a [] array.</summary>
	/// <param name='ina'>The [] array.</param>
	/// <param name='header'>Header function</param>
	/// <param name='newValue'>Value assigned to created elements.</param>
	/// <param name='body'>Body.</param>
	/// <param name='preRemove'>Called before removing an element. Return false to cancel removal.</param>
	/// <param name='removed'>Called when an element is Removed.</param>
	/// <param name='added'>Called when an element is Added.</param>
	/// <param name='preSwap'>Called before two elements are swapped. Return false to prevent swap.</param>
	/// <param name='swapped'>Called after two elements are Swapped.</param>
	/// <param name='duplicator'>Called to duplicate an element.</param>
	/// <param name='forbidRearrange'>Forbid rearrange.</param>
	/// <param name='forbidDelete'>Forbid delete.</param>
	/// <param name='elementName'>Create new (element name) instead of new (type name).</param>
	/// <typeparam name='T'>Type of the array.</typeparam>
	public static void ArrangableArrayWithValue<T>( 
	                                               ref T[] ina,
	                                               RefAndIndexQuery<T> 	header, 
	                                               T						newValue,
	                                               RefAndIndexNotify<T> 	body=null, 
	                                               ElementAndIndexQuery<T>	preRemove=null,
	                                               IndexNotify 			removed=null, 
	                                               IndexNotify 			added=null, 
	                                               DualIndicesQuery		preSwap=null,
	                                               DualIndicesNotify	 	swapped=null,
	                                               ElementCopier<T> 		duplicator=null,
	                                               ElementCopier<T>		copier=null,
	                                               bool					forbidRearrange=false,
	                                               bool					forbidDelete=false,
	                                               bool					forbidAdd=false,
	                                               string					elementName=null,
	                                               string					elementStyle="box",
	                                               string					contentsBorderStyle="textarea"
	                                               )
	{
		ArrangableArrayInternal(ref ina,header,newValue,null,body,preRemove,removed,added,preSwap,swapped,duplicator,copier,forbidRearrange,forbidDelete,forbidAdd,elementName,elementStyle,contentsBorderStyle);
	}
	
	static void ArrangableArrayInternal<T>(
		ref T[] ina,
		RefAndIndexQuery<T> 	header, 
		T						newValue,
		ElementBuilder<T>		newBuilder=null,
		RefAndIndexNotify<T> 	body=null, 
		ElementAndIndexQuery<T>	preRemove=null,
		IndexNotify 			removed=null, 
		IndexNotify 			added=null, 
		DualIndicesQuery		preSwap=null,
		DualIndicesNotify	 	swapped=null,
		ElementCopier<T> 		duplicator=null,
		ElementCopier<T>		copier=null,
		bool					forbidRearrange=false,
		bool					forbidDelete=false,
		bool					forbidAdd=false,
		string					elementName=null,
		string					elementStyle="box",
		string	 				contentsBorderStyle="textarea"
		)
	{
		int ix;
		int rem = -1;
		bool open = false;
		GUIStyle btnstyle = GUI.skin.FindStyle("minibutton");
		if ( null == btnstyle ) btnstyle = GUI.skin.button;
		if ( null == ina ) {
			ina = new T[0];
		}
		for( ix = 0; ix < ina.Length; ix++ ) {
			if ( elementStyle == "" ) GUILayout.BeginVertical(GUIStyle.none);
			else GUILayout.BeginVertical(elementStyle);
			GUILayout.BeginHorizontal();
			open = true;
			if ( null!=header )	open = header(ref ina[ix], ix);
			if ( !forbidRearrange ) {
				if ( GUILayout.Button(gcInsert,btnstyle,GUILayout.Width(20)) ) {
					ArrayInsert( ref ina, ix, null!=newBuilder?newBuilder():newValue );
					if ( added != null ) added(ix);
				}
				if ( ix > 0 && GUILayout.Button(gcUpArrow,btnstyle,GUILayout.Width(20))) {
					bool carryOn = true;
					if ( preSwap != null ) carryOn = preSwap(ix,ix-1);
					if ( carryOn ) {
						ArraySwap(ref ina, ix,ix-1);
						if ( swapped != null ) swapped(ix,ix-1);
					}
				}
				if ( ix == 0 ) GUILayout.Space (20+btnstyle.margin.left); //GUILayout.Button("",btnstyle,GUILayout.Width(20));
				if ( ix < ina.Length-1 && GUILayout.Button(gcDownArrow,btnstyle,GUILayout.Width(20))) {
					bool carryOn = true;
					if ( preSwap != null ) carryOn = preSwap(ix,ix+1);
					if ( carryOn ) {
						ArraySwap(ref ina, ix,ix+1);
						if ( swapped != null ) swapped(ix,ix+1);
					}
				}
				if ( ix == ina.Length-1 ) GUILayout.Space (20+btnstyle.margin.left); //GUILayout.Button("",btnstyle,GUILayout.Width(20));
			}
			if ( !forbidAdd && duplicator != null ) {
				if ( GUILayout.Button(gcDuplicate,btnstyle,GUILayout.Width(20))) {
					if ( forbidRearrange ) {
						ArrayInsert( ref ina, ina.Length, duplicator( ina[ ix ] ) );
						if ( added != null ) added(ina.Length-1);
					} else {
						ArrayInsert( ref ina, ix+1, duplicator(ina[ix]) );
						if ( added != null ) added(ix+1);
					}
				}
			}
			if ( null!=copier ) {
				if ( GUILayout.Button( gcCopy, btnstyle, GUILayout.Width( 20 ) ) ) {
					Clipboard<T>.value = copier(ina[ix]);
				}
				if ( null != Clipboard<T>.value && GUILayout.Button( gcPaste, btnstyle, GUILayout.Width( 20 ) ) ) {
					ina[ ix ]=copier(Clipboard<T>.value);
				}
			}
			if ( !forbidDelete ) {
				if ( GUILayout.Button(gcRemove,btnstyle,GUILayout.Width(20))) {
					rem = ix;
				}
			}
			GUILayout.EndHorizontal();
			if ( open && (null!=body) && (null!= ina[ix]) ) {
				BeginIndent();
				
				GUILayout.BeginVertical(contentsBorderStyle);
				
				body(ref ina[ix], ix);
				GUILayout.EndVertical();
				EndIndent();
			}
			GUILayout.EndVertical();
		}
		if ( rem > -1 ) {
			bool carryOn = true;
			if ( preRemove != null ) {
				carryOn = preRemove(ina[rem], rem);
			}
			if ( carryOn ) {
				ArrayRemove( ref ina, rem );
				if ( removed != null ) removed(rem);
			} else {
				Debug.LogWarning("Removal was prevented by the pre-remove function.");
			}
		}
		if ( !forbidAdd ) {
			if ( GUILayout.Button("Add New " + (null==elementName?typeof(T).Name:elementName), btnstyle, GUILayout.ExpandWidth(false) ) ) {
				ArrayInsert(ref ina, ina.Length, null!=newBuilder?newBuilder():newValue );
				if ( added != null ) added(ina.Length-1);
			}
		}
	}
	#endregion
	
	#region Foldout
	/////////////////////////////////////////////////////////////////////////////
	/*
	* This section makes an EditorGUILayout.Foldout, except you can actually click anywhere on the word to open it
	* instead of having to click on the teeny-tiny triangle.
	* 
	* ... what the bananas, Unity
	* 
	* Usage: 
	* 
	* Foldout( ref myBoolean, "Optional Section");
	* if ( myBoolean ) {
	*   //... display interior of foldout
	* }
	*/ 
	private static GUIStyle openFoldoutStyle;
	private static GUIStyle closedFoldoutStyle;
	private static GUIStyle noRightBox;
	private static GUIStyle noRightTextArea;
	private static bool initted = false;
	private static bool inittedInPlayer = false;
	
	private static void Init() {
		#if UNITY_EDITOR
		if ( (Application.isPlaying==inittedInPlayer) && initted ) return;
		initted = true;
		inittedInPlayer = Application.isPlaying;
		if ( Application.isPlaying ) {
			#else
			if ( initted ) return;
			#endif
			openFoldoutStyle = new GUIStyle( GUI.skin.FindStyle("Button") );
			#if UNITY_EDITOR
		} else {
			openFoldoutStyle = new GUIStyle( GUI.skin.FindStyle("Foldout") );
		}
		#endif
		openFoldoutStyle.fontStyle = (FontStyle)1;
		openFoldoutStyle.stretchHeight = true;
		
		
		closedFoldoutStyle = new GUIStyle( openFoldoutStyle );
		openFoldoutStyle.normal = openFoldoutStyle.onNormal;
		openFoldoutStyle.active = openFoldoutStyle.onActive;
		
		noRightBox=new GUIStyle( GUI.skin.box );
		noRightBox.margin.right=0;
		noRightBox.margin.bottom=0;
		//noRightBox.border.right=0;
		//noRightBox.overflow.right=0;
		noRightBox.padding.right=0;
		noRightBox.padding.bottom=0;
		noRightTextArea=new GUIStyle( GUI.skin.textArea );
		noRightTextArea.margin.right=0;
		noRightTextArea.margin.bottom=0;
		//noRightTextArea.border.right=0;
		//noRightTextArea.overflow.right=0;
		noRightTextArea.padding.right=0;
		noRightTextArea.padding.bottom=0;
	}
	
	public static void StyledFoldout(ref bool state, string text, EmptyDelegate drawFunction, string style = "box", string style2 = "textarea") {
		using ( new Vertical(style) ) {
			Foldout(ref state, text);
			if ( state )
				using ( new Indent() ) 
					using ( new Vertical(style2) ) 
					drawFunction();
		}
	}
	
	public static bool StyledFoldout(bool state, string text, EmptyDelegate drawFunction, string style = "box", string style2 = "textarea") {
		using ( new Vertical(style) ) {
			Foldout(ref state, text);
			if ( state )
				using ( new Indent() ) 
					using ( new Vertical(style2) ) 
					drawFunction();
		}
		return state;
	}
	
	public static bool Foldout(bool open, string text, params GUILayoutOption[] additionalStyles) { return Foldout(ref open, new GUIContent(text), additionalStyles); }
	public static bool Foldout(ref bool open, string text, params GUILayoutOption[] additionalStyles ) { return Foldout(ref open, new GUIContent(text), additionalStyles); }
	public static bool Foldout(ref bool open, GUIContent text, params GUILayoutOption[] additionalStyles ) {
		Init();
		
		if ( GUILayout.Button( text, open ? openFoldoutStyle : closedFoldoutStyle, additionalStyles.With(GUILayout.Height(20)) ) ) {
			GUI.FocusControl ("");
			GUI.changed = false; // force change-checking group to take notice
			GUI.changed = true;
			open = !open;
		}
		#if UNITY_EDITOR
		/*
		if ( null != EditorWindow.focusedWindow && !hasRepainted ) {
			hasRepainted = true;
			EditorWindow.focusedWindow.Repaint();
		}
		/**/
		#endif
		return open;
	}
	#endregion
	
	#region Pickers
	/////////////////////////////////////////////////////////////////////////////
	// Object pickers
	/*
	* Usage:
	* 
	* boolean MyValidator( Class class ) {
	*   // return true if the class should be shown in the picker
	* }
	* 
	* Class must extend Component (MonoBehaviour does extend Component).
	* 
	* Pick from objects in project:
	* ProjectPicker( ref Class class, "Pick a Class:", MyValidator );
	* 
	* Pick from objects in scene:
	* HierarchyPicker( ref Class class, "Pick a Class:", MyValidator );
	* 
	* 
	*/
	#if UNITY_EDITOR
	
	class Folder<T> {
		public Folder( string p ) {
			name = p;
			files = new List<T>();
			folders = new List<Folder<T> >();
		}
		public string name;
		public List<T> files;
		public List<Folder<T> > folders;
		public bool open;
		public bool sorted;
		
		public static Folder<T> baseFolder;
	}
	
	public delegate bool Validator<T>(T arg);
	
	private static int TSort<T>( T x, T y ) where T : UnityEngine.Object {
		return string.Compare(x.name,y.name);
	}
	private static int FSort<T>( Folder<T> x, Folder<T> y ) {
		return string.Compare(x.name, y.name);
	}
	
	private static void ShowPickerFolder<T>( Folder<T> folder, ref T cur, bool showFoldout ) where T : UnityEngine.Object	{
		if ( null == folder ) return;
		int ix;
		
		if ( !folder.sorted ) {
			folder.files.Sort(TSort);
			folder.folders.Sort(FSort);
			folder.sorted = true;
		}
		if ( showFoldout )
			Foldout( ref folder.open, folder.name );
		else
			folder.open=true;
		
		if ( folder.open ) {
			using ( new GUIUtils.Indent() ) {
				for (ix = 0; ix < folder.folders.Count; ix++) {
					ShowPickerFolder( folder.folders[ ix ], ref cur, true );
				}
				for (ix = 0; ix < folder.files.Count; ix++) {
					if ( folder.files[ ix ]!=null ) {
						if ( cur==folder.files[ ix ] ) {
							GUILayout.Label( folder.files[ ix ].name );
						} else {
							if ( GUILayout.Button( folder.files[ ix ].name, GUILayout.ExpandWidth( false ) ) ) {
								cur=folder.files[ ix ];
								foldersOpen[ currentControl ]=false;
							}
						}
					}
				}
			}
		}
	}
	
	public static void HierarchyPicker<T>( ref T cur, string label, Validator<T> validate ) where T : Component {
		HierarchyPicker(ref cur, new GUIContent(label,""), validate);
	}
	public static void HierarchyPicker<T>( ref T cur, GUIContent label, Validator<T> validate ) where T : Component {
		NextControl();
		if ( null==foldersOpen ) foldersOpen = new List<bool>();
		while(foldersOpen.Count<=currentControl)foldersOpen.Add (false);
		
		GUILayout.BeginVertical ("box");
		GUILayout.BeginHorizontal();
		bool open = foldersOpen[currentControl];
		Foldout( ref open, label );
		foldersOpen[currentControl]=open;
		if ( GUILayout.Button ("X",GUILayout.ExpandWidth(false)) ) cur = null;
		cur = EditorGUILayout.ObjectField(cur, typeof(T), true) as T;
		if ( open && GUILayout.Button(gcRefresh, GUILayout.ExpandWidth(false)) ) BuildHierarchyFolders<T>(validate);
		GUILayout.EndHorizontal();
		//Folder<T>.baseFolder.open = open;
		if ( open ) {
			if ( Folder<T>.baseFolder==null ) BuildHierarchyFolders<T>(validate);
			GUILayout.BeginVertical ("textarea");
			GUILayout.Label("Looking for " + typeof(T).Name + "s in the scene");
			Folder<T>.baseFolder.open = true; // reopen after BHF closes it
			ShowPickerFolder<T>(Folder<T>.baseFolder, ref cur,false);
			GUILayout.EndVertical();
		}
		GUILayout.EndVertical ();
	}
	
	public static void ChildPicker<T>(ref T cur, string label    , Transform parent, Validator<T> validate = null) where T : Component {
		ChildPicker(ref cur, new GUIContent(label), parent, validate);
	}
	public static void ChildPicker<T>(ref T cur, GUIContent label, Transform parent, Validator<T> validate = null) where T : Component {
		NextControl();
		if ( null==foldersOpen ) foldersOpen = new List<bool>();
		while(foldersOpen.Count<=currentControl)foldersOpen.Add (false);
		
		GUILayout.BeginVertical ("box"); {
			bool open = foldersOpen[currentControl];
			GUILayout.BeginHorizontal(); {
				Foldout( ref open, label );
				foldersOpen[currentControl]=open;
				if ( GUILayout.Button ("X",GUILayout.ExpandWidth(false)) ) cur = null;
				GUILayout.Label((null==cur)?"Nothing selected":cur.name);
				if ( open && GUILayout.Button(gcRefresh,GUILayout.ExpandWidth(false)) ) BuildPrefabFolders<T>(parent, validate);
			} GUILayout.EndHorizontal();
			//Folder<T>.baseFolder.open = open;
			if(open) {
				if ( Folder<T>.baseFolder==null ) BuildPrefabFolders<T>(parent, validate);
				GUILayout.BeginVertical("textarea");
				GUILayout.Label ("Looking for " + typeof(T).Name + "s in  '" + parent.name + "' and its descendants");
				Folder<T>.baseFolder.open = true; // reopen after Build closes it
				Folder<T> folder = Folder<T>.baseFolder;
				ShowPickerFolder<T>(folder, ref cur,false);
				GUILayout.EndVertical ();
			}
		} GUILayout.EndVertical ();
	}
	
	public static void ProjectPicker<T>( ref T cur, string label    , Validator<T> validate = null, string extension="prefab", string location="", bool resourcesOnly=false ) where T : UnityEngine.Object {
		ProjectPicker(ref cur, new GUIContent(label,""), validate, extension, location, resourcesOnly);
	}
	public static void ProjectPicker<T>( ref T cur, GUIContent label, Validator<T> validate = null, string extension="prefab", string location="", bool resourcesOnly=false ) where T : UnityEngine.Object {
		if ( extension == "prefab" && typeof(ScriptableObject).IsAssignableFrom(typeof(T)) ) extension = "asset";
		string[] extensions = extension.Split(';');
		NextControl();
		if ( null==foldersOpen ) foldersOpen = new List<bool>();
		while(foldersOpen.Count<=currentControl)foldersOpen.Add (false);
		
		using ( new Vertical("box") ) {
			bool open = foldersOpen[currentControl];
			using ( new Horizontal() ) {
				Foldout( ref open, label );
				foldersOpen[currentControl]=open;
				if ( GUILayout.Button ("X",GUILayout.ExpandWidth(false)) ) cur = null;
				cur = EditorGUILayout.ObjectField(cur, typeof(T), true, GUILayout.Width(250)) as T;
				if ( open && GUILayout.Button(gcRefresh,GUILayout.ExpandWidth(false)) ) BuildProjectFolders<T>(validate, resourcesOnly, extensions);
				if ( resourcesOnly && null != cur && !cur.GetAssetPath().Contains("Resources")) {
					Debug.LogError("This field only accepts assets inside Resources folders! (current path is " + cur.GetResourcePath()+")");
					//					cur = null;
				}
			}
			//		Folder<T>.baseFolder.open = open;
			if ( open ) {
				using( new Vertical("textarea") ) {
					using ( new Horizontal() ) {
						string searching = "Looking for *."+extension+" " + typeof(T).Name + "s ";
						if ( location!="" ) searching+="in "+location;
						if ( resourcesOnly) searching+="in Resources folders";
						GUILayout.Label(searching);
					}
					if ( null == Folder<T>.baseFolder ) {
						//if ( GUILayout.Button("Search project (crashes?)") ) BuildProjectFolders<T>(validate, resourcesOnly, extensions);
					} else {
						Folder<T>.baseFolder.open = true; // reopen after BHF closes it
						List<string> locationList = new List<string>(location.Split('/'));
						locationList.RemoveAt (0);
						Folder<T> folder = Folder<T>.baseFolder;
						while( locationList.Count > 0 ) {
							bool found = false;
							foreach(Folder<T> f in folder.folders ) {
								if ( f.name == locationList[0] ) {
									found = true;
									folder = f;
									locationList.RemoveAt (0);
									break;
								}
							}
							if ( !found ) {
								folder = Folder<T>.baseFolder;
								break;
							}
						}
						ShowPickerFolder<T>(folder, ref cur,false);
					}
				}
			}
		}
	}
	private static List<bool> foldersOpen;
	
	private static void BuildPrefabFolders<T>(Transform start, Validator<T> validate) where T : Component {
		Folder<T>.baseFolder = new Folder<T>("!!Base folder");
		
		BuildPrefabFolderRecurse<T>(start,new List<string>(),validate);
	}
	
	private static void BuildPrefabFolderRecurse<T>(Transform start, List<string> path, Validator<T> validate) where T : Component {
		T t = start.GetComponent<T>();
		if ( null != t ) {
			if ( null==validate || validate(t) ) {
				List<string> buildPath = new List<string>(path);
				Folder<T> folder = Folder<T>.baseFolder;
				while ( buildPath.Count > 0 ) {
					bool foundFolder = false;
					for ( int ix = 0; ix < folder.folders.Count; ix++ ) {
						if ( folder.folders[ix].name.Equals(buildPath[0]) ) {
							folder = folder.folders[ix];
							foundFolder = true;
						}
					}
					if ( !foundFolder ) {
						folder.folders.Add( new Folder<T>( buildPath[0] ) );
						folder = folder.folders[ folder.folders.Count-1 ];
						
					}						
					buildPath.RemoveAt(0);
				}
				folder.files.Add( t );
			}
		}
		path.Add(start.name);
		foreach(Transform tx in start) {
			BuildPrefabFolderRecurse<T>( tx, path, validate );
		}
		path.Remove(start.name);
	}
	private static void BuildHierarchyFolders<T>(Validator<T> validate) where T : Component {
		EditorUtility.DisplayProgressBar("GUI Utilities", "Finding " + typeof(T).Name + "s in hierarchy...", 0f);
		T n;
		
		int ix, ix2;
		List<string> path;	
		Folder<T> folder;
		
		Transform t;
		
		UnityEngine.Object[] obs = Resources.FindObjectsOfTypeAll(typeof(T));
		Folder<T>.baseFolder = new Folder<T>("!!Base folder");
		
		for ( ix2 = 0; ix2 < obs.Length; ix2++ ) {
			EditorUtility.DisplayProgressBar("GUI Utilities", "Finding " + typeof(T).Name + "s in hierarchy...", 1-((float)ix2 / (float)obs.Length));
			n = obs[ix2] as T;
			bool show = true;
			if ( null != validate ) show = validate(n);
			if ( n.gameObject != null && show ) {
				t = n.transform;
				path = new List<string>();
				while( t.transform.parent ) {
					t = t.transform.parent;
					path.Add( t.name );
				}
				folder = Folder<T>.baseFolder;
				while ( path.Count > 0 ) {
					bool foundFolder = false;
					for ( ix = 0; ix < folder.folders.Count; ix++ ) {
						if ( folder.folders[ix].name.Equals(path[path.Count-1]) ) {
							folder = folder.folders[ix];
							foundFolder = true;
						}
					}
					if ( !foundFolder ) {
						folder.folders.Add( new Folder<T>( path[path.Count-1] ) );
						folder = folder.folders[ folder.folders.Count-1 ];
						
					}						
					path.RemoveAt(path.Count-1);
				}
				folder.files.Add( n );
			}
		}
		EditorUtility.ClearProgressBar();
	}
	
	private static void BuildProjectFolders<T>( Validator<T> validate, bool resources, params string[] extensions) where T : UnityEngine.Object {
		Folder<T>.baseFolder = new Folder<T>("!!Base folder");
		if (!allowFolderBuilders) return;
		string strs = extensions[0];
		int ix, pix, fix;
		
		for(ix =1; ix < extensions.Length; ix++) { strs+=", "+extensions[ix]; }
		EditorUtility.DisplayCancelableProgressBar("GUI Utilities : 0 found", "Find *."+strs+" "+ typeof(T).Name + "s in "+(resources?"resources folders in ":"")+"project...",0f);
		
		string[] files = AssetDatabase.GetAllAssetPaths().Where(
			x=>extensions.ContainsElement(System.IO.Path.GetExtension(x).Replace(".",""))
			//x=>System.Array.IndexOf(extensions, System.IO.Path.GetExtension(x).Replace(".",""))>-1
			).ToArray();
		
		T o;
		Folder<T>.baseFolder = new Folder<T>("!!Base folder");
		Folder<T> folder;
		string checkFile;
		int found = 0;
		for ( ix = files.Length; ix > 0; ix-- ) {
			checkFile = files[ix-1];
			if ( EditorUtility.DisplayCancelableProgressBar("GUI Utilities : " + found + " found", "Find *."+strs+" "+ typeof(T).Name + "s in "+(resources?"resources folders in ":"")+"project...", 1-((float)ix / (float)files.Length)) ) {
				break;
			}
			if ( resources && !checkFile.Contains("Resources") ) continue;
			UnityEngine.Object thing = UnityEditor.AssetDatabase.LoadAssetAtPath(checkFile, typeof(T));
			o = (T)thing;
			if ( null != o ) {
				bool check = true;
				if ( null != validate ) check = validate(o);
				if ( check ) {
					string[] path = files[ix-1].Split('/');
					folder = Folder<T>.baseFolder;
					pix = 1;
					
					
					while ( pix < path.Length-1 ) {
						bool foundFolder = false;
						for ( fix = 0; fix < folder.folders.Count; fix++ ) {
							if ( folder.folders[fix].name.Equals(path[pix]) ) {
								folder = folder.folders[fix];
								foundFolder = true;
							}
						}
						if ( !foundFolder ) {
							folder.folders.Add( new Folder<T>( path[pix] ) );
							folder = folder.folders[ folder.folders.Count-1 ];
						}
						pix++;
					}
					folder.files.Add( o );
					found++;
					
					path = null;
					folder = null;
				}
			}
			if ( null != thing ) {
				if ( thing is Component || thing is GameObject ) {
				} else if ( thing is AssetBundle ) {
					// ?
				} else {
					Resources.UnloadAsset(thing);
				}
			}
			if ( ix % 100 == 0 ) System.GC.Collect();
		}
		EditorUtility.ClearProgressBar();
	}
	#endif
	#endregion
};

/// <summary>Blank class used to create inspector labels without any GUI controls (toggle/field/etc)</summary>
[System.Serializable]
public class InspectorLabel { }

/// <summary>
/// Inspector helper; gives classes a name and "open" state for the editor to use.
/// </summary>
[System.Serializable]
public class InspectorHelper {
	public bool inspectorOpen = true;
	public string name = null;

	#if UNITY_EDITOR

	public bool DefaultHeader(int index) {
		bool ret = false;
		string title = GetType().Name + ": ";
		using(new GUIUtils.Horizontal()) {
			ret = GUIUtils.Foldout(ref inspectorOpen, title, GUILayout.ExpandWidth(false));
			if ( name == null ) name = index.ToString();
			name = GUILayout.TextField(name);
		}
		return ret;
	}
	#endif
}
