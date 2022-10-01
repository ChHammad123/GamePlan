using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine.UI;


public static class UtilityExtensions {
	public delegate U Convert<T,U>(T t);
	public delegate T OneFromTwo<T>(T a, T b);
	public delegate void ArrayDuo<T>(T a, T b);
	public delegate T Cloner<T>(T t);
	public delegate float MultiopComponent(float[] input);

	#region UI


	public static void SetSpriteNullable(this Image image, Sprite sprite) {
		image.sprite = sprite;
		if ( null == sprite ) image.color = Color.clear;
		else image.color = Color.white;
	}


	#endregion

	static string _dataPath = null; 
	public static string dataPath { get {
			if ( _dataPath == null ) _dataPath = Application.dataPath;
			return _dataPath;
		} } 

	#region Layers
	public static LayerMask LayersCollisiveWith(this int layer) {
		int ret = 0;
		for(int i = 0; i < 32; i++) {
			if ( Physics.GetIgnoreLayerCollision(layer, i) ) ret = ret & (1<<i);
		}
		LayerMask retm = new LayerMask();
		retm.value = ret;
		return retm;
	}
	#endregion

	#region Bounds
	public static Vector3 RandomInteriorPoint(this Bounds b) {
		return new Vector3(
			b.center.x + UnityEngine.Random.Range(-b.extents.x,b.extents.x),
			b.center.y + UnityEngine.Random.Range(-b.extents.y,b.extents.y),
			b.center.z + UnityEngine.Random.Range(-b.extents.z,b.extents.z)
			);
	}
	#endregion


#if UNITY_EDITOR
	#region SerializedProperty
	/// <summary>Gets an object in the class hierarchy of a property. (defaults to the parent object)
	/// class A { B b; } class B { C c; } class C { }
	/// A seriprop representing a 'C' would return:
	/// GetParent(0): the 'C'.
	/// GetParent(1): a 'B'.
	/// GetParent(2]: an 'A'.
	/// </summary>
	/// <returns>The object at the indicated level.</returns>
	/// <param name="prop">Property.</param>
	/// <param name="level">Level. 0 = this object, 1 = parent, 2 = grandparent, etc.</param>
	public static object GetParent(this SerializedProperty prop, int level = 1) {
		var path = prop.propertyPath.Replace(".Array.data[", "[");
		//		Debug.Log (path);
		object obj = prop.serializedObject.targetObject;
		var elements = path.Split('.');
		foreach(var element in elements.Take(elements.Length-level)) {
			//			Debug.Log(element);
			if(element.Contains("[")) {
				var elementName = element.Substring(0, element.IndexOf("["));
				var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[","").Replace("]",""));
				obj = GetSeriPropValue(obj, elementName, index);
				//				Debug.Log (elementName);
			} else {
				obj = GetSeriPropValue(obj, element);
			}
		}
		return obj;
	}
	
	static object GetSeriPropValue(object source, string name) {
		if(source == null) return null;
		var type = source.GetType();
		var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		if(f == null) {
			var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			if(p == null) return null;
			return p.GetValue(source, null);
		}
		return f.GetValue(source);
	}
	
	static object GetSeriPropValue(object source, string name, int index) {
		var enumerable = GetSeriPropValue(source, name) as IEnumerable;
		if ( null == enumerable ) return null;
		var enm = enumerable.GetEnumerator();
		while(index-- >= 0) if (!enm.MoveNext() ) return null;
		return enm.Current;
	}

	#endregion

	#region EditorWindow
	public static void ShowNotification(this EditorWindow ew, string notification) {
		ew.ShowNotification(new GUIContent(notification));
	}
	#endregion
#endif

	#region FieldInfo
	public static void SetArrayValue(this FieldInfo field, object owner, object valueIn, int index) {
		object oldValue = field.GetValue(owner);
		IEnumerable ie = oldValue as IEnumerable;
		object[] newValue = new object[0];
		foreach(object e in ie) newValue = newValue.With(e);
		if ( index >= newValue.Length || index < 0 ) throw new IndexOutOfRangeException(index + " is out of range of array of size " + newValue.Length);
		newValue[index] = valueIn;
		Type fieldType = field.FieldType.GetElementType();
		Array valueToSet = Array.CreateInstance(fieldType, newValue.Length);
		for(int i=0; i<newValue.Length; i++) {
			valueToSet.SetValue(Convert.ChangeType(newValue[i],fieldType),i);
		}
		field.SetValue(owner, valueToSet);
	}
	public static object GetArrayValue(this FieldInfo field, object owner, int index) {
		object value = field.GetValue(owner);
		IEnumerable ie = value as IEnumerable;
		object[] values = new object[0];
		foreach(object e in ie) values = values.With(e);
		if ( index >= values.Length || index < 0 ) throw new IndexOutOfRangeException(index + " is out of range of array of size " + values.Length);
		return values[index];
	}
	#endregion

	#region Rect
	public static Rect WithX(this Rect r, float x) {return new Rect(x,r.y,r.width,r.height);}
	public static Rect WithY(this Rect r, float y) {return new Rect(r.x,y,r.width,r.height);}
	public static Rect WithWidth(this Rect r, float width) {return new Rect(r.x,r.y,width,r.height);}
	public static Rect WithHeight(this Rect r, float height) {return new Rect(r.x,r.y,r.width,height);}
	#endregion

	#region int
	public static bool IsValidIndexFor<T>(this int index, IEnumerable<T> array) {
		if ( null == array ) return false;
		if ( index < 0 ) return false;
		return index < array.Count();
	}

	public static int BitMaskHighest(this int mask) {
		int ret = 0;
		int selfMask = mask;
		while(selfMask > 1) {ret++; selfMask = selfMask >> 1; }
		return ret;
	}
	public static int BitMaskRemove(this int mask, int value) { return mask & ~value; }
	public static int BitMaskAdd(this int mask, int value) { return mask | value; }
	public static bool BitMaskContains(this int mask, int value) { return (mask & value) >= value; }
	#endregion

	#region Dictionary
	public static void AddOrSet<T,U>(this Dictionary<T,U> dict, T key, U value) {
		if ( dict.ContainsKey(key) ) dict[key] = value;
		else dict.Add(key,value);
	}
	#endregion

	#region DirectoryInfo
#if !UNITY_WEBPLAYER
	public static string[] GetFilesByExtensions(this DirectoryInfo dirInfo, params string[] extensions) {
		var allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
		return dirInfo.GetFiles()
			.Where(f => allowedExtensions.Contains(f.Extension))
			.Select(x=>x.FullName)
			.ToArray();
	}
#endif
	#endregion
	#region Mecanim
#if UNITY_EDITOR
	public static UnityEditor.Animations.AnimatorState FindState(this UnityEditor.Animations.AnimatorStateMachine sm, string name) {
		for(int i=0; i < sm.states.Length; i++) {
			UnityEditor.Animations.AnimatorState ret = sm.states[i].state;
			if ( ret.name == name ) return ret;
		}
		return null;
	}
#endif
	#endregion
	#region ScriptableObject
#if UNITY_EDITOR
	public static T CloneScriptable<T>(this T source) where T : ScriptableObject {
		T t = ScriptableObject.CreateInstance<T>();
		EditorUtility.CopySerialized(source, t);
		return t;
	}
#endif
	#endregion
	#region Texture2D
	public static Texture2D Inverse(this Texture2D t) {
		Color[] colors = t.GetPixels();
		for (int i=0; i<colors.Length; i++) {
			colors[ i ]=colors[ i ].Inverse();
		}
		Texture2D ret=new Texture2D( t.width, t.height, t.format, false );
		return ret;
	}

