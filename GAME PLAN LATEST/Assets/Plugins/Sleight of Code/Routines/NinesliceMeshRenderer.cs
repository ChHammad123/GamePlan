using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class NinesliceMeshRenderer : MonoBehaviour {
	public Material material;
	public float scale = 1f;
	public Rect uvRegion = new Rect(0f,0f,1f,1f);
	public Rect border;
	public bool stretch = true;
	[DisplayIf("stretch",false)]
	public bool stretchCenter;
	[DisplayIf("stretch",false)]
	public bool stretchPartials;
	public bool refresh = true;
	[DisplayIf("flipDirection",false)]
	public bool doubleSided = true;
	[DisplayIf("doubleSided",false)]
	public bool flipDirection;

	Vector3 lastScale;
	Transform tx;
	MeshFilter mf;
	MeshRenderer mr;
	Material lastMaterial;
	Rect lastBorder;
	Vector3 scaleFactor;

	Vector3 n3(float a, float b) { return new Vector3(a,b,0f); }
	Vector2 n2(float a, float b) { return new Vector2(a,b); }

	Vector3[] baseVerts {
		get {
			float 
				r = .5f-border.width/scaleFactor.x,
				l = -.5f+border.x/scaleFactor.x,
				t = .5f-border.height/scaleFactor.y,
				b = -.5f+border.y/scaleFactor.y;

			return new Vector3[]{
				n3(-.5f,-.5f),n3(l,-.5f),n3(r,-.5f),n3(.5f,-.5f),
				n3(-.5f,b),   n3(l,b),   n3(r,b),   n3(.5f,b),
				n3(-.5f,t),   n3(l,t),   n3(r,t),   n3(.5f,t),
				n3(-.5f,.5f), n3(l,.5f), n3(r,.5f), n3(.5f,.5f)
			};
		}
	}
	int[] baseTris {
		get {
			return new int[] {
				4,0,5, 5,0,1, 6,2,7, 7,2,3,
				12,8,13, 13,8,9, 14,10,15, 15,10,11,
//				5,10,9, 5,6,10
			};
		}
	}
	Vector2[] baseUVs {
		get {
			float w = 1f-border.width;
			float h = 1f-border.height;
			float x = border.x; float y = border.y;
			return new Vector2[]{
				UV(0,0), UV(x,0), UV(w,0), UV(1,0),
				UV(0,y), UV(x,y), UV(w,y), UV(1,y),
				UV(0,h), UV(x,h), UV(w,h), UV(1,h),
				UV(0,1), UV(x,1), UV(w,1), UV(1,1)
			};
		}
	}

	Vector2 UV(float u, float v) {
		return n2(u * uvRegion.width+uvRegion.x, v*uvRegion.height+uvRegion.y);
	}
	void OnDisable() {
//		DestroyImmediate(mf.sharedMesh);
		lastScale = Vector3.zero;
	}
	void Start() {
		tx = transform;
		mf = this.GetOrAddComponent<MeshFilter>();
		mr = this.GetOrAddComponent<MeshRenderer>(); 
		Mesh m = new Mesh();
		m.hideFlags = HideFlags.HideAndDontSave;
//		m.vertices = baseVerts;
//		m.triangles = baseTris;
//		m.uv = new Vector2[16];
//		FillMesh(m);
		mf.sharedMesh = m;
	}
	void Update() {
		if ( scale > 0f ) {
			scaleFactor = tx.lossyScale / scale;
		}

		if ( lastMaterial != material ) {
			lastMaterial = material;
			mr.sharedMaterial = material;
		}
		if ( (refresh || lastScale != scaleFactor || border != lastBorder)
		    && scaleFactor.x != 0f && scaleFactor.y != 0f && scaleFactor.z != 0f ) {
			border = new Rect(Mathf.Clamp01(border.x),Mathf.Clamp01(border.y),Mathf.Clamp(border.width,0f,1f-border.x),Mathf.Clamp(border.height,0f,1f-border.y));
			lastBorder = border;
			refresh = false;
			lastScale = scaleFactor;
			Mesh m = mf.sharedMesh;
			Vector3[] v;
			Vector2[] u;
			int[]     t;

			v = baseVerts;
			t = baseTris;
			u = baseUVs;

			if ( stretch ) {
				t = t.With(new int[]{
					5,10,9, 5,6,10, // center

					1,6,5, 1,2,6, 4,9,8, 4,5,9, 6,11,10, 6,7,11, 9,14,13, 9,10,14
				}); 
			} else {
				if ( scaleFactor.x > 0f && scaleFactor.y > 0f ) {
					Vector2 uvRemaining = n2(1f-border.x-border.width, 1f-border.y-border.height);
					Vector2 step = n2(uvRemaining.x/scaleFactor.x, uvRemaining.y/scaleFactor.y);
					Vector2 count = n2((v[2].x-v[1].x)/Mathf.Ceil((v[2].x-v[1].x)/step.x),(v[8].y-v[4].y)/Mathf.Ceil((v[8].y-v[4].y)/step.y));
					if ( stretchPartials ) step = count;
					Vector3 pivotb = v[1], pivotl=v[4], pivotr=v[6], pivott=v[9];
					Vector3 
						pivopb = n3(step.x, border.y/scaleFactor.y),
						pivopt = n3(step.x, border.height/scaleFactor.y),
						pivopl = n3(border.x/scaleFactor.x,step.y),
						pivopr = n3(border.width/scaleFactor.x,step.y);

					if ( step.x > 0.01f ) {
						float max = v[2].x; // 1f - border.x - border.width;
						var uvs = new Vector2[]{
							UV(border.x, border.y), UV(1f-border.width, border.y),
							UV(1f-border.width, 1f-border.height), UV(border.x, 1f-border.height)
						};

						// TOP AND BOTTOM
						for(pivotb.x=v[1].x; pivotb.x<max; pivotb.x+= step.x) {
							int nvi = v.Length;
							if ( pivotb.x+step.x>max ) {
								pivopt.x = pivopb.x = max-pivotb.x;
							}
							pivott.x = pivotb.x;
							v = v.With(new Vector3[]{												
								pivotb, pivotb+Vector3.right*pivopb.x, pivotb+pivopb, pivotb+Vector3.up*pivopb.y,
								pivott, pivott+Vector3.right*pivopt.x, pivott+pivopt, pivott+Vector3.up*pivopt.y								
							});
							t = t.With(new int[]{
								nvi,nvi+2,nvi+3, nvi,nvi+1,nvi+2,
								nvi+4,nvi+6,nvi+7, nvi+4,nvi+5,nvi+6
							});
							u = u.With(new Vector2[]{
								UV(border.x,0f), UV(1f-border.width,0f),UV(1f-border.width, border.y), UV(border.x,border.y),
								UV(border.x,1f-border.height),UV(1f-border.width,1f-border.height),UV(1f-border.width,1f),UV(border.x,1f)
							});


							// CENTER VERTICAL STRIP
							if ( step.y > 0.01f ) {
								float ymax = v[8].y;
								Vector3 piv = pivotb + pivopb.y * Vector3.up;
								Vector3 pivur = n3(pivopb.x, step.y);
								for(; piv.y<ymax; piv.y+= step.y) {
									nvi = v.Length;
									if ( piv.y+step.y>ymax ) {
										pivur.y = ymax-piv.y;
									}
									v = v.With(
										piv,
										piv+Vector3.right*pivopb.x,
										piv+pivur,
										piv+Vector3.up*pivur.y
									);
									t = t.With(nvi,nvi+2,nvi+3, nvi,nvi+1,nvi+2);
									u = u.With(uvs);
								}
							}

						}
					}
					if ( step.y > 0.01f ) {
						float max = v[8].y;
						// LEFT & RIGHT
						for(pivotl.y=v[4].y; pivotl.y<max; pivotl.y+= step.y) {
							int nvi = v.Length;
							if ( pivotl.y+step.y>max ) {
								pivopr.y = pivopl.y = max-pivotl.y;
							}
							pivotr.y = pivotl.y;
							v = v.With(new Vector3[]{												
								pivotl, pivotl+Vector3.right*pivopl.x, pivotl+pivopl, pivotl+Vector3.up*pivopl.y,
								pivotr, pivotr+Vector3.right*pivopr.x, pivotr+pivopr, pivotr+Vector3.up*pivopr.y								
							});
							t = t.With(new int[]{
								nvi,nvi+2,nvi+3, nvi,nvi+1,nvi+2,
								nvi+4,nvi+6,nvi+7, nvi+4,nvi+5,nvi+6
							});
							u = u.With(new Vector2[]{
								UV(0f,border.y), UV(border.x,border.y),UV(border.x, 1f-border.height), UV(0f,1f-border.height),
								UV(1f-border.width,border.y), UV(1f,border.y),n2(1f, 1f-border.height), UV(1f-border.width,1f-border.height),
							});

						}
					}
				}
			}
			if ( flipDirection ) {
				int[] ttd = new int[t.Length];
				for(int i=0; i < t.Length; i+=3) {
					ttd[i]=t[i]; ttd[i+1]=t[i+2]; ttd[i+2]=t[i+1];
				}
				t = ttd;
			} else if ( doubleSided ) {
				int[] ttd = t.CloneCopy();
				for(int i=0; i < t.Length; i+=3 ) {
					ttd = ttd.With(t[i],t[i+2],t[i+1]);
				}
				t = ttd;
			}

			/**/
			if ( null == m ) { m = new Mesh(); m.hideFlags = HideFlags.HideAndDontSave; }
			m.Clear();
			m.vertices = v;
			m.triangles = t;
			m.uv = u;
			FillMesh(m);
			mf.sharedMesh = m;
		}
	}
	void FillMesh(Mesh m) {
		int vcount = m.vertices.Length;
		m.colors = new Color[vcount].Fill(Color.white);
		m.normals = new Vector3[vcount].Fill(Vector3.up );
		m.tangents = new Vector4[vcount].Fill(Vector4.zero);
	}
}

