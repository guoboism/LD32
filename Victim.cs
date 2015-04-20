using LittlePolygon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RNG = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class Victim : CustomBehaviour {
	public const int Layer = 9;
	public const int ShrapnelLayer = 10;
	
	public float minSpeed = 10.0f;
	public float maxSpeed = 20.0f;
	public float fov = 25.0f;

	public Transform billboard;
			
	public SpriteRenderer dropShadow;
			
	public PhysicMaterial shrapnelMat;
			
	internal Vector3 move;
	internal Rigidbody body;
	internal Patch patch;
	
	public enum Status { Running, Hit };
	
	internal Status status = Status.Running;

	internal List<GameObject> shrapnel;
			
	void Awake() {
		body = GetComponent<Rigidbody>();
	}
	
	void OnDestroy() {
		if (shrapnel != null) {
			foreach(var s in shrapnel) {
				if (s) Destroy(s);
			}
		}
	}
			
			
	public Victim Create(Patch patch) {
		var x = RNG.Range (patch.x0+0.25f, patch.x1-0.25f);
		var z = RNG.Range(patch.z0, patch.z1);
		var y = patch.HeightAtZ(z);
		var result = Dup(this, Vec(x,y,z));
		
		result.patch = patch;
		
		var mag = RNG.Range (minSpeed, maxSpeed) * Time.fixedDeltaTime;
		var move = (RNG.value < 0.5f) ? Vec(mag,0,0) : Vec(-mag,0,0);
		result.move = Quaternion.AngleAxis(RNG.Range (-fov, fov), Vector3.up) * move;
		
		result.transform.localScale = Vec(Mathf.Sign (result.move.x), 1f, 1f);
		
		patch.localObjects.Add(result.gameObject);
		
		return result;
	}
			
	void FixedUpdate() {
		
		if (status == Status.Running) {
			
			var pos = body.position;
			pos += move;
			
			var x0 = patch.x0 + 0.25f;
			var x1 = patch.x1 - 0.25f;
			
			if (pos.x < x0) {
				var delta = x0 - pos.x;
				pos.x += delta + delta;
				move.x *= -1f;
				transform.localScale = Vec(Mathf.Sign (move.x), 1f, 1f);
			} else if (pos.x > x1) {
				var delta = x1 - pos.x;
				pos.x += delta + delta;
				move.x *= -1f;
				transform.localScale = Vec(Mathf.Sign (move.x), 1f, 1f);
			}
			
			if (pos.z < patch.z0) {
				var delta = patch.z0 - pos.z;
				pos.z += delta + delta;
				move.z *= -1f;
			} else if (pos.z > patch.z1) {
				var delta = patch.z1 - pos.z;
				pos.z += delta + delta;
				move.z *= -1f;
			}
			
			pos.y = patch.HeightAtZ(pos.z);
			body.MovePosition(pos);
			
		} else if (status == Status.Hit) {
			
		}
		
		
		
	}
	
	void Update() {
		billboard.rotation = Quaternion.LookRotation(billboard.position - Cam.inst.transform.position);
	}
	
	public bool TryHit(Vector3 pos, Vector3 vel) {
		if (status == Status.Hit) { return false; }
		
		var sfx = RNG.Range(1, 4);
		Jukebox.Play ("victim_hit_" + sfx);
		
		
		dropShadow.enabled = false;
		
		status = Status.Hit;

		pos = transform.position; // hack


		var sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
		shrapnel = new List<GameObject>(sprites.Length);

		foreach(var spr in sprites) {
			if (spr == dropShadow) continue; 
			var xf = spr.transform;
			var bounds = spr.bounds;
			
			var dup = Dup(spr.gameObject).transform;
			
			var sr = dup.GetComponent<SpriteRenderer>();
			var deadSprite = ShrapnelRemapper.Get(sr.sprite);
			if (deadSprite != null) {
				sr.sprite = deadSprite;
			}
			
			var body = new GameObject("Shrapnel", typeof(Rigidbody), typeof(SphereCollider)).GetComponent<Rigidbody>();
			body.gameObject.layer = ShrapnelLayer;
			body.position = 0.3f * xf.position + 0.7f * pos;
			body.rotation = xf.rotation;
			
			body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
			body.velocity = vel;
			body.AddForce(Vec(RNG.Range(-3f, 3f), RNG.Range(8f,12f), RNG.Range(0.5f,2.5f)), ForceMode.Impulse);
			body.interpolation = RigidbodyInterpolation.Extrapolate;
			
			body.angularDrag = 0.01f;
			body.drag = 0.01f;
			body.angularVelocity = Vec(0, 0, RNG.Range(-0.4f, 0.4f));
			
			var sphere = body.GetComponent<SphereCollider>();
			sphere.radius = 0.55f * Mathf.Min(bounds.size.x, bounds.size.y);
			sphere.sharedMaterial = shrapnelMat;
			
			dup.parent = body.transform;
			dup.localScale = xf.lossyScale;
			dup.localPosition = Vector3.zero;
			spr.enabled = false;
			
			shrapnel.Add(body.gameObject);
			
		}		
		return true;
	}
	
}
