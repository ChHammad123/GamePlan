using UnityEngine;
using System.Collections;

public class AdvancedAnimatedUV : MonoBehaviour {
	public Material material;
	public string targetTextureProperty;

	public Rect frameZero;

	public float timePerFrame = 0f;

	public Action atEnd;

	public bool startAtTopRow = true;

	public enum Action {
		Loop, Stop, Die
	}

	public delegate void Empty();
	public event Empty onReachedEnd;

	Vector2 speed;
	Vector2 offset;

	void Start() { if ( null == material ) material = GetComponent<Renderer>().material; 
		speed = new Vector2(frameZero.width, frameZero.height);
		if ( startAtTopRow ) offset = new Vector2(0f, 1f-frameZero.height);
		else offset = Vector2.zero;
	}

	float timer;
	void Update() {
		if ( timePerFrame == 0f ) {
			offset.x += Time.deltaTime * speed.x; 
		} else {
			timer -= Time.deltaTime;
			if ( timer < 0f ) {
				offset.x += speed.x;
				timer += timePerFrame;
			}
		}
		if ( offset.x > (1f-speed.x) ) {
			offset.x -= (1f-speed.x);
			if ( startAtTopRow ) {
				offset.y -= speed.y;
				if ( offset.y < 0f ) {
					if ( null != onReachedEnd ) onReachedEnd();
					if ( atEnd == Action.Loop ) offset.y = 1f-frameZero.height;
					else if ( atEnd == Action.Stop ) speed = Vector2.zero;
				}
			} else {
				offset.y += speed.y;
				if ( offset.y > (1f - speed.y) ) {
					if ( null != onReachedEnd ) onReachedEnd();
					if ( atEnd == Action.Loop ) offset.y = 0f;
					else if ( atEnd == Action.Stop ) speed = Vector2.zero;
				}
			}
		}
		if ( speed != Vector2.zero ) material.SetTextureOffset(targetTextureProperty, offset);
	}
}

