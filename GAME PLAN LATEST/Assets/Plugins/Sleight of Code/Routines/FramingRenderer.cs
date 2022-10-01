using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class FramingRenderer : MonoBehaviour {
	public Renderer target;
	public Vector3 positionOffset;
	public bool offsetLocalToTarget = true;
	public Vector3 frameSize;
	Transform tx;
	Transform tartx;
	Vector3 lastSize;

	void Start() { tx = transform; }

	void Update() {
		if ( target ) {
			Vector3 size = (target.bounds.max - target.bounds.min);
			tx.position = target.bounds.center;
			if ( offsetLocalToTarget ) {
				tx.position += target.transform.TransformDirection(positionOffset);
			} else {
				tx.position += positionOffset;
			}

			if ( size != lastSize ) {
				lastSize = size;
				if ( tx.parent ) {
					// Does not work on android device. ?! ?! ?! ?! ?!
//					tx.localScale = UtilityExtensions.Multiop(
//						(float[] input)=>{return (input[2]==0f)?1f:(input[0]+input[1])/input[2];},
//						size, frameSize, tx.parent.lossyScale);

					Vector3 newScale = Vector3.zero;
					newScale.x = tx.parent.lossyScale.x == 0f ? 1f : (size.x + frameSize.x) / tx.parent.lossyScale.x;
					newScale.y = tx.parent.lossyScale.y == 0f ? 1f : (size.y + frameSize.y) / tx.parent.lossyScale.y;
					newScale.z = tx.parent.lossyScale.z == 0f ? 1f : (size.z + frameSize.z) / tx.parent.lossyScale.z;

					tx.localScale = newScale;
				} else {
					tx.localScale = tx.InverseTransformDirection(size + frameSize);
				}
			}
		}
	}

}

