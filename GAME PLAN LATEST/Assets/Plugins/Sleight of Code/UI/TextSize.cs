using UnityEngine;
using System.Collections;

/**
 
 TextSize for Unity3D by thienhaflash (thienhaflash@gmail.com)
 
 Version	: 0.1
 Update		: 18.Jun.2012
 Features	:
	Return perfect size for any TextMesh
 	Cache the size of each character to speed up the size
	Evaluate and cache only when there are requirements
 
 Sample 	:
		
		//declare it locally, so we can have access anywhere from the script
		TextSize ts;
		
		//put this on the Start function
	 	ts = new TextSize(gameObject.GetComponent<TextMesh>());
		
		//anywhere, after you change the text :
		print(ts.width);
		
		//or get the length of an abitrary text (that is not assign to TextMesh)
		print(ts.GetTextWidth("any abitrary text goes here"));

 You are free to use the code or modify it any way you want (even remove this block of comments) but if it's good
 please give it back to the community.
 
 */

public class TextSize {
	private Hashtable dict; //map character -> width
	
	private TextMesh textMesh;
	private Renderer renderer;
	
	public TextSize(TextMesh tm){
		textMesh = tm;
		renderer = tm.GetComponent<Renderer>();
		dict = new Hashtable();
		GetSpace();
	}
	
	private void GetSpace(){ //the space can not be got alone
		string oldText = textMesh.text;
		
		textMesh.text = "a";
		float aw = renderer.bounds.size.x;
		textMesh.text = "a a";
		float cw = renderer.bounds.size.x - 2* aw;
		
		//MonoBehaviour.print("char< > " + cw);
		dict.Add(' ', cw);
		dict.Add('a', aw);
		
		textMesh.text = oldText;
	}
	
	public float GetTextWidth(string s) {
		char[] charList = s.ToCharArray();
		float w = 0;
		char c;
		string oldText = textMesh.text;
		
		for (int i=0; i<charList.Length; i++){
			c = charList[i];
			if ( c == '\n' ) { w = 0f; continue; }
			if (dict.ContainsKey(c)){
				w += (float)dict[c];
			} else {
				textMesh.text = ""+c;
				float cw = renderer.bounds.size.x;
				dict.Add(c, cw);
				w += cw;
				//MonoBehaviour.print("char<" + c +"> " + cw);
			}
		}
		
		textMesh.text = oldText;
		return w;
	}

	public void FitToWidth(float wantedWidth) {
		
		if(width <= wantedWidth) return;
		
		string oldText = textMesh.text;
		textMesh.text = "";
		
		string[] lines = oldText.Split('\n');

		foreach(string line in lines){
			textMesh.text += WrapLine(line, wantedWidth);
			textMesh.text += "\n";
		}
		textMesh.text = textMesh.text.Substring(0,textMesh.text.Length-1);

	}
	
	private string WrapLine(string s, float w)
	{
		// need to check if smaller than maximum character length, really...
		if(w <= 0 || s.Length <= 0) return s;
		
		char c;
		char[] charList = s.ToCharArray();
		
		float charWidth = 0;
		float wordWidth = 0;
		float currentWidth = 0;
		
		string word = "";
		string newText = "";
		string oldText = textMesh.text;
		
		for (int i=0; i<charList.Length; i++){
			c = charList[i];
			
			if (dict.ContainsKey(c)){
				charWidth = (float)dict[c];
			} else {
				textMesh.text = ""+c;
				charWidth = renderer.bounds.size.x;
				dict.Add(c, charWidth);
				//here check if max char length
			}
			
			if(c == ' ' || i == charList.Length - 1){
				if(c != ' ' ){
					word += c.ToString();
					wordWidth += charWidth;
				}
				
				if(currentWidth + wordWidth < w){
					currentWidth += wordWidth;
					newText += word;
				} else {
					currentWidth = wordWidth;
					newText += word.Replace(" ", "\n");
				}
				
				word = "";
				wordWidth = 0;
			} 
			
			word += c.ToString();
			wordWidth += charWidth;
		}
		
		textMesh.text = oldText;
		return newText;
	}

	public float width { get { return GetTextWidth(textMesh.text); } }
	public float height { get { return renderer.bounds.size.y; } }
}