	#endregion

	#region Camera
	public static float UnitHeight(this Camera camera, float z = 0f) {
		if ( camera.orthographic ) {
			return camera.orthographicSize*2f;
		} else {
			// TODO: this
		}
		return 0f;
	}
	public static float UnitWidth(this Camera camera, float z = 0f) {
		if ( camera.orthographic ) {
			return camera.UnitHeight() * Screen.width / Screen.height;
		} else {
			 //TODO: this
		}
		return 0f;
	}
	#endregion

	#region Color
	public static Color Desaturated(this Color c, int power = 1) {
		Color ret = new Color(c.r,c.g,c.b,c.a);
		while(power > 0) {
			ret.r = (1f-ret.r)/2f + ret.r;
			ret.g = (1f-ret.g)/2f + ret.g;
			ret.b = (1f-ret.b)/2f + ret.b;
			power--;
		}
		return ret;
	}

	public static bool Approximately( this Color c, Color o ) {
		bool ret = (
			Mathf.Abs(c.r-o.r) < 0.01f &&
			Mathf.Abs(c.g-o.g) < 0.01f &&
			Mathf.Abs(c.b-o.b) < 0.01f &&
			Mathf.Abs(c.a-o.a) < 0.01f 
			);
		return ret;
	}
	public static float[] AsFloatArray(this Color c) {
		return new float[]{c.r,c.g,c.b,c.a};
	}
	public static Color AsColor(this float[] f) {
		if(f.Length == 4 ) {
			return new Color(f[0],f[1],f[2],f[3]);
		}
		return new Color(f[0],f[1],f[2],1f);
	}
	public static Color Inverse(this Color c) {
		return new Color( 1f-c.r, 1f-c.g, 1f-c.b, c.a );
	}
	public static Color32 HighContrast(this Color32 c) { return (Color)(((Color32)c).HighContrast()); }
	public static Color HighContrast(this Color c) {
		float h,s,l;
		c.GetHSL(out h,out s,out l);
		h+= 0.5f; if ( h >= 1f ) h-= 1f;
		l+= 0.5f; if ( l >= 1f ) l-= 1f;
		//s+= 0.5f; if ( s >= 1f ) s-= 1f;

		return ColorFromHSL(h,s,l);
		//if ( l < 0.4f ) return Color.white;
		//if ( l > 0.6f ) return Color.black;
		//return ColorFromHSL(1f-h,s,h>0.5f?l+0.4f:l-0.4f);
	}
	public static Color WithR(this Color c, float r) {return new Color(r,c.g,c.b,c.a);}
	public static Color WithG(this Color c, float g) {return new Color(c.r,g,c.b,c.a);}
	public static Color WithB(this Color c, float b) {return new Color(c.r,c.g,b,c.a);}
	public static Color WithA(this Color c, float a) {return new Color(c.r,c.g,c.b,a);}

	// Given H,S,L in range of 0-1
	public static Color ColorFromHSL(float h, float s, float l) {
		float v,r,g,b;
		r = g = b = l;   // default to gray
		v = (l <= 0.5f) ? (l * (1.0f + s)) : (l + s - l * s);
		if (v > 0f) {
			int sextant;
			float m, sv, fract, vsf, mid1, mid2;
			
			m = l + l - v;
			sv = (v - m ) / v;
			h *= 6.0f;
			sextant = (int)h;
			fract = h - sextant;
			vsf = v * sv * fract;
			mid1 = m + vsf;
			mid2 = v - vsf;
			switch (sextant) {
				case 0:
					r = v;
					g = mid1;
					b = m;
					break;
				case 1:
					r = mid2;
					g = v;
					b = m;
					break;
				case 2:
					r = m;
					g = v;
					b = mid1;
					break;
				case 3:
					r = m;
					g = mid2;
					b = v;
					break;
				case 4:
					r = mid1;
					g = m;
					b = v;
					break;
				case 5:
					r = v;
					g = m;
					b = mid2;
					break;
			}
		}
		return new Color(r,g,b);
	}
	
	// Given a Color (RGB Struct) in range of 0-255
	// Return H,S,L in range of 0-1
	public static void GetHSL (this Color rgb, out float h, out float s, out float l) {
		float r = rgb.r, g = rgb.g, b = rgb.b;
		float v, m, vm, r2, g2, b2;

		h = s = l = 0f; // default to black
		v = Math.Max(r,g); v = Math.Max(v,b);
		m = Math.Min(r,g); m = Math.Min(m,b);
		l = (m + v) / 2.0f;
		if (l <= 0.0f) return;
		vm = v - m;
		s = vm;
		if (s > 0.0f) {
			s /= (l <= 0.5f) ? (v + m ) : (2.0f - v - m) ;
		} else {
			return;
		}
		r2 = (v - r) / vm;
		g2 = (v - g) / vm;
		b2 = (v - b) / vm;
		if (r == v) {
			h = (g == m ? 5.0f + b2 : 1.0f - g2);
		} else if (g == v) {
			h = (b == m ? 1.0f + r2 : 3.0f - b2);
		} else {
			h = (r == m ? 3.0f + g2 : 5.0f - r2);
		}
		h /= 6.0f;
	}
	
	
	#endregion
	#region Enum
	public static bool EnumMaskContains<T>(this T e, System.Enum value) {
		return e.EnumMaskContains(Convert.ToInt32(value));
	}
	public static bool EnumMaskContains<T>(this T e, int value) {
		return (Convert.ToInt32(e) & value) == value;
	}
	public static T EnumMaskAdd<T>(this T e, System.Enum value) {
		return e.EnumMaskAdd( Convert.ToInt32( value ));
		//YourEnum foo = Enum.ToObject(typeof(YourEnum) , yourInt);
	}
	public static T EnumMaskAdd<T>(this T e, int value) {
		return (T)Enum.ToObject(e.GetType(), (Convert.ToInt32( e ) | value ));
	}
	public static T EnumMaskRemove<T>(this T e, System.Enum value) {
		return e.EnumMaskRemove( Convert.ToInt32( value ) );
	}
	public static T EnumMaskRemove<T>(this T e, int value) {
		return (T)Enum.ToObject(e.GetType(), (Convert.ToInt32( e ) & ~value ));
	}
	
