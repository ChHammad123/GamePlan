using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class UVQuad : MonoBehaviour {
	public Vector2[] uvs = new Vector2[4];

	Mesh mesh = null;

	void Awake() { 
		UpdateUV();
	}

	void OnDisable() {
//		if ( null != mesh ) { DestroyImmediate(mesh); mesh = null; }
	}

	public void SetUVS(Vector2[] newUV) {
		uvs = newUV; UpdateUV();
	}

	Mesh BaseMesh() {
		Mesh m = new Mesh();
		m.hideFlags = HideFlags.HideAndDontSave;
		m.vertices = new Vector3[]{new Vector3(-0.5f,-0.5f,0f),new Vector3(0.5f,-0.5f,0f),new Vector3(0.5f,0.5f,0f),new Vector3(-0.5f,0.5f,0f)};
		m.triangles = new int[]{0,2,1, 0,3,2};
		m.normals = new Vector3[]{-Vector3.forward,-Vector3.forward,-Vector3.forward,-Vector3.forward};
		return m;
	}

	void UpdateUV() {
		if ( null == mesh ) mesh = BaseMesh();
		mesh.uv = uvs;
		GetComponent<MeshFilter>().sharedMesh = mesh;
	}

#if UNITY_EDITOR
	Vector2[] lastUVs = null;
	// Update is called once per frame
	void Update() {
		if ( null != lastUVs ) {
			if ( !lastUVs.MemberwiseEquals(uvs) ) {
				UpdateUV();
			}
		}
		lastUVs = uvs.Copy();
	}
#endif
}

