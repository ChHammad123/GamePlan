using UnityEngine;
using System.Collections;

public class PersistentDebugLine : MonoBehaviour
{
	public class Segment {
		public Vector3 from, to;
		public Segment(Vector3 f, Vector3 t, Color c) { from = f; to = t; color = c; }
		public Color color;
	}
	static Segment[] segments;

	static PersistentDebugLine instance;

	static void EnsureInstance() {
		if ( !instance ) {
			instance = new GameObject("Line Drawer").AddComponent<PersistentDebugLine>();
		}
	}

	void Awake() {
		segments = new Segment[0];
	}
	public static void UnregisterLine(params Segment[] lines) {
		foreach(var line in lines) {
			segments = segments.Without(line);
		}
	}

	public static Segment[] RegisterBounds(Bounds b) { 
		return RegisterBounds(b, Color.white);
	}
	public static Segment[] RegisterBounds(Bounds b, Color c) {
		Segment[] ret = new Segment[]{};
		ret = ret.With(RegisterLine(b.min,b.min.WithX(b.max.x), c));
		ret = ret.With(RegisterLine(b.min,b.min.WithY(b.max.y), c));
		ret = ret.With(RegisterLine(b.min,b.min.WithZ(b.max.z), c));
		ret = ret.With(RegisterLine(b.max,b.max.WithX(b.min.x), c));
		ret = ret.With(RegisterLine(b.max,b.max.WithY(b.min.y), c));
		ret = ret.With(RegisterLine(b.max,b.max.WithZ(b.min.z), c));
		return ret;
	}

	static Color arrowColor;
	public static void RegisterArrow(Vector3 from, Vector3 to) {
		RegisterArrow(from, to, Color.white);
	}
	public static void RegisterArrow(Vector3 from, Vector3 to, Color color) {
		arrowColor = color;
		GUIUtils.DrawArrow(from, to, RegisterArrowLine, (to-from).magnitude * 0.2f, 15f, 12);
	}
	static void RegisterArrowLine(Vector3 from, Vector3 to) {
		RegisterLine(from, to, arrowColor);
	}

	public static Segment RegisterLine(Vector3 from, Vector3 to) {
		return RegisterLine(from, to, Color.white);
	}
	public static Segment RegisterLine(Vector3 from, Vector3 to, Color color) {
		EnsureInstance();
		segments = segments.With (new Segment(from,to,color));
		return segments[segments.Length-1];
	}

	public static void Clear() {
		segments = new Segment[0];
	}

	void Update() {
		foreach(var x in segments) {
			Debug.DrawLine(x.from,x.to,x.color);
		}
	}
}

