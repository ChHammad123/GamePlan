using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PoolMaster {
	public static bool poolingActive = false;

	static Pooler runner=null;
	static PoolMaster() {
		runner = new GameObject("Poolmaster").AddComponent<Pooler>();
		GameObject.DontDestroyOnLoad(runner.gameObject);
	}
	static Dictionary<Pooler, Pooler[]> dict = new Dictionary<Pooler, Pooler[]>();

	public static Pooler GetOne(Pooler p, Vector3 position, Quaternion rotation) {
		if ( poolingActive ) {
			if ( !dict.ContainsKey(p) ) dict.Add(p, new Pooler[0]);
			Pooler[] list = dict[p];
			for(int i=0; i<list.Length; i++) {
				if ( list[i].isAvailable ) {
					list[i].transform.position = position;
					list[i].transform.rotation = rotation;
					list[i].isAvailable = false;
					if ( !list[i].startsInactive ) list[i].gameObject.SetActive(true);
					return list[i];
				}
			}
		}
		Pooler newOne = (Pooler)GameObject.Instantiate(p,position,rotation);
		if ( poolingActive ) dict[p] = dict[p].With(newOne);
		return newOne;
	}
	public static void Destroy(Object thing) {
		if ( null == thing ) throw new System.NullReferenceException();
		Pooler p=null;
		GameObject g=null;
		if ( thing is Component ) {
			g = (thing as Component).gameObject;
			p = g.GetComponent<Pooler>();
		} else if ( thing is GameObject) {
			g = (thing as GameObject);
			p = g.GetComponent<Pooler>();
		} else if ( null == thing ) return;

		if ( null != g ) g.SendMessage("BeforeDestroy",SendMessageOptions.DontRequireReceiver);
		if ( null != p && p.usePooling && poolingActive ) {
			p.gameObject.SetActive(false);
			p.isAvailable = true;
		} else {
//			Debug.Log("Destroying " + thing.name + " because it's not a pooler");
			GameObject.Destroy(thing);
		}
	}
	public static void Destroy(Object thing, float time) {
		runner.StartCoroutine(DestroyAfterTime(thing,time));
	}

	static IEnumerator DestroyAfterTime(Object thing, float time) { 
		GameObject ob = thing as GameObject;
		Component c = thing as Component;
		if ( ob ) {
			while(time > 0f ) {
				yield return null;
				if ( !ob ) yield break;
				if ( !ob.activeSelf ) { yield break; }
				time -= Time.deltaTime;
			}
			Destroy(thing);
		} else if ( c ) {
			while(time > 0f ) {
				yield return null;
				if ( !c ) { yield break; }
				time -= Time.deltaTime;
			}
			Destroy(thing);
		}
	}
}

public class Pooler : SOCBehaviour {
	public bool startsInactive = false;
	public bool usePooling = true;
	[HideInInspector] public bool isAvailable = false;

	public bool IsActive { get { return !isAvailable; } }

//	public new static Object Instantiate(Object toInstantiate, Vector3 position, Quaternion rotation) {
	public static T Instantiate<T>(T toInstantiate, Vector3 position, Quaternion rotation) where T : Object {
		Pooler pInst=null;
		if ( toInstantiate is GameObject ) {
			pInst = (toInstantiate as GameObject).GetComponent<Pooler>();
		} else if ( toInstantiate is Component ) {
			pInst = (toInstantiate as Component).GetComponent<Pooler>();
		}
		if ( null == pInst ) return (T)GameObject.Instantiate(toInstantiate, position, rotation);
		Pooler ret = PoolMaster.GetOne(pInst, position, rotation);
		if ( toInstantiate is GameObject ) return ret.gameObject as T;
		if ( toInstantiate is Component ) return ret.GetComponent(toInstantiate.GetType()) as T;
		return ret as T;
	}
	public new static Object Instantiate(Object toInstantiate) {
		return Instantiate(toInstantiate, Vector3.zero, Quaternion.identity);
	}

	public new virtual void Destroy(Object thing, float time) {
		PoolMaster.Destroy(thing,time);
	}
	public new virtual void Destroy(Object thing) {
		PoolMaster.Destroy(thing);
	}
}