	#endregion
	#region Type
	public static bool Implements(this object thing, string method) {
		return thing.GetType().Implements(method);
	}
	public static bool Implements(this Type type, string method) {
		var mInfo = type.GetMethod(method);
		if ( null == mInfo ) return false;
		return (!mInfo.IsAbstract) && mInfo.DeclaringType == type;
	}
	public static bool IsAssignableTo(this Type type, Type test) {
		return test.IsAssignableFrom(type);
	}

	public static bool IsNumericType( this Type type ) {
		if (type == null) return false;
		switch (Type.GetTypeCode(type)) {
			case TypeCode.Byte:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.SByte:
			case TypeCode.Single:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return true;
			case TypeCode.Object:
				if ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
					return Nullable.GetUnderlyingType(type).IsNumericType();
				}
				return false;
		}
		return false;
	}

	public static IEnumerable<MethodInfo> GetDelegates(this Type type, Type delegateType) {
		MethodInfo mi = delegateType.GetMethod("Invoke");
		List<Type> parameters = new List<Type>();
		foreach(ParameterInfo param in mi.GetParameters()) {
			parameters.Add(param.ParameterType);
		}
		return type.GetMethodsBySig(mi.ReturnType, parameters.ToArray() );
	}
	
	public static IEnumerable<MethodInfo> GetMethodsBySig(this Type type, Type returnType, params Type[] parameterTypes)
	{
		return type.GetMethods().Where((m) =>
		                               {
			if (m.ReturnType != returnType) return false;
			var parameters = m.GetParameters();
			if ((parameterTypes == null || parameterTypes.Length == 0))
				return parameters.Length == 0;
            if (parameters.Length != parameterTypes.Length)
                return false;
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (parameters[i].ParameterType != parameterTypes[i])
                    return false;
            }
            return true;
        });
    }
	#endregion	
	#region T
	public static void Swap<T>(this T a, T b, out T newA, out T newB) {
		newA = b; newB = a;
	}

	public static object CallGenericMethod<T>(this T o, string methodName, object[] parameters, params System.Type[] types) {
		MethodInfo mi = o.GetType().GetMethods().Where(x=>x.Name==methodName).FirstOrDefault();
		if ( null == mi ) {
			Debug.Log("Couldn't match method " + methodName + " on " + o.GetType().Name);
			return null;
		}
		mi = mi.MakeGenericMethod(types);
		return mi.Invoke(o,parameters);
	}
	public static object CallGenericExtensionMethod<T>(this T o, System.Type methodSource, string methodName, object[] parameters, params System.Type[] types) {
		MethodInfo mi = methodSource.GetMethods().Where(x=>x.Name==methodName).FirstOrDefault();
		if ( null == mi ) return null;
		mi = mi.MakeGenericMethod(types);
		return mi.Invoke(o,parameters);
	}

	public static T DeserializeFromPlayerPrefs<T>(this T o, string prefix) where T : class {
		T ret = o; string result;
		object[] attributes = ret.GetType().GetCustomAttributes(false);
		if ( attributes.Any(x=>x.GetType()==typeof(ScrambleWhenSerializingToPlayerPrefsAttribute)) ) {
			result = PlayerPrefs.GetString(prefix);
			if ( result != "" ) ret = result.DeserializeToObject<T>();
			return ret;
		}
		FieldInfo[] fields = ret.GetType().GetFields();
		foreach(FieldInfo field in fields) {
			string pref = prefix + "." + field.Name;
			attributes = field.GetCustomAttributes(false);
			if ( attributes.Any(x=>x.GetType()==typeof(ScrambleWhenSerializingToPlayerPrefsAttribute))
			    && (field.FieldType.IsSerializable || typeof(ISerializable).IsAssignableFrom(field.FieldType)) ) {
				result = PlayerPrefs.GetString(pref,"");
				if ( result != "" ) {
					field.SetValue(ret,result.CallGenericExtensionMethod(typeof(UtilityExtensions),"DeserializeToObject",new object[]{result},typeof(T)));
				}
			} else {
				if ( field.FieldType == typeof(int) ) {
					field.SetValue(ret, PlayerPrefs.GetInt(pref,(int)field.GetValue(ret)));
				} else if ( field.FieldType == typeof(bool) ) {
					field.SetValue(ret, PlayerPrefs.GetInt(pref,((bool)field.GetValue(ret))?1:0)==1);
				} else if ( field.FieldType == typeof(float) ) {
					field.SetValue(ret, PlayerPrefs.GetFloat(pref,(float)field.GetValue(ret)));
				} else if ( field.FieldType == typeof(string) ) {
					field.SetValue(ret, PlayerPrefs.GetString(pref,(string)field.GetValue(ret)));
				} else {
					if ( attributes.Any(x=>x.GetType()==typeof(SerializeIndividualFieldsToPlayerPrefsAttribute)) ) {
						field.SetValue(ret, field.GetValue(ret).DeserializeFromPlayerPrefs(pref));
					} else if ( field.FieldType.IsSerializable || typeof(ISerializable).IsAssignableFrom(field.FieldType) ) {
						result = PlayerPrefs.GetString(pref,"");
						if ( result != "" ) {
							field.SetValue(ret,result.CallGenericExtensionMethod(typeof(UtilityExtensions),"DeserializeToObject",new object[]{result},field.FieldType));
						}
					} else {
						Debug.LogWarning(field.Name+ " not serialized");
						// couldn't handle this field
					}
				}
			}
//			Debug.Log("load " + pref + " = " + field.GetValue(ret).ToString());
		}
		return ret;
	}
	public static string SerializeToSummaryString<T>(this T o, int currentLevel = 0, int maxLevel = 7) {
		string prefix = "";
		for(int i=0; i < currentLevel; i++) prefix += ".";
		if ( currentLevel > maxLevel ) return prefix + "Too deep!\n";
		string ret = "";

		FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
		foreach(var field in fields) {
			ret += prefix + field.FieldType.Name + " " + field.Name + " : ";
			object value = field.GetValue(o);
			if ( null == value ) {ret += "NULL\n"; continue;}

			if ( field.FieldType == typeof(int) ) {
				ret += (int)value;
			} else if ( field.FieldType == typeof(bool) ) {
				ret += (bool)value;
			} else if ( field.FieldType == typeof(float) ) {
				ret += (float)value;
			} else if ( field.FieldType == typeof(string) ) {
				ret += (string)value;
			} else if ( field.FieldType.IsEnum ) {
				ret += value.ToString();
			} else if ( field.FieldType.IsArray ) {
				IEnumerable list = (IEnumerable)field.GetValue(o);
				foreach(var x in list) {
					ret += "\n" + x.SerializeToSummaryString(currentLevel+1);
				}
			} else if ( field.IsNotSerialized ) {
				ret += value.ToString() + " (not serialized)";
			} else {
				ret += "\n" + field.GetValue(o).SerializeToSummaryString(currentLevel+1);
			}
			ret += "\n";
		}
		return ret;
	}

	public static string SummarizeSerializationPlan(this Type type, int currentLevel = 0, int maxLevel = 7) {
		string prefix = "";
		for(int i=0; i < currentLevel; i++) prefix += ".";
		if ( currentLevel > maxLevel ) return prefix + "Too deep!\n";
		string ret = "";

		FieldInfo[] fields = type.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
		if ( fields.Length == 0 ) return "";
		foreach(var field in fields) {
			if ( field.IsNotSerialized ) continue;

			ret += prefix + field.FieldType.Name + " " + field.Name + "\n";
			if ( field.FieldType.IsPrimitive ) {
			} else if ( field.FieldType.IsArray ) {
				ret+= field.FieldType.GetElementType().SummarizeSerializationPlan(currentLevel+1);
			} else {
				ret+= field.FieldType.SummarizeSerializationPlan(currentLevel+1);
			}
		}
		return ret;
	}

	public static void SerializeToPlayerPrefs<T>(this T o, string prefix) {
		object[] attributes = o.GetType().GetCustomAttributes(false);
		if ( attributes.Any(x=>x.GetType()==typeof(ScrambleWhenSerializingToPlayerPrefsAttribute)) ) {
			PlayerPrefs.SetString(prefix,o.SerializeToString());
			return;
		}
		FieldInfo[] fields = o.GetType().GetFields();
		foreach(FieldInfo field in fields) {
			string pref = prefix + "." + field.Name;
			attributes = field.GetCustomAttributes(false);
			if ( null == field.GetValue(o) ) {
				PlayerPrefs.SetString(pref, "");
				continue;
			}
//			Debug.Log("save " + pref + " = " + field.GetValue(o).ToString());
			if ( attributes.Any(x=>x.GetType()==typeof(ScrambleWhenSerializingToPlayerPrefsAttribute))
			    && (field.FieldType.IsSerializable || typeof(ISerializable).IsAssignableFrom(field.FieldType)) ) {
				PlayerPrefs.SetString(pref, field.GetValue(o).SerializeToString());
			} else {
				if ( field.FieldType == typeof(int) ) {
					PlayerPrefs.SetInt(pref,(int)field.GetValue(o));
				} else if ( field.FieldType == typeof(bool) ) {
					PlayerPrefs.SetInt(pref,((bool)field.GetValue(o))?1:0);
				} else if ( field.FieldType == typeof(float) ) {
					PlayerPrefs.SetFloat(pref, (float)field.GetValue(o));
				} else if ( field.FieldType == typeof(string) ) {
					PlayerPrefs.SetString(pref,(string)field.GetValue(o));
				} else {
					if ( attributes.Any(x=>x.GetType()==typeof(SerializeIndividualFieldsToPlayerPrefsAttribute)) ) {
						field.GetValue(o).SerializeToPlayerPrefs(pref);
					} else if ( field.FieldType.IsSerializable || typeof(ISerializable).IsAssignableFrom(field.FieldType) ) {
						PlayerPrefs.SetString(pref, field.GetValue(o).SerializeToString());
					} else {
						Debug.LogWarning(field.Name+ " not serialized");
						// couldn't handle this field
					}
				}
			}
		}
	}

	/// <summary>Deserializes from file.</summary>
	/// <param name='target'>Target.</param>
	/// <param name='fileName'>File name.</param>
	/// <typeparam name='T'>The 1st type parameter.</typeparam>
	/// <exception cref='InvalidOperationException'>Is thrown if the type of target isn't serializable.</exception>
	public static bool DeserializeFromFile<T>(ref T target, string fileName, bool prependDatapath=true) {
		if ( !typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T)) ) ) {
			throw new System.InvalidOperationException(typeof(T).Name + " is not serializable");
		}
		string file = (prependDatapath?dataPath+"/":"")+fileName;
		if ( !File.Exists(file) ) {
			Debug.Log("File " + file + " doesn't exist");
			return false;
		} 
		FileStream inStr = null;
		//try {
			inStr = new FileStream(file, FileMode.Open);
			BinaryFormatter bf = new BinaryFormatter();
			target = (T)bf.Deserialize(inStr);
			inStr.Close();
		//} catch (System.Exception e) {
		//	Debug.Log("Could not load " + typeof(T).Name + " from " + fileName + " ("+e.Message+")");
		//	if ( null != inStr ) inStr.Close();
		//	return false;
		//
		return true;
	}
	public static T DeserializeFromFile<T>(T target, string fileName, bool prependDatapath=true) {
		if ( !typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T)) ) ) {
			throw new System.InvalidOperationException(typeof(T).Name + " is not serializable");
		}
		string file = (prependDatapath?dataPath+"/":"")+fileName;
		FileStream inStr = null;
		T ret = default(T);
		if ( !File.Exists(file) ) {
			Debug.Log(file + " doesn't exist, returning default");
			return ret;
		}
		try {
			inStr = new FileStream(file, FileMode.Open);
			BinaryFormatter bf = new BinaryFormatter();
			ret = (T)bf.Deserialize(inStr);
			inStr.Close();
		} catch (System.Exception e) {
			Debug.Log("Could not load " + typeof(T).Name + " from " + fileName + " ("+e.Message+")");
			if ( null != inStr ) inStr.Close();
		}
		return ret;
	}

	public static void DeserializeFromResource<T>(ref T target, string resource) {
		TextAsset asset = (TextAsset)Resources.Load(resource,typeof(TextAsset));

		if ( null == asset ) {
			Debug.LogError("Couldn't load from " + resource);
		} else {
			DeserializeFromBytes(ref target, asset.bytes);
		}
	}

	public static void DeserializeFromBytes<T>(ref T target, byte[] bytes) {
		BinaryFormatter bf = new BinaryFormatter();
		using(MemoryStream ms = new MemoryStream(bytes)) {
			target = (T)bf.Deserialize(ms);
		}
	}

	public static string SerializeToString<T>(this T serialize) {
		if ( !typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T)) ) ) {
			throw new InvalidOperationException(typeof(T).Name + " is not serializable");
		}
		string ret;
		using(MemoryStream ms = new MemoryStream()) {
			new BinaryFormatter().Serialize(ms, serialize);
			ret = Convert.ToBase64String(ms.ToArray());
		}
		return ret;
	}
	public static T DeserializeToObject<T>(this string str) where T : class {
		if ( !typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T)) ) ) {
			throw new InvalidOperationException(typeof(T).Name + " is not serializable");
		}
		byte[] bytes = Convert.FromBase64String(str);
		using(MemoryStream ms = new MemoryStream()) {
			ms.Write(bytes,0,bytes.Length);
			ms.Position = 0;
			return new BinaryFormatter().Deserialize(ms) as T;
		}
	}

	public static void SerializeToFile<T>(this T serialize, string fileName, bool prependDataPath=true) {
		if ( !typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T)) ) ) {
			throw new InvalidOperationException(typeof(T).Name + " is not serializable");
		}
		
		if(ReferenceEquals(serialize, null)) {
			throw new InvalidOperationException("Cannot serialize null " + typeof(T).Name);
		}
		if ( prependDataPath ) fileName = dataPath+"/"+fileName;
		System.IO.Directory.CreateDirectory( Path.GetDirectoryName(fileName) );
		try {
			using(FileStream fs = new FileStream(fileName, FileMode.Create)) {
			BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(fs, serialize);
			}
		} catch ( Exception e ) {
			Debug.LogError(e.Message);
		}
	}
	
	
	public static T Clone<T>(this T source)
	{
		if ( !typeof(T).IsSerializable && !(typeof(ISerializable).IsAssignableFrom(typeof(T)) ) ) {
			throw new InvalidOperationException(typeof(T).Name + " is not serializable");
		}
		
		if(ReferenceEquals(source,null))
			return default(T);
		
		IFormatter formatter = new BinaryFormatter();
		using(MemoryStream serializeStream = new MemoryStream())
		{
			formatter.Serialize(serializeStream,source);
			serializeStream.Seek(0,SeekOrigin.Begin);
			return(T) formatter.Deserialize(serializeStream);
		}
	}
	#endregion	
	#region ValueType multioverrides
	public static float Squared(this float a) { return a*a; }
	public static int   Squared(this int a) { return a*a; }

	public static bool Near(this float t, float target, float range) {
		return Mathf.Abs(t-target) <= range;
	}

	public static bool BetweenExclusive(this int t, int a, int b) {
		if ( a < b ) {
			return a < t && t < b;
		} else {
			return b < t && t < a;
		}
	}
	public static bool BetweenExclusive(this float t, float a, float b) {
		if ( a < b ) {
			return a < t && t < b;
		} else {
			return b < t && t < a;
		}
	}

	/// <summary>True if value is between the specified a and b (inclusive).</summary>
	public static bool BetweenInclusive(this int t, int a, int b) {
		if ( a < b ) {
			return a <= t && t <= b;
		} else {
			return b <= t && t <= a;
		}
	}
	/// <summary>True if value is between the specified a and b (inclusive).</summary>
	public static bool BetweenInclusive(this float t, float a, float b) {
		if ( a < b ) {
			return (a < t || Mathf.Approximately(a,t)) && (t < b || Mathf.Approximately(b,t));
		} else {
			return (b < t || Mathf.Approximately(b,t)) && (t < a || Mathf.Approximately(a,t));
		}
	}
	
	public static float[] MemberwiseSubtract(this float[] arr, float[] sub) {
		for(int i=0; i<arr.Length; i++) {
			arr[i]-=sub[i];
		}
		return arr;
	}
	public static float[] MemberwiseAdd(this float[] arr, float[] ad) {
		for(int i=0; i<arr.Length; i++) {
			arr[i]+=ad[i];
		}
		return arr;
	}
	public static int[] MemberwiseSubtract(this int[] arr, int[] ad) {
		if ( null == ad ) throw new Exception("Array to subtract cannot be null");
		if ( arr.Length != ad.Length ) throw new Exception("Array lengths do not match ("+arr.Length+","+ad.Length+")");
		for(int i=0; i<arr.Length; i++) {
			arr[i]-=ad[i];
		}
		return arr;
	}
	public static int[] MemberwiseAdd(this int[] arr, int[] ad) {
		if ( null == ad ) throw new Exception("Array to add cannot be null");
		if ( arr.Length != ad.Length ) throw new Exception("Array lengths do not match ("+arr.Length+","+ad.Length+")");
		for(int i=0; i<arr.Length; i++) {
			arr[i]+=ad[i];
		}
		return arr;
	}
	#endregion	
	#region T[]
	public static void Swap<T>(this T[] array, int index1, int index2) {
		T temp = array[index1];
		array[index1] = array[index2];
		array[index2] = temp;
	}
	public static T RandomElement<T>(this IEnumerable<T> array) {
		if ( array.Count() < 1 ) return default(T);
		return array.ElementAt(UnityEngine.Random.Range(0,array.Count()));
	}

	public static T IndexModLength<T>(this T[] array, int index) {
		if ( array.Length < 1 ) return default(T);
		while(index < 0) index += array.Length;
		return array[index%array.Length];
	}

	public static T[] Fill<T>(this T[] array, T with) {
		for(int i=0; i < array.Length; i++) {
			array[i] = with;
		}
		return array;
	}

	public static U Combine<T,U>(this T[] array, U spacer, Convert<T,U> converter, OneFromTwo<U> adder) {
		U ret = converter(default(T));
		if ( array.Length < 1 ) return ret;
		for(int i=0; i<array.Length-1; i++) ret = adder(ret, adder(converter(array[i]),spacer));
		ret = adder(ret, adder(converter(array[array.Length-1]),spacer));
		return ret;
	}
	public static bool ContainsElement<T>(this T[] arr, T element) {
		return System.Array.IndexOf(arr,element) > -1;
	}
	public static void Memberwise<T>(this T[] arr, T[] other, ArrayDuo<T> function) {
		for(int i=0; i<arr.Length && i<other.Length; i++) {
			function(arr[i], other[i]);
		}
	}

	public static bool MemberwiseEquals<T>(this T[] arr, T[] other) {
		if ( null == arr && null == other ) return true;
		if ( null == arr ) return false;
		if ( null == other ) return false;
		if ( arr.Length != other.Length ) return false;
		for(int i=0; i<arr.Length; i++) {
			if ( !Comparer<T>.Equals(arr[i],other[i]) ) return false;
		}
		return true;
	}


	public static T[] CloneCopy<T>(this T[] arr, Cloner<T> cloner) {
		return arr.Select(x=>cloner(x)).ToArray();
	}

	public static T[] CloneCopy<T>(this T[] arr) {
		return arr.Select(x=>Clone(x)).ToArray();
	}

	public static T[] Copy<T>(this T[] arr) {
		return new List<T>(arr).ToArray();
	}

	// FIXME: be very careful with usage of this
	/// <summary>Appends to an array using List::AddRange.\nMay be a GC menace.</summary>
	public static T[] With<T>(this T[] arr, params T[] other) {
		List<T> l = new List<T>(arr);
		l.AddRange(other);
		return l.ToArray();
	}
	
	public static T[] WithoutAll<T>(this T[] arr, params T[] remove) {
		List<T> l = new List<T>(arr);
		foreach(T t in arr) { if ( remove.Contains(t) ) l.Remove(t); }
		return l.ToArray();
	}
	public static T[] Without<T>(this T[] arr, params T[] elements) {
		List<T> l = new List<T>(arr);
		foreach(T t in elements) l.Remove(t);
		return l.ToArray();
	} 
	public static T[] WithoutIndices<T>(this T[] arr, params int[] indices) {
		List<T> l = new List<T>();
		for(int i=0; i < arr.Length; i++) {
			if ( !indices.ContainsElement(i) ) {
				l.Add(arr[i]);
			}
		}
		return l.ToArray();
	}
	public static T[] WithInsertion<T>(this T[] arr, int atIndex, params T[] insert) {
		List<T> l = new List<T>();
		for(int i = 0; i < atIndex; i++) {
			l.Add (arr[i]);
		}
		l.AddRange(insert);
		for(int i=atIndex; i<arr.Length; i++) {
			l.Add (arr[i]);
		}
		return l.ToArray();
	}
	#endregion	
	#region List
	public static void AddIfNotIn<T>( this List<T> arr, T arg ) {
		if ( !arr.Contains( arg ) ) arr.Add( arg );
	}
	#endregion
	#region string
	/// <summary>Returns the substring from one index to another, inclusive.</summary>
	/// <param name="str">String.</param>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	public static string SubstringFromTo(this string str, int from, int to) {
		return str.Substring(from, to-from+1);
	}
	/// <summary>Returns the position of the character immediately after the 'find' string within this string.
	/// Returns -1 if find was not found.</summary>
	/// <returns>The of end.</returns>
	/// <param name="str">String to search.</param>
	/// <param name="find">String to find.</param>
	public static int IndexOfEnd(this string str, string find) {
		int index = str.IndexOf(find);
		if ( index == -1 ) return -1;
		return index + find.Length;
	}
