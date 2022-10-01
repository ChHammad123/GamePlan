// TODO: looping desyncs animations
// TODO: editor doesn't show last frame on an 8-frame complex with 3x multipliers


using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class AnimatedSprite : MonoBehaviour {
	public static float globalFramerate = 18f;
	public static float timePerFrame = 1f / globalFramerate;

	public static Quaternion right = Quaternion.identity;
	public static Quaternion left = Quaternion.AngleAxis(180,Vector3.up);

	public SpriteSheet sheet;
	public string startAnimation = "";
	public float startSpeed = 1f;
	public int startDirection = 0;
	
	public float randomOffset = 0f;
	public float timeOffset = 0f;

	public int Frame { get { return Mathf.RoundToInt(frame); } }

	public string CurrentAnimationName { get { return currentAnimation.name; } }


	public delegate void Empty();
	protected Empty onFrameChange = null;
	protected Empty onAnimationEnd = null;

	public void OnAnimationEnd(Empty function) {
		onAnimationEnd = function;
	}

	public void SetSprite(Sprite sprite) {
		if ( sprender ) sprender.sprite = sprite;
		if ( targetImage ) targetImage.sprite = sprite;
	}

	protected SpriteAnimation currentAnimation;
	protected float frame, frameDirection, nextFrameIn, pauseTime; int lastFrame;
	[HideInInspector] public SpriteRenderer sprender;
	[HideInInspector] public Image targetImage;
	protected GameObject myObject;
	protected bool paused = false;

	public int FacingDirection { get; private set; }

	/// <summary>Copy the state of another AnimatedSprite.</summary>
	/// <param name="aspr">The sprite to copy.</param>
	public void Copy(AnimatedSprite aspr) {
		sheet = aspr.sheet;
		ContinueAnimation(aspr.currentAnimation.name);
		frame = aspr.frame;	frameDirection = aspr.frameDirection;
		nextFrameIn = aspr.nextFrameIn;	lastFrame = aspr.lastFrame;
	}

	protected virtual void Awake() {
		myObject = gameObject;
		sprender = GetComponent<SpriteRenderer>();
		targetImage = GetComponent<Image>();
		if ( !sprender && !targetImage ) {
			sprender = this.GetOrAddComponent<SpriteRenderer>();
		}
		//sprender.sprite = null;
		if ( null != sheet && !string.IsNullOrEmpty(startAnimation)) PlayAnimation(startAnimation, startDirection, startSpeed);
		AdvanceFrame(timeOffset + Random.Range (0f,randomOffset));
	}

	public void StopAnimation() {
		frameDirection = 0f;
	}

	public bool HasAnimation(string animation) {
		foreach(SpriteAnimation anim in sheet.animations) {
			if ( anim.name == animation ) return true;
		}
		return false;
	}

	/// <summary>Plays an indicated animation from the beginning.</summary>
	/// <param name="animation">Name of animation to play.</param>
	/// <param name="direction">Direction to face the object: Below zero will spin the object 180 degrees around the Y axis. A value of zero will cause this function to not rotate the object.</param>
	/// <param name="anidir">Speed to play the animation. This can be negative.</param>
	/// <param name="ofc">Function to call each time a new frame is displayed. This function must return void and have zero arguments.</param>
	public void PlayAnimation(string animation, float direction = 0f, float anidir = 1f, Empty ofc=null) {
		onFrameChange = ofc;
		if ( direction < 0 ) { transform.rotation = left; } else if ( direction > 0 ) { transform.rotation = right; }
		if ( direction != 0 ) FacingDirection = (int)direction;
		bool found = false;

		foreach(SpriteAnimation anim in sheet.animations) {
			if ( anim.name == animation ) {
#if UNITY_EDITOR
				if ( found ) {
					Debug.LogError("Sheet " + sheet.name + " contains multiple animations named " + animation);
				}
#endif
				currentAnimation = anim;
				if ( anidir < 0 ) frame = anim.frameLast+0.49f;
				else frame = anim.frameFirst-0.49f;
				frameDirection = anidir * globalFramerate;
				SetSprite(sheet.sprites[Mathf.RoundToInt(frame)]);
				nextFrameIn = timePerFrame*0.5f;
				found = true;
				paused = false;
			}
		}
#if UNITY_EDITOR
		if ( !found ) {
			Debug.LogError("Sheet " + sheet.name + " does not contain an animation " + animation);
		}
#endif
	}

	/// <summary>Continue an animation. If the animation is already playing, it will continue to play with no changes. Otherwise this function works exactly like PlayAnimation.</summary>
	/// <param name="animation">Name of animation to play.</param>
	/// <param name="direction">Direction to face the object: Below zero will spin the object 180 degrees around the Y axis. A value of zero will cause this function to not rotate the object.</param>
	/// <param name="anidir">Speed to play the animation. This can be negative.</param>
	/// <param name="ofc">Function to call each time a new frame is displayed. This function must return void and have zero arguments.</param>
	public void ContinueAnimation(string animation, float direction = 0f, float anidir = 1f, Empty ofc=null) {
		if ( direction < 0 ) { transform.rotation = left; } else if ( direction > 0 ) { transform.rotation = right; }
		if ( null != currentAnimation && currentAnimation.name == animation && anidir * frameDirection > 0f && ofc == onFrameChange ) { 
			return;
		}
		paused = false;
		PlayAnimation(animation,direction,anidir,ofc);
	}

	public void PauseForFrames(int frames) {
		paused = true;
		pauseTime = (float)frames * (1f/globalFramerate) * Mathf.Abs(frameDirection);
	}
	public void Pause() {
		paused = true;
		pauseTime = -1f;
	}
	public void UnPause() { paused = false; }

	public bool JustChangedFrames { get; private set; }

	/// <summary>Advance the frame counter by an amount. Advancing past the last or before the first frame may have unexpected results.</summary>
	/// <param name="amount">Amount.</param>
	public void AdvanceFrame(float amount) {frame += amount;}

	protected virtual void Update() {
		if ( null != currentAnimation && frameDirection != 0f ) {
			if ( paused ) {
				if ( pauseTime > 0f ) { // don't run down time if it's less than zero
					pauseTime -= frameDirection*Time.deltaTime;
					if ( pauseTime < 0f ) paused = false;
				} 
			} else {
				SpriteAnimation.ComplexFrame complexFrame = null;
				if ( currentAnimation.animationStyle == SpriteAnimation.AnimationStyle.Advanced ) {
					if ( Mathf.RoundToInt (frame) >= currentAnimation.complexFrames.Length ) {
						frame = 0f;
					}
					complexFrame = currentAnimation.complexFrames[Mathf.RoundToInt(frame)];
				}
				if ( null == complexFrame ) {
					frame+= frameDirection * Time.deltaTime;
				} else {
					frame+= frameDirection * Time.deltaTime / (complexFrame.lengthInFrames);
				}
				int thisFrame = Mathf.RoundToInt(frame);
				if ( null != complexFrame ) {
					if ( thisFrame >= currentAnimation.complexFrames.Length ) {
						//thisFrame = 0;
					}
//					complexFrame = currentAnimation.complexFrames[thisFrame];
				}
				bool sendEndMessage = false;
				if ( null != complexFrame ) {
					if ( thisFrame < 0 ) {
						switch (currentAnimation.repeat) {
						case SpriteAnimation.RepeatStyle.Loop: frame = currentAnimation.complexFrames.Length+0.49f; break;
						case SpriteAnimation.RepeatStyle.StayAtEnd: frame = lastFrame; nextFrameIn -=Mathf.Sign(frameDirection)* Time.deltaTime; break;
						case SpriteAnimation.RepeatStyle.PingPong: frame = -0.49f; frameDirection*=-1f; break;
						case SpriteAnimation.RepeatStyle.DieAtEnd: frame = 0f; Destroy(gameObject); break;
						case SpriteAnimation.RepeatStyle.PlayAnimation: PlayAnimation(currentAnimation.nextAnimation,0f,Mathf.Sign(frameDirection)); break;
						}
						sendEndMessage = true;
					} else if ( thisFrame >= currentAnimation.complexFrames.Length ) {
						switch (currentAnimation.repeat) {
						case SpriteAnimation.RepeatStyle.Loop: frame = -0.49f; break;
						case SpriteAnimation.RepeatStyle.StayAtEnd: frame = lastFrame; nextFrameIn -=Mathf.Sign(frameDirection)* Time.deltaTime; break;
						case SpriteAnimation.RepeatStyle.PingPong: frame = currentAnimation.complexFrames.Length-1f; frameDirection*=-1f; break;
						case SpriteAnimation.RepeatStyle.DieAtEnd: frame = 0f; Destroy(gameObject); break;
						case SpriteAnimation.RepeatStyle.PlayAnimation: PlayAnimation(currentAnimation.nextAnimation,0f,Mathf.Sign(frameDirection)); break;
						}
						sendEndMessage = true;
					}
				} else {
					if ( thisFrame < currentAnimation.frameFirst ) {
						switch (currentAnimation.repeat) {
							case SpriteAnimation.RepeatStyle.Loop: frame = currentAnimation.frameLast+0.49f; break;
							case SpriteAnimation.RepeatStyle.StayAtEnd: frame = lastFrame; nextFrameIn -=Mathf.Sign(frameDirection)* Time.deltaTime; break;
							case SpriteAnimation.RepeatStyle.PingPong: frame = currentAnimation.frameFirst-0.49f; frameDirection*=-1f; break;
							case SpriteAnimation.RepeatStyle.DieAtEnd: frame = 0f; Destroy(gameObject); break;
							case SpriteAnimation.RepeatStyle.PlayAnimation: PlayAnimation(currentAnimation.nextAnimation,0f,Mathf.Sign(frameDirection)); break;
						}
						sendEndMessage = true;
					}
					if ( thisFrame >= currentAnimation.frameLast + 1 ) {
						switch (currentAnimation.repeat) {
							case SpriteAnimation.RepeatStyle.Loop: frame = currentAnimation.frameFirst-0.49f; break;
							case SpriteAnimation.RepeatStyle.StayAtEnd: frame = lastFrame; nextFrameIn -=Mathf.Sign(frameDirection)* Time.deltaTime; break;
							case SpriteAnimation.RepeatStyle.PingPong: frame = currentAnimation.frameLast+0.49f; frameDirection*=-1f; break;
							case SpriteAnimation.RepeatStyle.DieAtEnd: frame = 0f; Destroy(gameObject); break;
							case SpriteAnimation.RepeatStyle.PlayAnimation: PlayAnimation(currentAnimation.nextAnimation,0f,Mathf.Sign(frameDirection)); break;
						}
						sendEndMessage = true;
					}
				}
				thisFrame = Mathf.RoundToInt(frame);

				JustChangedFrames = thisFrame != lastFrame || nextFrameIn < 0;
				string wasPlayingAnimation = currentAnimation.name;
				if ( JustChangedFrames ) {
					if ( null == complexFrame ) {
						SetSprite(sheet.sprites[thisFrame]);
					} else {
						if ( currentAnimation.animationStyle == SpriteAnimation.AnimationStyle.Advanced ) {
							if ( Mathf.RoundToInt (frame) >= currentAnimation.complexFrames.Length ) {
								frame = 0f;
							}
							complexFrame = currentAnimation.complexFrames[Mathf.RoundToInt(frame)];
						}
						SetSprite(sheet.sprites[complexFrame.sprite]);
					}
					if ( nextFrameIn < 0f ) {
						nextFrameIn += timePerFrame*0.5f;

					}
					if ( null != onFrameChange ) onFrameChange();
					AnimatedSpriteMessage[] messages = currentAnimation.messages.Where(x=>x.frame==thisFrame).ToArray();
					foreach(AnimatedSpriteMessage m in messages) {
						m.SendTo(myObject);
					}
					if ( wasPlayingAnimation != currentAnimation.name ) {
						thisFrame = Mathf.RoundToInt(frame);
						sendEndMessage = false;
					}
				}
				if (sendEndMessage){
					if ( null != onAnimationEnd ) onAnimationEnd();
					if ( !string.IsNullOrEmpty(currentAnimation.messageOnEnd) ) 
						SendMessage(currentAnimation.messageOnEnd, SendMessageOptions.DontRequireReceiver);
				}
				lastFrame = thisFrame;
			}
		}
	}
}