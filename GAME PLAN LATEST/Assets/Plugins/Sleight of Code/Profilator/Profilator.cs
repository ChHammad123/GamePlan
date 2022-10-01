using UnityEngine;
using System.Collections.Generic;

public class Profilator : System.IDisposable {
	public class p{
		public float least=Mathf.Infinity, last, total, most;
		public int count, frames, framesLast;
		public double lastSW; // Last time recorded using Stopwatch
	}
	public static Dictionary<string,p> profiles = new Dictionary<string, p>();
	
	System.Diagnostics.Stopwatch stopwatch;
	
	string name;
	float start,end;
	int startFrame, endFrame;

	public Profilator(string name) {
		stopwatch = new System.Diagnostics.Stopwatch();
		stopwatch.Start();
		
		this.name = name;
		start = Time.realtimeSinceStartup;
		startFrame = Time.frameCount;
	}
	public void Dispose() {
		end = Time.realtimeSinceStartup;
		endFrame = Time.frameCount;
		if ( !profiles.ContainsKey(name) ) profiles.Add(name,new p());
		profiles[name].last = (end-start);
		
		profiles[name].least = Mathf.Min(profiles[name].least, profiles[name].last);
		profiles[name].most = Mathf.Max(profiles[name].most, profiles[name].last);
		profiles[name].count += 1;
		profiles[name].total += profiles[name].last;
		profiles[name].frames = (endFrame-startFrame);
		profiles[name].framesLast = (endFrame-startFrame);
		
		stopwatch.Stop();
		profiles[name].lastSW = stopwatch.Elapsed.TotalSeconds;
	}
	
	public static void ShowAll() {
		string kill = null;
		int colWidth = 70;
		using( new GUIUtils.Horizontal() ){
			GUILayout.Label("Name",GUILayout.Width(150));
			GUILayout.Label("Last S",GUILayout.Width(colWidth));
			GUILayout.Label("Average S", GUILayout.Width(colWidth));
			GUILayout.Label("Most S",GUILayout.Width(colWidth));
			GUILayout.Label("Least S",GUILayout.Width(colWidth));
			GUILayout.Label("Last F",GUILayout.Width(colWidth));
			GUILayout.Label("Average F", GUILayout.Width(colWidth));
			GUILayout.Label("SW Last S",GUILayout.Width(colWidth));
			GUILayout.Label("Count", GUILayout.Width(colWidth));
		}
		foreach(string key in profiles.Keys) {
			using (new GUIUtils.Horizontal()) {
				GUILayout.Label(key,GUILayout.Width(150));
				GUILayout.Label(profiles[key].last.ToString(), GUILayout.Width(colWidth));
				GUILayout.Label((profiles[key].total/profiles[key].count).ToString(), GUILayout.Width(colWidth));
				GUILayout.Label(profiles[key].most.ToString(),GUILayout.Width(colWidth));
				GUILayout.Label(profiles[key].least.ToString(),GUILayout.Width(colWidth));
				GUILayout.Label(profiles[key].framesLast.ToString(), GUILayout.Width(colWidth));
				GUILayout.Label((profiles[key].frames/profiles[key].count).ToString(), GUILayout.Width(colWidth));
				GUILayout.Label(profiles[key].lastSW.ToString(), GUILayout.Width(colWidth));
				GUILayout.Label(profiles[key].count.ToString(), GUILayout.Width(colWidth));
				GUILayout.FlexibleSpace();
				if ( GUILayout.Button ("Remove",GUILayout.ExpandWidth(false))) {
					kill=key;
				}
			}
		}
		if ( null != kill ) profiles.Remove (kill);
	}
	
}

