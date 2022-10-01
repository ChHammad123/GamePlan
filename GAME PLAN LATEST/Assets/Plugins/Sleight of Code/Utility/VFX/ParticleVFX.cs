using UnityEngine;
using System.Collections;

public class ParticleVFX : VFX {
	public float DATdoesNotMatter;

	protected override void OnEnable() { 
		ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
		if ( ps ) {
			destroyAfterTime = ps.startLifetime;
			ps.enableEmission = true;
			if ( ps.isPlaying )	ps.Stop(); ps.Play();
		}
		base.OnEnable();
	}
	
	public override void ParentSuiciding() {
		if ( this ) {
			ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
			if ( ps ) {
				ps.enableEmission = false;
			}
			base.ParentSuiciding();
		}
	}
}
