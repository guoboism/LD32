using LittlePolygon;
using System.Collections;
using UnityEngine;

public class Cam : CustomBehaviour {
	public static Cam inst;
	internal Vector3 p, offset;
	internal Quaternion baseRotation;
	internal float shakeTime = -999f;
	internal Color bgColor;
	internal Camera cam;
	
	void Awake() {
		inst = this;
		cam = GetComponent<Camera>();
		bgColor = cam.backgroundColor;
	}
	
	void OnDestroy() {
		if (inst == this) { inst = null; }
	}

	void Start() {
		p = transform.position;
		offset = p - Hero.inst.transform.position;
		baseRotation = transform.rotation;
		hp = Hero.inst.transform.position;
		tracking = 1f;
	}
	
	float zspeed;
	Vector3 hp, tp;
	internal float tracking;
	float lastHeroTime = -999f;
	bool wasWaiting = false;
	
	internal Patch trackingPatch = null;
	
	void LateUpdate() {
	
		if (wasWaiting || Hero.inst.status == Hero.Status.Waiting) {
		
			var t = TimeSince(lastHeroTime);
			tracking = Mathf.Clamp01(t/2.0f);
		
			zspeed = zspeed.EaseTowards(Hero.inst.maxSpeed, 0.1f);
			tp.z += zspeed * Time.deltaTime;
			tp.x = -1f;
			trackingPatch = World.inst.FindPatch(tp.x, tp.z);
			tp.y = trackingPatch == null ? tp.y : trackingPatch.HeightAtZ(tp.z);
		
			p = Lerp(hp, tp, SmoothStep(tracking)) + offset;
		
		} else {
			zspeed = 0f;
			trackingPatch = null;
			tracking = 0f;
		
			var p1 = Hero.inst.transform.position;
			lastHeroTime = Time.time;
			
			p.x = p.x.EaseTowards(0.65f * p1.x + offset.x, 0.1f);
		
			var targetY = Mathf.Max (p1.y, 0f);
			if (Hero.inst.patch != null) {
				targetY = Hero.inst.patchHeight;
			}
			p.y = p.y.EaseTowards (targetY + offset.y, 0.085f);
			
			p.z = p1.z + offset.z;
			
			// don't go below ground
			var patch = World.inst.FindPatch(p.x, p.z);
			if (patch != null) {
				var h = patch.HeightAtZ(p.z);
				var minY = h+2.0f;
				p.y = Mathf.Max (p.y, minY);
			}
			
			
			tp = hp = p - offset;
		}
	
		
		wasWaiting = Hero.inst.status == Hero.Status.Waiting;
		
		transform.position = p;		
		
		var shakeDuration = 0.5f;
		var shake = Wibble(TimeSince(shakeTime)/shakeDuration, 6f);
		transform.rotation = baseRotation * Quaternion.AngleAxis(shakeIntensity * 2f * shake, Vector3.right);
		
		var ft = TimeSince(flashTime);
		if (ft < 0.1f) {
			cam.cullingMask = 0;
			cam.backgroundColor = ft < 0.05f ? Color.black : Color.white;
		} else if (cam.cullingMask == 0) {
			cam.cullingMask = 0xffff;
			cam.backgroundColor = bgColor;
		}
		
	}
	
	static float Wibble(float x, float oscillations) {
		return Mathf.Sin (oscillations * Mathf.PI * x) / Mathf.Pow (x + 1f, 8f);
	}
	
	internal float shakeIntensity = 0f;
	
	public void Shake(float intensity=1f) {
		shakeTime = Time.time;
		shakeIntensity = intensity;
	}
	
	internal float flashTime = -999f;
	
	public void Flash() {
		flashTime = Time.time;
	}
}
