using UnityEngine;
using System.Collections;
using System.Reflection;

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ChooseStringFromList : PropertyAttribute {
	public string property;
	public bool allowAdditions;
	public ChooseStringFromList(string property, bool allowAdditions = false) {
		this.property = property;
		this.allowAdditions = allowAdditions;
	}

	public string[] GetList(object onObject) {
		if ( null == onObject ) return new string[0];
		
		PropertyInfo pi = onObject.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
		if ( null == pi ) {
			return new string[0];
		}
		object value = pi.GetValue(onObject,null);
		if ( !(value is string[]) ) return new string[0];
		return (string[])value;
	}
}

