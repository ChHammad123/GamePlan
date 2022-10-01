using UnityEngine;

// Tiles the sprite based on scale
[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedTiledSprite : AnimatedSprite {
	public float tileCount = 1f;
	static Vector2 leftPivot = new Vector2(0,0.5f);

	AnimatedSprite[] cache;
	float lastSize=1f;
	Sprite endSprite;
	Sprite originalSprite;

	public void SetEndpoints(Vector3 from, Vector3 to) {
		Vector3 dif = from-to;
		tileCount = dif.magnitude/(sprender.UnitsPerPixel()*sprender.sprite.rect.width);
		transform.position = from;
		transform.LookRightAt(dif);
	}

	public Color color {
		get { return cache[0].sprender.color; }
		set { foreach(AnimatedSprite sprend in cache) sprend.sprender.color = value; }
	}
	protected override void Awake() {
		base.Awake();
		cache = new AnimatedSprite[]{this};
		originalSprite = sprender.sprite;
		endSprite = Sprite.Create(sprender.sprite.texture,sprender.sprite.rect,sprender.sprite.bounds.center);
	}
	void LateUpdate() {
//		base.Update();
		tileCount = Mathf.Max(0.00001f,tileCount);
		int spritesRequired = Mathf.CeilToInt(tileCount);
		if ( tileCount != lastSize ) {
			while(cache.Length < spritesRequired ) {
				Sprite s = Sprite.Create(sprender.sprite.texture,sprender.sprite.rect,sprender.sprite.bounds.center);
				SpriteRenderer sr = new GameObject("extender").AddComponent<SpriteRenderer>();
				AnimatedSprite aspr = sr.gameObject.AddComponent<AnimatedSprite>();
				aspr.Copy(this);
				sr.transform.CopyAndParentTo(transform);
				sr.transform.localPosition = new Vector3(sr.UnitsPerPixel() * s.rect.width * cache.Length,0f,0f);
				cache = cache.With(aspr);
			}
			for(int i=1; i<cache.Length; i++) {
				if ( i < spritesRequired ) {
					cache[i].gameObject.SetActive(true);
					cache[i].GetComponent<AnimatedSprite>().Copy(this);
				} else {
					cache[i].gameObject.SetActive(false);
				}
			}
		}
		lastSize = tileCount;
	//}
	//void LateUpdate() {
//		int spritesRequired = Mathf.CeilToInt(tileCount);
		for(int i=0; i< cache.Length;i++) {
			cache[i].Copy(this);
		}
		int last = Mathf.FloorToInt(tileCount);
		if ( last == tileCount ) last--;
		float lastSpriteSize = tileCount - (spritesRequired-1);
		Sprite lastSprite = endSprite;
		Sprite curSprite = cache[last].sprender.sprite;
		if ( null != curSprite ) {
			endSprite = Sprite.Create(curSprite.texture,curSprite.rect.WithWidth(lastSpriteSize * originalSprite.rect.width),leftPivot);
			cache[last].sprender.sprite = endSprite;
			Destroy(lastSprite);
		}
	}
}

