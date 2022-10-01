#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

public class CustomAssetUtility : MonoBehaviour {

	static T GetOrCreateScriptable<T> (string path) where T : ScriptableObject {
		if ( !Directory.Exists(Path.GetDirectoryName(path)) ) Directory.CreateDirectory(Path.GetDirectoryName(path));
		T t = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
		if ( t != null ) {
			return t;
		}
		
		T asset = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset(asset, path);
		AssetDatabase.SaveAssets();
		return asset;
	}
}


#endif