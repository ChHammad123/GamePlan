using UnityEngine;
using System.Collections;
using System.Reflection;

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class DisplayIf : PropertyAttribute {
	public delegate bool Query();
	
	public enum DDIAType {
		Bool, Enum
	}
	DDIAType type;
	string variable;
	bool myBool;
	string myString;
	
	public DisplayIf(string variable, bool isTrue) {
		this.variable = variable;
		type = DDIAType.Bool;
		myBool = isTrue;
	}
	public DisplayIf(string variable, string isEnumType, bool isOrIsNot) {
		this.variable = variable;
		type = DDIAType.Enum;
		myString = isEnumType;
		myBool = isOrIsNot;
	}
	
	public bool ShouldDisplay(object onObject) {
		if ( null == onObject ) return false;
		
		FieldInfo fi = onObject.GetType().GetField(variable, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
		PropertyInfo pi = onObject.GetType().GetProperty(variable, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
		if ( null == fi && null == pi ) {
			return false;
		}
		switch(type) {
			case DDIAType.Bool:
				if ( null == pi ) {
					if ( myBool == (bool)fi.GetValue(onObject) ) return true;
				} else {
					if ( myBool == (bool)pi.GetValue(onObject,null) ) return true;
				}
				break;
			case DDIAType.Enum:
				if ( null == pi ) {
					if ( myBool == (System.Enum.GetName(fi.FieldType,fi.GetValue(onObject)) == myString) ) return true;
				} else {
					if ( myBool == (System.Enum.GetName(pi.PropertyType,pi.GetValue(onObject,null)) == myString) ) return true;
				}
				break;
		}
		return false;
	}
}

