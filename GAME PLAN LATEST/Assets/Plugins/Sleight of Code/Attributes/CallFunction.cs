using UnityEngine;
using System.Collections;
using System.Reflection;

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class CallFunction : PropertyAttribute {
	public string function;
	public string buttonLabel;
	public CallFunction(string function, string buttonLabel = "") {
		this.function = function;
		this.buttonLabel = buttonLabel;
	}
	
	public void Do(object onObject) {
		if ( null == onObject ) return;
		onObject.GetType().GetMethod(function).Invoke(onObject,new object[]{});
	}
}

