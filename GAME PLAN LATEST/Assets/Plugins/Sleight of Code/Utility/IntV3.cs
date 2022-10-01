using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SeriV3: ISerializable {
	public static implicit operator Vector3(SeriV3 v) {return v.v3;}
	public static explicit operator SeriV3(Vector3 v) {return new SeriV3( v );}
	public SeriV3(Vector3 v) { v3 = v; }
	public static Vector3 operator*(SeriV3 v,float f) {return v.v3*f;}

	public Vector3 v3;

	public float x { get { return v3.x; } set { v3 = v3.WithX( value ); } }
	public float y { get { return v3.y; } set { v3 = v3.WithY( value ); } }
	public float z { get { return v3.z; } set { v3 = v3.WithZ( value ); } }

	public void GetObjectData(SerializationInfo info, StreamingContext context) {
		// Use the AddValue method to specify serialized values.
		info.AddValue("x", v3.x, typeof(float));
		info.AddValue("y", v3.y, typeof(float));
		info.AddValue("z", v3.z, typeof(float));
	}

	// The special constructor is used to deserialize values. 
	public SeriV3(SerializationInfo info, StreamingContext context)
	{
		// Reset the property value using the GetValue method.
		v3 = new Vector3(
			(float)info.GetValue( "x", typeof(float) ),
			(float)info.GetValue( "y", typeof(float) ),
			(float)info.GetValue( "z", typeof(float) )
		);
	}

	public override string ToString ()
	{
		return string.Format ("[x={0}, y={1}, z={2}]", x, y, z);
	}
}

/// <summary>Integer Vector3. Any input floats are rounded to int on the assumption that you meant 0.99999999 as 1.0, not 0.0.</summary>
[System.Serializable]
public class IntV3 : IEquatable<IntV3> {
	public static explicit operator Vector3(IntV3 a) { return new Vector3(a.x,a.y,a.z); }
	public static implicit operator IntV3(Vector3 a) { return new IntV3(a.x,a.y,a.z); }
	
	public static IntV3		operator+( IntV3 a, IntV3 b ) { return new IntV3(a.x+b.x,a.y+b.y,a.z+b.z); }
	public static IntV3		operator-( IntV3 a, IntV3 b ) { return new IntV3(a.x-b.x,a.y-b.y,a.z-b.z); }
	public static IntV3		operator-( IntV3 a ) { return new IntV3(-a.x,-a.y,-a.z); }
	public static Vector3	operator*(Vector3 v3, IntV3 iv3) {return new Vector3(v3.x*iv3.x,v3.y*iv3.y,v3.z*iv3.z);}
	public static Vector3	operator*(IntV3 iv3, Vector3 v3) {return new Vector3(v3.x*iv3.x,v3.y*iv3.y,v3.z*iv3.z);}
	public static Vector3	operator*(IntV3 v, float f) { return new Vector3(v.x*f,v.y*f,v.z*f); }
	public static IntV3		operator*(IntV3 v, int f) { return new IntV3(v.x*f,v.y*f,v.z*f); }
	public static bool		operator ==( IntV3 a, IntV3 b ) {return a.y == b.y && a.x == b.x && a.z == b.z;}
	public static bool		operator!=(IntV3 a, IntV3 b) { 
		return ( a.y!=b.y || a.x!=b.x || a.z!=b.z );
	}
	
	public static IntV3 zero { get { return new IntV3(0,0,0); } }
	public static IntV3 one { get { return new IntV3(1,1,1); } }
	public static IntV3 up { get { return new IntV3(0,1,0); } }
	public static IntV3 right { get { return new IntV3(1,0,0); } }
	public static IntV3 forward { get { return new IntV3(0,0,1); } }

	public delegate void ForeachFunction(IntV3 pos);
	public static void ForEachInCube(IntV3 origin, IntV3 size, ForeachFunction function) {
		IntV3 multiplier = new IntV3(size.x<0?-1:1,size.y<0?-1:1,size.z<0?-1:1);
		if ( multiplier != one ) {
			origin = new IntV3(origin.x,origin.y,origin.z);
			size = new IntV3(size.x,size.y,size.z);
			if ( multiplier.x < 0 ) {
				int newX = origin.x + size.x;
				origin.x = newX; size.x*=-1;
			}
			if ( multiplier.y < 0 ) {
				int newX = origin.y + size.y;
				origin.y = newX; size.y*=-1;
			}
			if ( multiplier.z < 0 ) {
				int newX = origin.z + size.z;
				origin.z = newX; size.z*=-1;
			}
		}
		IntV3 max = origin+size;
		for(int a=origin.x; a<=max.x; a++) {
			for(int b=origin.y; b<=max.y; b++) {
				for(int c=origin.z; c<=max.z; c++) {
					function(new IntV3(a,b,c));
				}
			}
		}
	}
	public bool Within(IntV3 range) {return x>=0 && y>=0 && z>=0 && x<range.x && y<range.y && z < range.z;}
	public bool Equals(IntV3 o) {return o.x==x&&o.y==y&&o.z==z;}
	public override int GetHashCode() { return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode(); }
	public override string ToString() { return "("+x+","+y+","+z+")"; }
	public override bool Equals(object obj) { 
		if ( !(obj is IntV3)) return false;
		IntV3 o = (IntV3)obj; return o.x==x&&o.y==y&&o.z==z;
	}
	public bool OrthogonalTo(IntV3 o) {
		if (x==o.x && y==o.y) return true;
		if (y==o.y && z==o.z) return true;
		if (z==o.z && x==o.x) return true;
		return false;
	}
	
	public int x,y,z;
	
	public IntV3() {x=y=z=0;}
	public IntV3(IntV3 a) {x=a.x;y=a.y;z=a.z;}
	public IntV3(int a, int b, int c) { x=a;y=b;z=c; }
	public IntV3(float a, float b, float c ) {x=Mathf.RoundToInt(a);y=Mathf.RoundToInt(b);z=Mathf.RoundToInt(c); }
	//public IntV3(Vector3 v) { x=Mathf.RoundToInt(v.x);y=Mathf.RoundToInt(v.y);z=Mathf.RoundToInt(v.z); }
	
	public int sqrMagnitude { get { return Mathf.Abs(x)+Mathf.Abs(y)+Mathf.Abs(z); } }
	public IntV3 normalized { get { return new IntV3(x==0?0:x>0?1:-1,y==0?0:y>0?1:-1,z==0?0:z>0?1:-1); } }
	public IntV3 flattened { get { return new IntV3(x,0,z); } }

	public IntV3 WithX(int x) { return new IntV3(x,y,z); }
	public IntV3 WithY(int y) { return new IntV3(x,y,z); }
	public IntV3 WithZ(int z) { return new IntV3(x,y,z); }

	public void Normalize() { 
		if ( x!=0 ) x/=Mathf.Abs(x);
		if ( y!=0 ) y/=Mathf.Abs(y);
		if ( z!=0 ) z/=Mathf.Abs(z);
	}
	
#if UNITY_EDITOR
	public void EditorFields() {
		using(new GUIUtils.Horizontal() ) {
			using(new GUIUtils.LabelWidth("Y: ")) {
				x = EditorGUILayout.IntField("X: ", x);
				y = EditorGUILayout.IntField("Y: ", y);
				z = EditorGUILayout.IntField("Z: ", z);
			}
		}
	}
#endif
}