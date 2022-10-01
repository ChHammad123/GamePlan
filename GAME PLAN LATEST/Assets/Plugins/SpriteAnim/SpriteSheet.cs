using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;

public static class SpriteExtensions {
	public static float UnitsPerPixel(this SpriteRenderer sprend) {
		return sprend.bounds.size.x / sprend.sprite.rect.width;
	}
	public static float PixelsPerUnit(this SpriteRenderer sprend) { return 1f / UnitsPerPixel(sprend); }
}

[System.Serializable]
public class AnimatedSpriteMessage {
	public int frame=0;
	public string message="";
	public int iOb; public float fOb; public bool bOb; public string sOb; public Vector2 v2ob;
	public enum MType { None, Int, Float, Bool, String, Vector2 }
	public MType type;
	public enum SendType { Self, Descendants, ChildrenOnly, ParentOnly, Ancestors, Broadcast }
	[OptionalField] public SendType sendType;

	public void SendTo(GameObject source) {
		switch (sendType) {
			case SendType.Ancestors:
				switch(type) {
					case MType.None:   source.SendMessageUpwards(message,SendMessageOptions.DontRequireReceiver); break;
					case MType.Bool:   source.SendMessageUpwards(message,bOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Int:    source.SendMessageUpwards(message,iOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Float:  source.SendMessageUpwards(message,fOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.String: source.SendMessageUpwards(message,sOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Vector2:source.SendMessageUpwards(message,v2ob,SendMessageOptions.DontRequireReceiver);break;
				}
				break;
			case SendType.Broadcast:
				switch(type) {
					case MType.None:   source.BroadcastMessage(message,SendMessageOptions.DontRequireReceiver); break;
					case MType.Bool:   source.BroadcastMessage(message,bOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Int:    source.BroadcastMessage(message,iOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Float:  source.BroadcastMessage(message,fOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.String: source.BroadcastMessage(message,sOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Vector2:source.BroadcastMessage(message,v2ob,SendMessageOptions.DontRequireReceiver);break;
				}
				break;
			case SendType.ChildrenOnly:
				foreach(Transform t in source.transform) {
					switch(type) {
						case MType.None:   t.gameObject.SendMessage(message,SendMessageOptions.DontRequireReceiver); break;
						case MType.Bool:   t.gameObject.SendMessage(message,bOb,SendMessageOptions.DontRequireReceiver); break;
						case MType.Int:    t.gameObject.SendMessage(message,iOb,SendMessageOptions.DontRequireReceiver); break;
						case MType.Float:  t.gameObject.SendMessage(message,fOb,SendMessageOptions.DontRequireReceiver); break;
						case MType.String: t.gameObject.SendMessage(message,sOb,SendMessageOptions.DontRequireReceiver); break;
						case MType.Vector2:t.gameObject.SendMessage(message,v2ob,SendMessageOptions.DontRequireReceiver);break;
					}
				}
				break;
			case SendType.Descendants:
				Transform[] Q = new Transform[]{source.transform};
				while(Q.Length > 0 ) {
					foreach(Transform t in Q[0]) {
						switch(type) {
							case MType.None:   t.gameObject.SendMessage(message,SendMessageOptions.DontRequireReceiver); break;
							case MType.Bool:   t.gameObject.SendMessage(message,bOb,SendMessageOptions.DontRequireReceiver); break;
							case MType.Int:    t.gameObject.SendMessage(message,iOb,SendMessageOptions.DontRequireReceiver); break;
							case MType.Float:  t.gameObject.SendMessage(message,fOb,SendMessageOptions.DontRequireReceiver); break;
							case MType.String: t.gameObject.SendMessage(message,sOb,SendMessageOptions.DontRequireReceiver); break;
							case MType.Vector2:t.gameObject.SendMessage(message,v2ob,SendMessageOptions.DontRequireReceiver);break;
						}
						Q = Q.With(t);
					}
					Q = Q.WithoutIndices(0);
				}
				break;
			case SendType.ParentOnly:
				if ( source.transform.parent != null ) {
					source = source.transform.parent.gameObject;
					switch(type) {
						case MType.None:   source.SendMessage(message,SendMessageOptions.DontRequireReceiver); break;
						case MType.Bool:   source.SendMessage(message,bOb,SendMessageOptions.DontRequireReceiver); break;
						case MType.Int:    source.SendMessage(message,iOb,SendMessageOptions.DontRequireReceiver); break;
						case MType.Float:  source.SendMessage(message,fOb,SendMessageOptions.DontRequireReceiver); break;
						case MType.String: source.SendMessage(message,sOb,SendMessageOptions.DontRequireReceiver); break;
						case MType.Vector2:source.SendMessage(message,v2ob,SendMessageOptions.DontRequireReceiver);break;
					}
				}
				break;
			case SendType.Self:
				switch(type) {
					case MType.None:   source.SendMessage(message,SendMessageOptions.DontRequireReceiver); break;
					case MType.Bool:   source.SendMessage(message,bOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Int:    source.SendMessage(message,iOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Float:  source.SendMessage(message,fOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.String: source.SendMessage(message,sOb,SendMessageOptions.DontRequireReceiver); break;
					case MType.Vector2:source.SendMessage(message,v2ob,SendMessageOptions.DontRequireReceiver);break;
				}
				break;
		}
	}
}

[System.Serializable]
public class SpriteAnimation {
	public enum RepeatStyle { Loop, PingPong, StayAtEnd, DieAtEnd, PlayAnimation }
	public string name = "", nextAnimation="";
	public int frameFirst = 0;
	public int frameLast = 0;
	public RepeatStyle repeat;
	public string messageOnEnd="";

	public AnimatedSpriteMessage[] messages = new AnimatedSpriteMessage[0];

	public enum AnimationStyle { Simple, Advanced }
	[OptionalField] public AnimationStyle animationStyle;

	[System.Serializable]
	public class ComplexFrame {
		public int sprite = 0;
		public float lengthInFrames = 1f;
#if UNITY_EDITOR
		public bool openInInspector=true;
#endif
	}
	[OptionalField] public ComplexFrame[] complexFrames;

#if UNITY_EDITOR
	public bool edInspectorOpen, edAnimated=true;
	public int edSpriteIndex;
	[OptionalField] public int complexEdIndex;
	[System.NonSerialized] public bool edMessagesOpen = false;
#endif
}

public class SpriteSheet : MonoBehaviour {
	public Texture2D sourceTexture;
	public Sprite[] sprites;
	[HideInInspector] public SpriteAnimation[] animations;
	public static int buttonPad = 6;
	public float spriteDrawingSize = 2f;
	public int inspectorSpriteSize = 2;

	public int PickSprite(int current, int show = 9) {
		int ret = current;
		using(new GUIUtils.Horizontal() ) {
			if ( null == sourceTexture || null == sprites || sprites.Length < 1 ) {
				GUILayout.Label("No sprites to pick...");
				return 0;
			}
			int from = Mathf.Max(0,current-4);
			int to = Mathf.Min(sprites.Length-1,from+show);
			for(int ix=from; ix <= to; ix++) {
				Rect r = new Rect(sprites[ix].rect);
				Rect draw = GUILayoutUtility.GetRect(r.width, r.height, GUILayout.Width(r.width*spriteDrawingSize+buttonPad*2), GUILayout.Height(r.height*spriteDrawingSize+buttonPad*2));
				r.width /= sourceTexture.width;
				r.height /= sourceTexture.height;
				r.x /= sourceTexture.width;
				r.y /= sourceTexture.height;
				if ( GUI.Button(draw, "", ix==ret?GUIUtils.StyleDepressedButton:GUI.skin.button) ) {
					ret = ix;
				}
				draw.x+=buttonPad/2; draw.width-=buttonPad; draw.height-=buttonPad; draw.y+=buttonPad/2;
				GUI.DrawTextureWithTexCoords(draw, sourceTexture, r);
			}
		}
		return ret;
	}
	[System.NonSerialized] Vector2 pickerScroll;
	public int PickSpriteFromFullImage(int current, int size=2) {
		using(new GUIUtils.Scroll(ref pickerScroll)) {
			Rect spacetaker = GUILayoutUtility.GetRect(sourceTexture.width*size,sourceTexture.width*size,sourceTexture.height*size,sourceTexture.height*size);
			GUI.Box(spacetaker, "");
			for(int i=0; i < sprites.Length; i++) {
				Rect r = sprites[i].rect;
				Rect guiPosition = new Rect(spacetaker.x+r.x*size,spacetaker.y+spacetaker.height-r.y*size-r.height*size,r.width*size,r.height*size); //GUILayoutUtility.GetRect(r.width*2f+4,r.width*2f+4,r.height*2f+4,r.height*2f+4);
				if ( GUI.Button(guiPosition,"") ) {
					current = i;
				}
				Rect ratio = new Rect(
					r.x/sourceTexture.width,
					r.y/sourceTexture.height,
					r.width/sourceTexture.width,
					r.height/sourceTexture.height);
				guiPosition.x+=2; guiPosition.y+=2;
				guiPosition.width-=2; guiPosition.height-=2;
				GUI.DrawTextureWithTexCoords(guiPosition, sourceTexture, ratio);
			}
		}
		return current;
	}
}
