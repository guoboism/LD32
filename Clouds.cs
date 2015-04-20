using LittlePolygon;
using System.Collections;
using UnityEngine;

using RNG = UnityEngine.Random;

public class Clouds : CustomBehaviour {

	struct Cloud {
		public Transform xform;
		public Vector3 pos;
		public float rate;
		public float rx, ry;
	}

	Cloud[] clouds;

	void Awake() {
		
		var sprites = GetComponentsInChildren<SpriteRenderer>();
		int len = sprites.Length;
		clouds = new Cloud[len];
		for(int i=0; i<len; ++i) {
			clouds[i] = new Cloud() {
				xform = sprites[i].transform,
				pos = sprites[i].transform.localPosition,
				rx = RNG.Range(0.2f, 0.9f),
				ry = RNG.Range(0.1f, 0.5f),
				rate = RNG.Range(-0.4f, 0.4f)
			};
		}
	}
	
	void Update() {
		var t = Time.time;
		int len = clouds.Length;
		for (int i=0; i<len; ++i) {
			var c = clouds[i];
			c.xform.localPosition = c.pos + Vec(c.rx * Mathf.Sin (c.rate*t), c.ry * Mathf.Cos (c.rate * t), 0f);
		}
	}
	
}
