using LittlePolygon;
using System.Collections;
using UnityEngine;

public class Hazard : CustomBehaviour {
	
	public const int Layer = 11;
	
	public bool isAnamorphic = false;
	public float scaleMin = 1f;
	public float scaleMax = 1f;
	
	public Transform wiggleRoot;
	
	internal float hitTime = -999f;
	
	void Update() {
		if (!isAnamorphic) {
			var fromCam = transform.position - Cam.inst.p;
			transform.rotation = Quaternion.LookRotation(fromCam);
		}
		
		if (wiggleRoot) {
			var ht = TimeSince(hitTime) / 2.5f;
			wiggleRoot.localRotation = Quaternion.AngleAxis(Hero.Wibble(ht, 30f) * 20f, Vector3.forward);
		}
		
		
	}
	
	public void WasHit() {
		hitTime = Time.time;
	}
	
}