#if UNITY_EDITOR
	public static string EditAsResource(this string str, Type type, string label, string extension="prefab", bool simple = false) { return str.EditAsResource(type, new GUIContent(label), extension, simple); }
	public static string EditAsResource(this string str, Type type, GUIContent label, string extension="prefab", bool simple = false) {
		return (string)
			typeof(UtilityExtensions)
			.GetMethod("EditAsResource",new Type[]{typeof(string),typeof(GUIContent),typeof(string),typeof(bool)})
			.MakeGenericMethod(type)
			.Invoke(null, new object[]{str,label,extension,simple});
	}

	public static string EditAsResource<T>(this string str, string label, string extension="prefab", bool simple=false) where T : UnityEngine.Object { return str.EditAsResource<T>(new GUIContent(label), extension,simple); }
	public static string EditAsResource<T>(this string str, GUIContent label, string extension="prefab", bool simple=false) where T : UnityEngine.Object {
		// Hey! Listen!
		// Using "As T" instead of "(T)" causes this to stop working!
		// Why? Good question!
		T thing = null;
		if ( null == str ) {
			str = "";
			thing = null;
		} else {
			thing = (T)Resources.Load(str,typeof(T));
		}
		T oldThing = thing;
		if ( simple ) {
			thing = (T)EditorGUILayout.ObjectField(label,thing,typeof(T),false);
			if ( thing && !thing.IsResource() ) {
				thing = oldThing;
				Debug.LogError ("Only Resources are allowed in this field.");
			}
		} else {
			GUIUtils.ProjectPicker(ref thing, label, null, extension, "", true);
		}
		if ( oldThing != thing ) {
			str = thing.GetResourcePath();
//			Resources.UnloadAsset(oldThing);
		}
		return str;
	}

	public static string GetAssetPath(this UnityEngine.Object thing) {
		if ( null == thing ) return "Null";
		string path = AssetDatabase.GetAssetPath(thing);
		if ( path.Length < 1 ) return "Not a prefab";
		return path;
	}
	public static string GetResourcePath(this UnityEngine.Object thing) {
		if ( null==thing ) return "Null";
		string path = AssetDatabase.GetAssetPath(thing);
		if ( path.Length < 1 ) return "Not a prefab";
		int nameStart = path.LastIndexOf("Resources/")+10;
		if ( nameStart < 1 ) return "Not a resource";
		if ( !path.Contains(".") ) return "Not a file";
		path = path.Substring(nameStart,path.LastIndexOf('.')-nameStart);
		return path;
	}
	public static bool IsResource(this UnityEngine.Object thing) {
		if ( null == thing ) return false;
		string path = AssetDatabase.GetAssetPath(thing);
		if ( path.Length < 1 ) return false;
		int nameStart = path.LastIndexOf("Resources/")+10;
		if ( nameStart < 10 ) return false;
		return true;
	}
