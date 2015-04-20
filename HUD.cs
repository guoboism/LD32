using LittlePolygon;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : CustomBehaviour {
	
	public static HUD inst;
	public CanvasScaler scaler;
	public Image barImage;
	public Transform accent;
	public Text multiText;
	
	public float decayTime = 8f;
	public float hitCount = 10f;
	
	internal int multi = 0;
	internal float current = 0f;
	
	Quaternion r0;
	
	void Awake() {
		inst = this;
		r0 = accent.transform.localRotation;
		multiText.text = "";
		
	}
	
	void OnDestroy() {
		if (inst == this) { inst = null; }
	}
	
	IEnumerator Start() {
		
		var title = FindObject<Title>();
		while(title) {
			yield return null;
		}	
		
		barImage.transform.localScale = Vec(0,1,1);
		
		var p0 = scaler.transform.position;
		foreach(var t in Transition(1.5f)) {
			var u = EaseOutBack(t);
			scaler.transform.position = p0.Below(u * (105));
			yield return null;
		}
		
	}
	
	public void OnHitVictim() {
		current += 1f/hitCount;
		if (current >= 1f) {
			current = 0f;
			multi++;
			multiText.text = "x" + multi;
			Jukebox.Play("multi");
			Jukebox.Play("cheer");
		}
	}
	
	public void OnExplode() {
		current = 0f;
		multi = 0;
		multiText.text = "";
	}
	
	void LateUpdate() {
	
		var bps = 8f/3f;
		accent.localRotation = r0 * Quaternion.AngleAxis(5f * Mathf.Sin(2f * Time.time * Mathf.PI *bps), Vector3.forward);
	
		var decay = Time.deltaTime / decayTime;
		current = Mathf.Clamp01(current - decay);
		barImage.transform.localScale = Vec(current,1,1);
	}
	
}
