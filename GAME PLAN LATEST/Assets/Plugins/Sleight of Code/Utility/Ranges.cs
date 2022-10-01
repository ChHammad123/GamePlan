using UnityEngine;
using System.Collections;

[System.Serializable]
public class IntRange {
	public int min, max;
}

[System.Serializable]
public class FloatRange {
	public float min = 0f, max = 0f;
	public FloatRange(){}
	public FloatRange(float i, float a) { min = i; max = a; }
}