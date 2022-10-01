using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Translator : SleightScriptable {
	static Translator() {
		Language = SystemLanguage.English;
	}
	public static SystemLanguage Language {
		get { return _language; }
		set {
			if ( _language == value ) return;
			_language = value;
			if ( null != onLanguageChanged ) onLanguageChanged();
			/* TODO
			if ( !UtilityExtensions.DeserializeFromFile(ref currentDictionary,"Translations/"+value.ToString()+".dict") ) {
				Debug.Log("Failed to load " + _language + " dictionary");
				currentDictionary = new Translation();
				meta = new MetaData();
			} else {
				#if UNITY_EDITOR
				if ( !UtilityExtensions.DeserializeFromFile(ref meta, "Translations/meta.data") ) {
					meta = new MetaData();
					meta.data = new TranslationData[currentDictionary.strings.Length];
					for(int i=0; i<meta.data.Length; i++) meta.data[i] = new TranslationData();
				}
				#endif
			}
			/**/
		}
	}	

	delegate void Callback();
	static event Callback onLanguageChanged;

	static SystemLanguage _language;

	public TextAsset[] languageFiles = null;
	Translation currentDictionary;

	bool isInitialized = false;

#if UNITY_EDITOR
	[CallFunction("ReloadLanguage")]
	public TextAsset metaText;
	MetaData meta;

	public string[] Categories { 
		get {
			if ( !isInitialized ) Initialize();
			return meta.categories;
		}
	}
	public string[] AllStrings {
		get {
			if ( !isInitialized ) Initialize();
			return currentDictionary.strings;
		}
	}
	

#endif

	void Initialize() {
		if ( isInitialized ) return;
		isInitialized = true;
		onLanguageChanged += ReloadLanguage;
		ReloadLanguage();
	}

	public void ReloadLanguage() {
#if UNITY_EDITOR
		if ( null == languageFiles || languageFiles.Length < 1 ) {
			languageFiles = new TextAsset[System.Enum.GetNames(typeof(SystemLanguage)).Length];
			string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + "/" + name + "_data/";
			Directory.CreateDirectory(path);
			currentDictionary = new Translation();
			for(int i=0; i<languageFiles.Length; i++) {
				string file = path + ((SystemLanguage)i).ToString() + ".txt";
				try {
					currentDictionary.SerializeToFile(file,false);
				} catch ( System.Exception e ) {
					Debug.LogError(e.Message);
				}
				languageFiles[i] = (TextAsset)AssetDatabase.LoadAssetAtPath(file,typeof(TextAsset));
			}
			meta = new MetaData();
			meta.SerializeToFile(path + "meta.txt",false);
			AssetDatabase.Refresh();
			for(int i=0; i<languageFiles.Length; i++) {
				string file = path + ((SystemLanguage)i).ToString() + ".txt";
				languageFiles[i] = (TextAsset)AssetDatabase.LoadAssetAtPath(file,typeof(TextAsset));
			}
			metaText = (TextAsset)AssetDatabase.LoadAssetAtPath(path+"meta.txt",typeof(TextAsset));
			EditorUtility.SetDirty(this);
		}
		UtilityExtensions.DeserializeFromBytes(ref meta, metaText.bytes);
#endif
		UtilityExtensions.DeserializeFromBytes(ref currentDictionary, languageFiles[(int)Language].bytes);
	}

	public void Unload() {
		onLanguageChanged -= ReloadLanguage;
	}

	[System.Serializable]
	class Translation {
		public string[] strings = new string[0];
	}

	public string GetString(int index, params string[] args) {
		if ( !isInitialized ) Initialize();
		if ( index < 0 || index >= currentDictionary.strings.Length ) return "// OOB: " + index + " //";
		if ( null == currentDictionary.strings[index] ) return "// NULL //";
		string ret = currentDictionary.strings[index].Replace("{N}","\n");
		for(int i=0; i<100; i++) {
			if ( ret.Contains("{"+i+"}") ) {
				ret = ret.Replace("{"+i+"}",i<args.Length?args[i]:"?");
			} else {
				break;
			}
		}
		return ret;
	}

	#if UNITY_EDITOR
	[System.Serializable]
	public class TranslationData {
		public string context = "";
		public int category = -1;
		
		public override bool Equals(object obj) {
			TranslationData td = obj as TranslationData;
			if ( null == td ) return false;
			return td.context == context && td.category == category;
		}
		public override int GetHashCode() {
			return context.GetHashCode() ^ category.GetHashCode();
		}
	}
	[System.Serializable]
	class MetaData {
		public TranslationData[] data = new TranslationData[0];
		public string[] categories = new string[0];
	}
	
	public string GetCategoryName(int index) {
		if ( !isInitialized ) Initialize();
		if ( index < 0 ) return "";
		if ( index >= meta.categories.Length ) return "";
		return meta.categories[index];
	}
	public void SetCategoryName(int index, string value) {
		if ( !isInitialized ) Initialize();
		if ( index < 0 ) return;
		while(index >= meta.categories.Length ) meta.categories = meta.categories.With("");
		if ( meta.categories[index] == value ) return;
		meta.categories[index] = value;
		Save();
	}
	
	public string GetStringRaw(int index) {
		if ( !isInitialized ) Initialize();
		if ( index < 0 || index >= currentDictionary.strings.Length ) return "// OOB" + index + " //";
		if ( null == currentDictionary.strings[index] ) return "// NULL //";
		return currentDictionary.strings[index];
	}
	public void SetString(int index, string value) {
		if ( !isInitialized ) Initialize();
		if ( index < 0 || index >= currentDictionary.strings.Length ) return;
		string old = currentDictionary.strings[index];
		if ( value == old ) return;
		currentDictionary.strings[index] = value;
		Save();
	}
	public TranslationData GetMeta(int index) {
		if ( !isInitialized ) Initialize();
		if ( index < 0 || index >= meta.data.Length ) throw new System.IndexOutOfRangeException();
		return meta.data[index].Clone();
	}
	
	public void SetMeta(int index, TranslationData data) {
		if ( !isInitialized ) Initialize();
		if ( index < 0 || index >= meta.data.Length ) throw new System.IndexOutOfRangeException();
		if ( data == meta.data[index] ) return;
		meta.data[index] = data;
		Save();
	}
	
	public int NewID() {
		if ( !isInitialized ) Initialize();
		Language = SystemLanguage.English;
		for(int i=0; i < currentDictionary.strings.Length; i++) {
			if ( "// NULL //" == GetStringRaw(i) ) {
				currentDictionary.strings[i] = "";
				meta.data[i] = new TranslationData();
				return i;
			}
		}
		currentDictionary.strings = currentDictionary.strings.With("");
		meta.data = meta.data.With(new TranslationData());
		Save();
		return currentDictionary.strings.Length-1;
	}
	
	public void Save() {
		if ( !isInitialized ) { Debug.LogError("Can't save, not initialized."); return; }

		string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this)) + "/" + name + "_data/";
		if ( null == path || path == "" ) { Debug.LogError("Can't save non-asset."); return; }

		string langFile = AssetDatabase.GetAssetPath(languageFiles[(int)Language]);
		currentDictionary.SerializeToFile(langFile,false);
//		Debug.Log(langFile);
		langFile = AssetDatabase.GetAssetPath(metaText);
//		Debug.Log(langFile);
		meta.SerializeToFile(langFile,false);
	}
	#endif
}

[System.Serializable]
public class Translatable {
	public Translatable() { index = -1; }
	public int index = -1;
	public Translator translator;

	public static implicit operator string(Translatable t) {
		return t.translator.GetString(t.index);
	}	
	public override string ToString() {
		return (string)this;
	}

	/*

	gains +1 attack

	+1 attack gained

	gains {x} attack

	{x} lbkarhei ghishg
	 */

	public string WithArgs(params string[] args) {
		return translator.GetString(index,args);
	}
}