#endif

	public static string AsAcronym(this string text) {
		System.Text.StringBuilder sb = new System.Text.StringBuilder(text.Length);
		for(int i=0; i<text.Length;i++) {
			if ( char.IsUpper(text[i]) ) sb.Append(text[i]);
		}
		return sb.ToString();
	}

	public static string AsSpacedCamelCase(this string text) {
		if ( string.IsNullOrEmpty(text) ) return text;
		System.Text.StringBuilder sb = new System.Text.StringBuilder(text.Length*2);
		sb.Append(char.ToUpper(text[0]));
		for(int i=1; i<text.Length;i++) {
			if ( char.IsUpper(text[i]) && text[i-1] != ' ' )
				sb.Append(' ');
			sb.Append (text[i]);
		}
		return sb.ToString();
	}

	public static string GetFileNameAndExtensionOnly(this string str) {
		int slash = str.LastIndexOfAny(new char[]{Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar });
		int dot = str.Length;
		return str.Substring(slash+1,dot-slash-1);
	}
	public static string GetFileNameOnly(this string str) {
		int slash = str.LastIndexOfAny(new char[]{Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar });
		int dot = str.LastIndexOf('.');
		return str.Substring(slash+1,dot-slash-1);
	}
	
	public static string Without(this string s, string remThis) {
		int i = s.IndexOf(remThis);
		if ( i < 0 ) return s;
		string ret = s;
		ret = ret.Remove(i,remThis.Length);
		return ret;
	}
	public static T LoadResource<T>(this string str, bool silent=false) where T : UnityEngine.Object {
		UnityEngine.Object ret = Resources.Load(str);
		if ( null == ret ) {
			if ( !silent ) Debug.LogError("Could not load " + typeof(T).Name+ " resource at " + str);
			return null;
		}
		if ( null == (ret as T) ) {
			if ( ret is GameObject && typeof(T).IsSubclassOf(typeof(Component)) ) {
				ret = (ret as GameObject).GetComponent(typeof(T).Name) as T;
				if ( null == ret && !silent ) Debug.LogError("Loaded resource at " + str + " but it does not have a " + typeof(T).Name + " component");
			} else {
				if ( !silent ) Debug.LogError(str + " is not a " + typeof(T).Name + "; it is a " + ret.GetType().Name);
			}
		}
		return ret as T;
	}

	public static UnityEngine.Object InstantiateAsResource(this string str, System.Type type) {
		return (UnityEngine.Object)
			typeof(UtilityExtensions)
				.GetMethod("InstantiateAsResource",new Type[]{typeof(string)})
				.MakeGenericMethod(type)
				.Invoke(null, new object[]{str});
	}
	public static UnityEngine.Object InstantiateAsResource(this string str, System.Type type, Vector3 at, Quaternion rot) {
		return (UnityEngine.Object)
			typeof(UtilityExtensions)
				.GetMethod("InstantiateAsResource",new Type[]{typeof(string),typeof(Vector3),typeof(Quaternion)})
				.MakeGenericMethod(type)
				.Invoke(null, new object[]{str,at,rot});
	}
	public static T InstantiateAsResource<T>(this string str) where T : Component {
		return str.InstantiateAsResource<T>(Vector3.zero, Quaternion.identity);
	}
	public static T InstantiateAsResource<T>(this string str, Vector3 at, Quaternion rot) where T : Component {
		if ( string.IsNullOrEmpty(str) ) return null;
		try {
			return ((GameObject)GameObject.Instantiate(Resources.Load(str),at,rot)).GetComponent<T>();
		} catch ( System.Exception e ) {
			Debug.LogError("Error instantiating " + str + ": " + e.Message);
			return null;
		}
	}
	#endregion	
	#region GUISkin
	private static Dictionary<GUISkin,Vector2> previewscrolls = new Dictionary<GUISkin, Vector2>();
	public static void PreviewStyles(this GUISkin skin) {
		if (!previewscrolls.ContainsKey(skin)) previewscrolls.Add(skin,Vector2.zero);
		previewscrolls[skin] = GUILayout.BeginScrollView(previewscrolls[skin]);
		foreach(GUIStyle gs in skin.customStyles) {
			GUILayout.BeginHorizontal();
				GUILayout.TextField(gs.name, GUILayout.Width(250));
				GUILayout.Box("text TEXT text",gs);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
	}
	#endregion	
	#region Vector2
	public static Vector2 WithX(this Vector2 v2, float x) { return new Vector2(x,v2.y); }
	public static Vector2 WithY(this Vector2 v2, float y) { return new Vector2(v2.x,y); }
	#endregion
	#region Vector3
	public static Vector3 Scaled(this Vector3 v, Vector3 by) {
		Vector3 canScale = new Vector3(v.x, v.y, v.z);
		canScale.Scale(by);
		return canScale;
	}
	public static Vector3 RandomEach(this Vector3 v3) {
		return new Vector3(UnityEngine.Random.Range(-v3.x,v3.x), UnityEngine.Random.Range(-v3.y,v3.y), UnityEngine.Random.Range(-v3.z,v3.z));
	}
	public static string ToDetailString(this Vector3 v) {
		return v.x + ", " + v.y + ", " + v.z;
	}
	public static string ToPrettyString(this Vector3 v, string name="", int decimals=2) { //ASH
		if (name != "")
			return string.Format(name + "=" + "({0:F" + decimals + "}, {1:F" + decimals + "}, {2:F" + decimals + "})", v.x, v.y, v.z);
		else
			return string.Format("({0:F" + decimals + "}, {1:F" + decimals + "}, {2:F" + decimals + "})", v.x, v.y, v.z);
	}
	public static Vector3 Rotate(this Vector3 v, Vector3 euler) {
		Quaternion q = Quaternion.Euler(euler);
		return q * v;
	}

	public static Vector3 SwapXY(this Vector3 v) {return new Vector3(v.y,v.x,v.z);}
	public static Vector3 SwapYZ(this Vector3 v) {return new Vector3(v.x,v.z,v.y);}
	public static Vector3 SwapXZ(this Vector3 v) {return new Vector3(v.z,v.y,v.x);}

	public static Vector3 MostSignificantAxis(this Vector3 inp) {
		if ( Mathf.Abs(inp.x) > Mathf.Abs(inp.y) ) {
			if ( Mathf.Abs(inp.x) > Mathf.Abs(inp.z) ) {
				return Vector3.right * inp.x;
			} else {
				return Vector3.forward * inp.z;
			}
		}
		if ( Mathf.Abs(inp.y) > Mathf.Abs(inp.z) ) {
			return Vector3.up * inp.y;
		} else {
			return Vector3.forward * inp.z;
		}
	}
	public static Vector3 Multiop( MultiopComponent perComponent, params Vector3[] input) {
		return new Vector3(
			perComponent(input.Select(x=>x.x).ToArray()),
			perComponent(input.Select(x=>x.y).ToArray()),
			perComponent(input.Select(x=>x.z).ToArray())
			);
	}

	/// <summary>Get information about the nearest point on a line from this point.</summary>
	/// <returns>The distance from this point to the nearest point on the line.</returns>
	/// <param name='p'>This point.</param>
	/// <param name='a'>One point of the line.</param>
	/// <param name='b'>The second point of the line.</param>
	/// <param name='t'>Ref: Is set to where on the line the closest point is. 0.0==at A, 1.0==at B, 2.0==at B+(B-A)</param>
	/// <param name='closestPoint'>The closest point on the line.</param>
	public static float DistanceToLine(this Vector3 p, Vector3 a, Vector3 b, out float t, out Vector3 closestPoint) {
		Vector3 
			a2p = p-a,
			a2b = b-a;
		float 
			a2b2 = a2b.sqrMagnitude,
			dot = Vector3.Dot(a2p,a2b);

		t=dot/a2b2;
		closestPoint = a+a2b*t;

		float distance = (closestPoint-p).magnitude;
		return distance;
	}
	/// <summary>Same as DistanceToLine but you provide different information. Faster, for use over multiple points against same line.</summary>
	/// <param name="p">Point.</param>
	/// <param name="a">The start of the line to check.</param>
	/// <param name="a2b">The magnitude of the line to check.</param>
	/// <param name="t">Ratio along a2b on which closest point falls.</param>
	/// <param name="closestPoint">Closest point.</param>
	public static float DistanceToLinePrecalc(this Vector3 p, Vector3 a, Vector3 a2b, out float t, out Vector3 closestPoint) {
		float 
			a2b2 = a2b.sqrMagnitude,
			dot = Vector3.Dot(p-a,a2b);

		t=dot/a2b2;
		closestPoint = a+a2b*t;
		float distance = (closestPoint-p).magnitude;
		return distance;
	}

#if UNITY_EDITOR
	public static Vector3 Edit(this Vector3 v) {
		Vector3 ret;
		using(new GUIUtils.Horizontal() ){
			GUILayout.Label("X",GUILayout.ExpandWidth(false));
			float x = EditorGUILayout.FloatField(v.x);
			GUILayout.Label("Y",GUILayout.ExpandWidth(false));
			float y = EditorGUILayout.FloatField(v.y);
			GUILayout.Label("Z",GUILayout.ExpandWidth(false));
			float z = EditorGUILayout.FloatField(v.z);
			ret = new Vector3(x,y,z);
		}
		return ret;
	}
#endif
	public static Vector3 WithX(this Vector3 v, float x) {
		return new Vector3(x,v.y,v.z);
	}
	public static Vector3 WithY(this Vector3 v, float y) {
		return new Vector3(v.x,y,v.z);
	}
	public static Vector3 WithZ(this Vector3 v, float z) {
		return new Vector3(v.x,v.y,z);
	}
	#endregion
	#region UnityEngine.Object
	public static T Instantiate<T>(this T t) where T : UnityEngine.Object {
		return t.Instantiate(Vector3.zero, Quaternion.identity);
	}
	public static T Instantiate<T>(this T t, Vector3 position) where T : UnityEngine.Object {
		return t.Instantiate(position, Quaternion.identity);
	}
	public static T Instantiate<T>(this T t, Quaternion rotation) where T : UnityEngine.Object {
		return t.Instantiate(Vector3.zero, rotation);
	}
	public static T Instantiate<T>(this T t, Vector3 position, Quaternion rotation) where T : UnityEngine.Object {
		if ( null == t ) {
			Debug.LogError("Cannot instaniate 'null'!");
			return null;
		}
		if ( t is GameObject || t is Component ) {
			return (T)GameObject.Instantiate(t,position,rotation);
		} else {
			Debug.LogError("Cannot instantiate an object of type " + typeof(T));
		}
		return null;
	}
	#endregion

	#region Transform
	public static void LookRightAt(this Transform t, Vector3 direction) {
		Quaternion rot = Quaternion.FromToRotation(t.right, direction);
		t.rotation = rot * t.rotation;
	}
	public static void LookUpAt(this Transform t, Vector3 direction) {
		Quaternion rot = Quaternion.FromToRotation(t.up, direction);
		t.rotation = rot * t.rotation;
	}

	/// <summary>
	/// Childrens the type of the of. Works on prefab hierarchies.
	/// </summary>
	/// <returns>The of type.</returns>
	/// <param name="t">T.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T[] ChildrenOfType<T>(this Transform t) where T : Component {
		List<Transform> Q=new List<Transform>();
		List<T> ret=new List<T>();
		Q.Add( t );
		T x = t.GetComponent<T>();
		if ( null!=x ) ret.Add(x);

		while ( Q.Count > 0 ) {
			Transform p=Q[ 0 ];
			Q.RemoveAt( 0 );
			foreach (Transform c in p) {
				Q.Add( c );
				x=c.GetComponent<T>();
				if ( null!=x ) ret.Add( x );
			}
		}

		return ret.ToArray();
	}
	public static Transform DescendantByName(this Transform p, string name) {
		foreach(Transform c in p) {
			if(c.name==name) return c;
			Transform ret = c.DescendantByName(name);
			if ( null!= ret ) return ret;
		}
		return null;
	}
	#endregion
	#region Component
	public static T GetOrAddComponent<T>(this Component c) where T : Component {
		if ( null == c ) {
			throw new NullReferenceException();
		}
		T ret = c.GetComponent<T>();
		if ( null == ret ) ret = c.gameObject.AddComponent<T>();
		return ret;
	}
	public static IEnumerator DestroyOverTime(this Component c) { yield return DestroyOverTime(c.gameObject); }
	public static IEnumerator DestroyOverTime(this GameObject go) {
		go.SetActive(false);
		Transform[] list = go.GetComponentsInChildrenTiered<Transform>();
		for(int i=list.Length-1; i>=0; i--) {
			if ( list[i] ) {
				GameObject.Destroy(list[i].gameObject);
			}
			yield return null;
		}
	}

	public static T GetComponentInPrefabChildren<T>(this Component c) where T : Component {
		return (T)c.gameObject.GetComponentInPrefabChildren(typeof(T));
	}
	public static T GetComponentInPrefabChildren<T>(this GameObject go) where T : Component {
		return (T)go.GetComponentInPrefabChildren(typeof(T));
	}
	public static Component GetComponentInPrefabChildren(this GameObject go, System.Type type) {
		Transform[] Q = new Transform[]{go.transform};
		while(Q.Length > 0) {
			Transform cur = Q[0]; Q = Q.WithoutIndices(0);
			Component t = cur.GetComponent(type);
			if ( null != t ) return t;
			foreach(Transform tx in cur) {
				Q = Q.With(tx);
			}
		}
		return null;
	}

	public static T[] GetComponentsInChildrenTiered<T>(this Component c) where T : Component {return c.gameObject.GetComponentsInChildrenTiered<T>();}
	public static T[] GetComponentsInChildrenTiered<T>(this GameObject go) where T : Component {
		return go.GetComponentsInChildrenTiered(typeof(T)).Select(x=>x as T).ToArray();
	}
	public static Component[] GetComponentsInChildrenTiered(this GameObject go, System.Type type) {
		Component[] ret = new Component[0];
		Transform[] Q = new Transform[]{go.transform};
		while(Q.Length > 0) {
			Transform cur = Q[0]; Q = Q.WithoutIndices(0);
			Component t = cur.GetComponent(type);
			if ( null != t ) ret = ret.With(t);
			foreach(Transform tx in cur) {
				Q = Q.With(tx);
			}
		}
		return ret;
	}
	public static void SetLayerOfHierarchy(this GameObject go, int layer) {
		foreach(Transform t in go.transform.ChildrenOfType<Transform>()) {
			t.gameObject.layer = layer;
		}
	}
	public static void SetLayerOfHierarchy(this Component c, int layer) {SetLayerOfHierarchy(c.gameObject, layer);}

	public static bool IsChildOf<T,U>(this T c, U parent) where T : Component where U : Component {
		return c.IsChildOf(parent.gameObject);
	}
	public static bool IsChildOf<T>(this T c, GameObject parent) where T : Component {
		Transform t = c.transform;
		Transform p = parent.transform;
		while(t != null) {
			if ( t == p ) return true;
			t = t.parent;
		}
		return false;
	}

	public static void CopyState<T,U>(this T des, U sourc, bool position=true, bool rotation=true, bool scale=true, bool parent=false) where T : Component where U : Component {
		if ( null == des ) { Debug.LogError("Null destination"); return; }
		if ( null == sourc ) { Debug.LogError("Null source"); return; }

		Transform dest = des.transform, source = sourc.transform;
		if ( position ) dest.transform.position = source.position;
		if ( rotation ) dest.rotation = source.rotation;
		if ( scale ) {
			Transform oldParent = dest.parent;
#if UNITY_4_6
			dest.SetParent(source.parent);
			dest.localScale = source.localScale;
			dest.SetParent(oldParent);
#else
			dest.parent = source.parent;
			dest.localScale = source.localScale;
			dest.parent = oldParent;
#endif
		}
		if ( parent ) dest.parent = source.parent;
	}
	public static void CopyAndParentTo<T>(this T child, Transform parent, bool position=true, bool rotation=true, bool scale=false) where T : Component {
		if ( null == child ) { Debug.LogError("Null child"); return; }
		if ( null == parent ) { Debug.LogError("Null parent"); return; }

		child.CopyState(parent, position, rotation, scale, false);
		child.transform.parent = parent;
	}
	public static T GetComponentInParents<T>(this GameObject go) where T : Component {
		Transform c = go.transform;
		while(null != c) {
			if ( null != c.GetComponent<T>() ) return c.GetComponent<T>();
			c = c.parent;
		}
		return null;
	}
	public static T GetComponentInParents<T>(this Component child) where T : Component {
		return GetComponentInParents<T>(child.gameObject);
	}
	public static T[] GetComponentsInParents<T>(this T child) where T : Component {
		Transform c = child.transform;
		T[] ret = new T[0];
		while(null != c) {
			if ( null != c.GetComponent<T>() ) ret = ret.With(c.GetComponent<T>());
			c = c.parent;
		}
		return ret;
	}
	#endregion
}

public class SerializeIndividualFieldsToPlayerPrefsAttribute : Attribute {}
public class ScrambleWhenSerializingToPlayerPrefsAttribute : Attribute {}