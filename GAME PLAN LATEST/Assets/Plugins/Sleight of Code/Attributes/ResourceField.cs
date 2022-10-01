using UnityEngine;
using System.Collections;

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ResourceField : PropertyAttribute {
	public System.Type type;
	public string extension = "prefab";

	public ResourceField(System.Type type) {
		this.type = type;
	}
	public ResourceField(System.Type type, string extension) {
		this.type = type;
		this.extension = extension;
	}
}

