using UnityEngine;
using System.Collections;

public class AnimateMaterial : MonoBehaviour {
	public bool getMaterialFromSelf = true;

	public Animation[] animations;

	[System.Serializable]
	public class Animation {
		public string propertyName;
		public Type type;
		public Style style;
		public float variation = 1f, center = 0f, speed = 1f;

		public enum Style { Sine, Loop, Pong }
		public enum Type { Float }

		float value;

		public void Update(Material mat) {
			switch(type) {
				case Type.Float:
					switch(style) {
						case Style.Sine:
							value = center + variation * Mathf.Sin(speed * Time.realtimeSinceStartup);
							break;
						case Style.Loop:
							value += speed * Time.deltaTime;
							if ( center + value > center + variation ) value -= variation * 2f;
							if ( center - value < center - variation ) value += variation * 2f;
							break;
						case Style.Pong:
							value += speed * Time.deltaTime;
							if ( center + value > center + variation ) { value = center + value; speed *= -1f; }
							if ( center - value < center - variation ) { value = center - value; speed *= -1f; }
							break;
					}
					mat.SetFloat(propertyName, value);
					break;
			}
		}
	}

	public Material targetMaterial;

	void Awake() {
		if ( getMaterialFromSelf ) {
			targetMaterial = GetComponentInChildren<Renderer>().material;
		}
	}

	void Update() {
		foreach(Animation anim in animations) anim.Update(targetMaterial);
	}


}

