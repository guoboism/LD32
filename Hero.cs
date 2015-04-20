using LittlePolygon;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Hero : CustomBehaviour {
	public const int Layer = 8;
	
	public static Hero inst;
	
	public Transform fx;
	public Transform shadow;
	public Transform cone;
	public Transform coneAnchor;
	
	public MeshRenderer fx00;
	public MeshRenderer fx45;
	public MeshRenderer fx60;
	public MeshRenderer frontWheel;
	
	public Sprite[] creams;
	public SpriteRenderer coneRenderer;
	
	public float maxSpeed = 30f;
	public float minSpeed = 5f;
	public float turningSpeed = 20f;
	public float turningImpulse = 45.0f;
	public float turningDrag = 2.5f;
	public float turningDragQ = 0.1f;
	public float restoreP = 0.1f;
	public float cancelP = 1f;
	public float freefallDrag = 0.5f;
	
	public float stretchDuration = 1.0f;
	public float squishDuration = 1.0f;
	
	public ParticleSystem bloodSprayer;
	
	public float cliffHeight = 1f;
	
	internal Rigidbody body;
	internal float speed = 0f;
	internal float rotationSpeed = 0f;
	internal float rotation = 0f;
	internal float pitch = 0f;
	internal Vector3 velocity = Vector3.zero;
	
	internal bool grounded = true;
	internal Patch patch = null;
	internal float patchHeight, patchDeriv;
	internal float leaveGroundTime = -999f;
	internal float hitGroundTime = -999f;
	internal float scaleY = 1f;
	
	internal float coneSpeed = 0f;
	internal float coneY = 0f;
	internal float hitTime = -999f;
	
	public SpriteRenderer explosion;
	public SpriteRenderer rainbow;
	public Sprite[] explosionFrames;
	public Sprite[] rainbowFrames;
	internal float explosionTime = -999f;
	internal Vector3 explosionConePos;
	
	public enum Status {
		Waiting, Active, Exploding
	}
	
	internal Status status = Status.Active;
	
	internal float beatTime = 0f;
	internal float beatBounce = 1f;
	
	internal float respawnTime = -999f;

	//--------------------------------------------------------------------------------
	// INPUT
	
	float Steering {
		get {
			if (Input.GetKey(KeyCode.LeftArrow)) {
				return -1f;
			} else if (Input.GetKey(KeyCode.RightArrow)) {
				return 1f;
			} else {
				return 0f;
			}
		}
	}
	
	//--------------------------------------------------------------------------------
	// CALLBACKS	
	
	void Awake() {
		inst = this;
		body = GetComponent<Rigidbody>();
		fx.parent = null;
		shadow.parent = null;
		cone.parent = null;
		coneY = cone.position.y;
		
		explosion.sprite = null;
		explosion.gameObject.SetActive(false);
	}
	
	void OnDestroy() {
		if (inst == this) { 
			inst = null; 
		}
		if (fx) { Destroy(fx.gameObject); }
		if (shadow) { Destroy(shadow.gameObject); }
		if (cone) { Destroy(cone.gameObject); }
	}
	
	internal bool initialWait = false;
	
	void Start() {
		
		initialWait = true;
		SetWaiting();
		
	}
	
	void Update() {
		//if (Input.GetKeyDown(KeyCode.K)) {
		//	TryExplode();
		//}
		
		if (status == Status.Active) {
			UpdateActive();
		} else if (status == Status.Exploding) {
			UpdateExploding();
		} else if (status == Status.Waiting) {
			UpdateWaiting();
		}
		
	}
	
	void SetWaiting() {
		
		if (status == Status.Exploding) {
			explosion.gameObject.SetActive(false);
			status = Status.Waiting;
		} else if (status == Status.Active) {
			fx.gameObject.SetActive(false);
			shadow.gameObject.SetActive(false);
			cone.gameObject.SetActive(false);
			status = Status.Waiting;
		}
		
	}
	
	void UpdateWaiting() {
		
		
		if (initialWait) {
			if (!FindObject<Title>())
				initialWait = false;
			else
				return;
		}
		
		if (Cam.inst.tracking < 0.999f || Cam.inst.trackingPatch == null) {
			return;
		}
		
		// respawn
		
		respawnTime = Time.time;
		
		Jukebox.Play ("truck_respawn");
		Jukebox.Play("cheer");
		
		status = Status.Active;
		fx.gameObject.SetActive(true);
		shadow.gameObject.SetActive(true);
		cone.gameObject.SetActive(true);
		
		var spawnPos = Cam.inst.p - Cam.inst.offset;
		spawnPos.y += 8f;
		spawnPos.x = 0.5f * (Cam.inst.trackingPatch.x0 + Cam.inst.trackingPatch.x1) - 0.001f;
		
		body.position = spawnPos;
		body.rotation = Quaternion.identity;
		speed = maxSpeed;
		velocity = Vec(0, 0, speed);
		rotationSpeed = 0f;
		rotation = 0f;
		pitch = 0f;
		grounded = false;
		patch = null;
		leaveGroundTime = -999f;
		hitGroundTime = -999f;
		scaleY = 1f;
		
		coneSpeed = 0f;
		coneY = coneAnchor.position.y;
		cone.position = coneAnchor.position;
		
		explosionTime = -999f;
		
	}

	void FixedUpdate() {
		if (status == Status.Active) {
			FixedUpdateActive();
			
			if (patch == null && body.position.y < -3f) {
				TryExplode();
			}
		}
	}
	
	
	void TryExplode() {
		if (status == Status.Active) {
			status = Status.Exploding;
			explosionTime = Time.time;
			explosion.transform.rotation = Cam.inst.baseRotation * Quaternion.AngleAxis(-5f, Vector3.up);
			fx.gameObject.SetActive(false);
			shadow.gameObject.SetActive(false);
			explosion.gameObject.SetActive(true);
			Jukebox.Play("truck_explode");
			Jukebox.Play("boo");
			explosionConePos = cone.position;
			Cam.inst.Shake(10f);
			Cam.inst.Flash();
			HUD.inst.OnExplode();
		}
	}
	
	void UpdateExploding() {
		var t = TimeSince(explosionTime);
		var fps = 20.0f;
		var frame = Mathf.FloorToInt(t * fps);
		
		// finished?
		if (frame >= explosionFrames.Length) {
			SetWaiting();
			return;
		}
		
		cone.position = explosionConePos + Vec(0, 10f * Parabola(t), -20f * t);
		
		explosion.sprite = explosionFrames[frame];
		rainbow.sprite = rainbowFrames[frame];
	}
	
	void FixedUpdateActive() {
		
		// preamble		
		var dt = Time.fixedDeltaTime;
		var p0 = body.position;
		var t0 = fx.forward;
		var prevGrounded = grounded;
		
		// edge rotation
		var hw = World.inst.extent;
		var nx = (p0.x / hw);
		var edgeResponse = Mathf.Pow (nx, 7f) * Mathf.Max (0f, t0.x * nx);

		// udpate rotation		
		rotationSpeed -= turningDrag * rotationSpeed * dt;
		rotationSpeed -= turningDragQ * Mathf.Abs(rotationSpeed) * rotationSpeed * dt;
		if (grounded) {
			rotationSpeed += Steering * (1.0f - Mathf.Abs (edgeResponse)) * turningImpulse * dt;
		}
		rotationSpeed -= restoreP * rotation * dt;
		rotation += rotationSpeed * dt;
		
		if (grounded) {
			rotationSpeed *= 1f - edgeResponse;
			rotation -= cancelP * edgeResponse;
		}
		
		// update pitch
		if (!grounded && pitch > 0f) {
			pitch *= 0.97f;
		}
		
		// update orientation
		var radians = Mathf.Deg2Rad * rotation;
		var fwd = Vec(Mathf.Sin(radians), pitch, Mathf.Cos (radians));
		var rot = Quaternion.LookRotation(fwd);
		body.MoveRotation(rot);
		
		// linear location
		if (grounded) {
			
			// driving
			var effectiveMaxSpeed = maxSpeed + HUD.inst.multi * 4f;
			speed += 0.2f * (effectiveMaxSpeed - speed);
			velocity = fwd * speed;
			
		} else {
			// freefall
			velocity -= freefallDrag * velocity * dt;
		}
		velocity += Physics.gravity * dt;
		
		var p1 = p0 + velocity * dt;
		
		// cast move ray to clip position
		RaycastHit hit;
		var ep = 0.01f;
		var move = (p1-p0).magnitude;
		var moveRay = new Ray(p0.Above (ep), p1-p0);
		if (Physics.Raycast(moveRay, out hit, move, 1)) {
			if (hit.normal.y * hit.normal.y < 0.01f) {
				p1 = hit.point + ep * hit.normal;
			}
		}

		// cast left/right to avoid walls
		var pad = 0.8f;
		var leftRay = new Ray(p1+Vec(ep,0,0), Vec(-1,0,0));
		var rightRay = new Ray(p1-Vec(ep,0,0), Vec(1,0,0));
		if (Physics.Raycast(leftRay, out hit, pad, 1) || Physics.Raycast(rightRay, out hit, pad, 1)) {
			p1 = hit.point + pad * hit.normal;
			if (Vector3.Dot(transform.forward, hit.normal) < 0f) {
				rotation *= 0.8f;
			}
		}

		patch = World.inst.FindPatch(p1.x, p1.z);
		
						
		// resolve grounded
		
		if (patch == null) {
			grounded =false;
			
		} else {
			
			// slopes are hard :(
			patchHeight = patch.HeightAtZ(p1.z);
			patchDeriv = patch.DerivAtZ(p1.z);
			var heightSlop = grounded ? 0.025f : 0.01f;
			heightSlop -= 0.03f * Mathf.Clamp (patchDeriv, -1f, 0f);
			
			if (p1.y < patchHeight + heightSlop) {
							
				p1.y = Mathf.Max (p1.y, patchHeight);
				
				// bounce?
				if (!grounded && velocity.y < -4.0f && Mathf.Approximately(patchDeriv,0f)) {
					velocity.y = 4f;
					leaveGroundTime = hitGroundTime = Time.time;
					Jukebox.Play("truck_bounce");
				} else {
					velocity.y = 0f;
					speed = velocity.magnitude;
					pitch = patchDeriv;
					if (!grounded) {
						Jukebox.Play("truck_hit_ground");
					}
					grounded = true;
				}
				
					
			} else {
				grounded = false;
			}
		
		}
		body.MovePosition(p1);
		
		// bookmark ground collision times
		if (grounded ^ prevGrounded) {
			if (grounded) {
				hitGroundTime = Time.time;
			} else {
				leaveGroundTime = Time.time;
				Jukebox.Play("truck_jump");
			}
		}
		
	}
	
	public static float Wibble(float t, float oscillations) {
		return Mathf.Sin (oscillations * Mathf.PI * t) / Mathf.Pow (t+1f, 8f);
	}
	
	void UpdateActive() {
		var dt = Time.deltaTime;
		
		// fx postioning
		var p = transform.position;
		fx.position = p;
		var preWibble = fx.rotation.EaseTowards(transform.rotation, 0.5f);
		
		
		// hit wibble
		var wibbleTime = TimeSince(hitTime) / 1.25f;
		fx.rotation = preWibble * Quaternion.AngleAxis(15f * Wibble(wibbleTime, 20), fx.up);
		
		// squash/stretch
		{
			var sy = 1f;
			if (grounded) {
				var t = Mathf.Clamp01((TimeSince(hitGroundTime)) / squishDuration);
				var squishMin = 0.7f;
				var squishMax = 1.0f;
				sy = Lerp(squishMin, squishMax, EaseOutElastic(t));
			} else {
				var t = Mathf.Clamp01 ((TimeSince(leaveGroundTime)) / stretchDuration);
				var stretchMin = 1.0f;
				var stretchMax = 1.2f;
				sy = Lerp(stretchMin, stretchMax, EaseOutBack(t));
			}
			scaleY = scaleY.EaseTowards(sy, 0.5f);
			
			if (grounded) {
				beatTime += dt;
			} else {
				beatTime = 3f;
			}
			
			var fps = 16.0f;
			var frames = 6f;
			var modTime = Modf(beatTime * fps, frames)/frames;
			var beatBounce = Mathf.Abs (Mathf.Sin (Mathf.PI * modTime));
			var beatScale = Lerp(0.9f, 1f, beatBounce);
			
			var k = scaleY * beatScale;
			
			var m = -0.3f;
			var sx = m * k + (1f-m);
			fx.localScale=Vec(sx, k, sx);
		}
		
		// shadow
		if (patch == null) {
			shadow.gameObject.SetActive(false);
		} else {
			shadow.gameObject.SetActive(true);
			
			shadow.position = Vec(p.x, patchHeight, p.z);
			shadow.rotation = Quaternion.LookRotation(Vec(0, patchDeriv, 1f));
	
			var dh = Mathf.Max (p.y - patchHeight, 0f);
			var scale = 1f / (1f + 0.5f * dh);
			shadow.localScale = Vec(scale, scale, scale);
		}
		
		// cone
		{
			var anchorPosition = coneAnchor.position;
			var conePosition = anchorPosition;
			
			if (velocity.y > 0f && coneY < anchorPosition.y) {
				coneSpeed = Mathf.Max (1.01f * velocity.y, coneSpeed);
			} else {
				coneSpeed += 0.9f * Physics.gravity.y * dt;
			}
			
			coneY += coneSpeed * dt;
			if (anchorPosition.y > coneY) {
				coneY = anchorPosition.y;
				if (coneSpeed < 0f) {
					coneSpeed = -0.25f * coneSpeed;
				} else {
					coneSpeed = 0f;
				}
			} else {
				var topY = anchorPosition.y + 2.5f;
				if (coneY > topY) {
					coneSpeed += 2.0f * (topY - coneY) * dt;
				}
				
			}
			
			var fps = 16.0f;
			var frames = 6f;
			var modTime = Modf((beatTime+5.5f) * fps, frames)/frames;
			var beatBounce = Mathf.Abs (Mathf.Sin (Mathf.PI * modTime));
			var k = Lerp(0.9f, 1f, beatBounce);
			
			
			conePosition.y = coneY;
			if (coneY - anchorPosition.y < 0.4f) conePosition.y += 0.2f * beatBounce;
			//conePosition.y += 0.25f * beatBounce;
			cone.position = conePosition;
			cone.rotation = Cam.inst.baseRotation;
			
			var m = -0.3f;
			var sx = m * k + (1f-m);
			cone.localScale=Vec(sx, k, sx);
			
		}		
		
		PickPOV(preWibble);
	}
	
	void PickPOV(Quaternion preWibble) {
		var rot = preWibble.eulerAngles;
		var crot = Cam.inst.baseRotation.eulerAngles;
		var rel = rot - crot;
		rel.y = Mathf.DeltaAngle(rel.y, 0f);
		if (rel.y > 0f) {
			fx00.enabled = true;
			fx45.enabled = false;
			fx60.enabled = false;
			frontWheel.enabled = false;
		} else if (rel.y > -30f) {
			fx00.enabled = false;
			fx45.enabled = true;
			fx60.enabled = false;
			frontWheel.enabled = true;
		} else {
			fx00.enabled = false;
			fx45.enabled = false;
			fx60.enabled = true;
			frontWheel.enabled = true;
		}
		
		var coneAngle = Mathf.FloorToInt((-rel.y+4f) / 8f);
		coneAngle = Mathf.Clamp (coneAngle, 0, creams.Length-1);
		coneRenderer.sprite = creams[coneAngle];
	}
	
	void OnTriggerEnter(Collider c) {
		if (status != Status.Active) return;
		switch(c.gameObject.layer) {
			case Victim.Layer:
				HitVictim(c.GetComponent<Victim>());
				break;
			case Hazard.Layer:
				HitHazard(c.GetComponent<Hazard>());
				break;
		}
	}
	
	void HitVictim(Victim vic) {
		if (vic.TryHit(body.position, velocity)) {
			hitTime = Time.time;
			
			HUD.inst.OnHitVictim();
			
			bloodSprayer.Play();
			Cam.inst.Shake();
			
			if (grounded) {
				grounded = false;
				velocity.y = 2.0f;
			}
			//speed *= 0.25f;
			//velocity *= 0.25f;
		}
	}
	
	void HitHazard(Hazard haz) {
		if (TimeSince(respawnTime) > 2f) {
			TryExplode();
			haz.WasHit();
		}
	}
	
}
