/* This file is part of Stereoskopix FOV2GO for Unity V3.
 * URL: http://www.stereoskopix.com/ * Please direct any bugs/comments/suggestions to hoberman@usc.edu.
 * Stereoskopix FOV2GO for Unity Copyright (c) 2011-12 Perry Hoberman & MxR Lab. All rights reserved.
 *
 * s3d Smooth Mouse Look revised 12.30.12
 * javascript version for Stereoskopix package
 * Usage: Replace standard Unity MouseLook character controller script with this script.
 * Provides smoother mouse movement, by default only active when mouse button is down 
 * Uncheck MouseDownRequired to make always active.
 * Only active on desktop, automatically disabled on iOS and Android.
 * Note: conflicts to some extent with s3dTouchpad system, because this script uses entire screen for input, so touchpad movement triggers it
 */

public var MouseDownRequired : boolean = true;

public var frameCounter : float = 20;
private var originalRotation : Quaternion;

public var sensitivityX = 1.0;
public var sensitivityY = 1.0;

public var minimumX = -360.0;
public var maximumX = 360.0;

public var minimumY = -60.0;
public var maximumY = 60.0;

private var rotationX = 0.0;
private var rotationY = 0.0;

private var rotArrayX = new Array();
private var rotAverageX : float = 0;
private var rotArrayY = new Array();
private var rotAverageY : float = 0;
private	var xQuaternion : Quaternion;
private var yQuaternion : Quaternion;

function Update () {
	var tempFloat : float;
	
	var x : float;
	var y : float;
	
	
	if (Input.touchCount > 0) {
		var touch = Input.touches[0];
		x = touch.position.x - Screen.width / 2.0;
		y = touch.position.y - Screen.height / 2.0;
	} else {
		x = Input.GetAxis("Mouse X");
		y = Input.GetAxis("Mouse Y");
	}
	
	rotAverageY = 0;
	rotAverageX = 0;
	
  	rotationX += x * sensitivityX;
	rotationY += y * sensitivityY;
	
	rotArrayY.Add(rotationY);
	rotArrayX.Add(rotationX);

	if (rotArrayY.length >= frameCounter) {
		rotArrayY.RemoveAt(0);
	}
	if (rotArrayX.length >= frameCounter) {
		rotArrayX.RemoveAt(0);
	}
	 
	for(var j : int = 0; j < rotArrayY.length; j++) {
		tempFloat = rotArrayY[j];
		rotAverageY += tempFloat;
	}
	for(var i : int = 0; i < rotArrayX.length; i++) {
		tempFloat = rotArrayX[i];
		rotAverageX += tempFloat;
	}

	rotAverageY /= rotArrayY.length;
	rotAverageX /= rotArrayX.length;

	rotAverageY = Mathf.Clamp(rotAverageY, minimumY, maximumY);
	rotAverageX = Mathf.Clamp(rotAverageX%360, minimumX, maximumX);

	yQuaternion = Quaternion.AngleAxis (rotAverageY, Vector3.left);
	xQuaternion = Quaternion.AngleAxis (rotAverageX, Vector3.up);
	 
	transform.localRotation = originalRotation * xQuaternion * yQuaternion;
}

function Start () {
	// Make the rigid body not change rotation
	if (GetComponent.<Rigidbody>()) {
		GetComponent.<Rigidbody>().freezeRotation = true;
	}
	originalRotation = transform.localRotation;
}

