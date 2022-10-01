#pragma strict

var soundON:GameObject;
var soundOFF:GameObject;
var audio1:String="on";
var audio2: String="on";
var on_off:boolean=true;
public static var checkaudio:boolean;
function Start () {
     audio2=PlayerPrefs.GetString("audio1");
   if (audio2 == "off") {
       AudioListener.pause = true;
       checkaudio=true;
		soundON.SetActive (false);
		soundOFF.SetActive (true);
	  
		}
		
  else 	{
		AudioListener.pause = false;
		soundON.SetActive (true);
		checkaudio=false;
		soundOFF.SetActive (false);
		}
}

function Update () {

}



function audiolistenerMUTe()
		{
	 		
         AudioListener.pause = true;
	     soundON.SetActive (false);
	     soundOFF.SetActive (true);
		 PlayerPrefs.SetString("audio1","off");
			
		}
function audiolistenerON()
		{
			
			AudioListener.pause = false;
			soundON.SetActive (true);
			soundOFF.SetActive (false); 
			PlayerPrefs.SetString("audio1","on");
			//levelsSound.off_sound=true;
		}
		