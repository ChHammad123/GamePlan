using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SleightOfCode : MonoBehaviour {
	public delegate void Empty();

	public static event Empty onScreenSizeChanged;

	public static Coroutine StartFloatingCoroutine(IEnumerator routine) {
		return instance.StartCoroutine(routine);
	}

	static SleightOfCode instance;
	static SleightOfCode() {
		instance = new GameObject("Sleight of Code").AddComponent<SleightOfCode>();
		DontDestroyOnLoad(instance);
	}

	Vector2 lastScreenSize;

	void Start() {
		lastScreenSize = new Vector2(Screen.width, Screen.height);
	}
	void Update() {
		if ( lastScreenSize != new Vector2(Screen.width, Screen.height) ) {
			if ( null != onScreenSizeChanged ) onScreenSizeChanged();
			lastScreenSize = new Vector2(Screen.width, Screen.height);
		}
	}


#if UNITY_EDITOR
	/// <summary>Gets the type of the all objects in scene of.
	/// THIS. IS. RIDICULOUSLY. SLOW.</summary>
	public static List<T> GetAllObjectsInSceneOfType<T>() where T : Component {
		var ret = new List<T>();
		foreach(var go in Resources.FindObjectsOfTypeAll<GameObject>()) {
			var component = go.GetComponent<T>();
			if ( !component ) continue;

			if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
				continue;
			
			var assetPath = AssetDatabase.GetAssetPath(go.transform.root.gameObject);
			if (!string.IsNullOrEmpty(assetPath))
				continue;


			ret.Add(component);
		}
		return ret;
	}
#endif

}
