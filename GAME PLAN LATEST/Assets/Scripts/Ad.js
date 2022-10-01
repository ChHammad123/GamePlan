#pragma strict
var a1 : GUITexture;
var ad_tex : Texture2D[];
InvokeRepeating("Ad",2,2);
function Start () {

}

function Update () {


}

function Ad(){
if(a1.texture==ad_tex[0])
a1.texture=ad_tex[1];
else if(a1.texture==ad_tex[1])
a1.texture=ad_tex[0];

}
function OnMouseUp(){
	
Application.OpenURL("https://play.google.com/store/apps/details?id=com.football.worldcupgreal.free");

}