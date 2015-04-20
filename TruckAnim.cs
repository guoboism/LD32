using LittlePolygon;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TruckAnim : CustomBehaviour {
	public float fps = 16.0f;
	public Texture2D[] frames;
	MeshRenderer r;
	float t;
	void Awake() {
		r = GetComponent<MeshRenderer>();
	}
	
	void LateUpdate() {
		if (Hero.inst.grounded) {
			var frame = Mathf.FloorToInt(Hero.inst.beatTime * fps) % frames.Length;
			r.sharedMaterial.mainTexture = frames[frame];
		} else {
			r.sharedMaterial.mainTexture = frames[2];
		}
	}

}
