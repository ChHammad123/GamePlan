using UnityEngine;
using System.Collections;

public class AnimatedUV : MonoBehaviour {
	public Material material;
	public Vector2 speed;
	public Vector2 tileSpeed;
	public Vector2 randomSpread;

	public float timePerFrame = 0f;
	void Start() { if ( null == material ) material = GetComponent<Renderer>().material; 
		initialTile = material.mainTextureScale;
		initialOffset = material.mainTextureOffset;
		speed = new Vector2(speed.x + Random.Range(0f,randomSpread.x), speed.y + Random.Range(0f,randomSpread.y));
	}
	void OnDisable() {
		material.mainTextureScale = initialTile;
		material.mainTextureOffset = initialOffset;
	}
	Vector2 initialTile, initialOffset;

	float timer;
	void Update() {
		Vector2 offset = material.mainTextureOffset;
		Vector2 tileOffset = material.mainTextureScale;
		if ( timePerFrame == 0f ) {
			offset += Time.deltaTime * speed; 
			tileOffset += Time.deltaTime * tileSpeed;
		} else {
			timer -= Time.deltaTime;
			if ( timer < 0f ) {
				offset += speed;
				tileOffset += tileSpeed;
				timer += timePerFrame;
			}
		}
		while(offset.x >= 1f ) offset.x -= 2f;
		while(offset.x < -1f ) offset.x += 2f;
		while(offset.y >= 1f ) offset.y -= 2f;
		while(offset.y < -1f ) offset.y += 2f;
		if ( speed != Vector2.zero ) material.mainTextureOffset = offset;
		if ( tileSpeed != Vector2.zero ) material.mainTextureScale = tileOffset;
	}
}

