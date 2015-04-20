
using LittlePolygon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RNG = UnityEngine.Random;

public class Patch {

	static int ID = 0;

	public GameObject go, goSide;
	public float x0, x1;
	public float z0, z1;
	public Func<float,float> h;
	public Func<float,float> d;
	public List<GameObject> localObjects = new List<GameObject>();
	internal Patch prev, next;
	
	public Patch(Transform parent) {
		go = new GameObject("Patch"+ID, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
		go.transform.parent = parent;
		Filter.sharedMesh = new Mesh() { name = go.name };
						
		goSide = new GameObject("PatchSide"+ID, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
		goSide.transform.parent = go.transform;
		SideFilter.sharedMesh = new Mesh() {name = goSide.name };
		
		ID++;
		
		
	}
	
	public MeshRenderer Renderer { get { return go.GetComponent<MeshRenderer>(); } }
	public MeshFilter Filter { get { return go.GetComponent<MeshFilter>(); } }
	public MeshCollider Collider { get { return go.GetComponent<MeshCollider>(); } }
	public MeshRenderer SideRenderer { get { return goSide.GetComponent<MeshRenderer>(); } }
	public MeshFilter SideFilter { get { return goSide.GetComponent<MeshFilter>(); } }
	public MeshCollider SideCollider { get { return goSide.GetComponent<MeshCollider>(); } }
	
	public float HeightAtZ(float z) {
		return h((z-z0)/(z1-z0));
	}
	
	public float DerivAtZ(float z) {
		return d((z-z0)/(z1-z0)) / (z1-z0);
	}
	
	public bool Contains(float x, float z) {
		return x >= x0 && x < x1 && z >= z0 && z < z1;
	}
	
	public float Y0 { get { return h(0f); } }
	public float Y1 { get { return h(1f); } }
	
	public void Disable(World w) {
		foreach(var obj in localObjects) {
			if (obj) { GameObject.Destroy(obj); }
		}
		
		if (w.headPatch == this) { w.headPatch = next; }
		if (next != null) { next.prev = prev; }
		if (prev != null) { prev.next = next; }
		
		GameObject.Destroy(go);
	}
	
}

public class Segment {
	
	public enum Kind {
		Flat, Branch, Slope, Hill, Ramp
	}
	public const int NumKinds = 5;
	
	public Kind kind;
	public Patch[] patches;
	internal Segment next;
	
	public Segment(Kind kind, params Patch[] patches) {
		this.kind = kind;
		this.patches = patches;
	}
	
	public float Z1 { get { return patches[0].z1; } }
	
	public void Dispose(World w) {
		int len = patches.Length;
		for(int i=0; i<len; ++i) {
			patches[i].Disable (w);
		}
	}
	
	public Patch RandomPatch() {
		return patches[RNG.Range(0, patches.Length)];
	}

}


public class World : CustomBehaviour {

	public float extent = 10f;
	
	public Victim[] victimPrefabs;
	public Hazard[] hazardPrefabs;
	
	public Material defaultGround;
	public Material defaultSide;
	public PhysicMaterial surfMat;
	public int patchResolution = 128;
	
	public static World inst;
	MeshScratchpad pad = new MeshScratchpad();
	
	
	public Patch headPatch = null;
	public Segment headSegment = null, tailSegment = null;
	
	internal bool spawnVictims = true;
	internal float hazardMulti = 1f;
	
	
	public Patch FindPatch(float x, float z) {
		for(var p=headPatch; p!=null; p=p.next) {
			if (p.Contains(x,z)) {
				return p;
			}
		}
		return null;
	}
	
	void Awake() {
		inst = this;
	}
	
	void OnDestroy() {
		if (inst == this) {
			inst = null;
		}
	}
	
	void Start() {
		pad.Init (2 * patchResolution);
		pad.PlotTriangleStrip(2 * patchResolution);
		
		spawnVictims = false;
		headSegment = tailSegment = new Segment(Segment.Kind.Flat, AddFlatPatch(-extent, extent, -10, 50, 0f));
		spawnVictims = true;

	}
	
	void Update() {
		
		// cull old segments?
		var cz = Cam.inst.transform.position.z;
		while(headSegment != tailSegment && headSegment.Z1 < cz) {
			var s = headSegment;
			headSegment = s.next;
			s.Dispose(this);
		}
		
		// add new segments?
		while (tailSegment != null && tailSegment.Z1 < cz + 300f) {
			AppendSegment();
		}
	}
	
	bool didInitialHill = false;
	
	public void AppendSegment() {
		var z = tailSegment.Z1;
		
		if (Hero.inst.initialWait) {
		
			// keep plotting flats
			hazardMulti = 0.5f;
			if (RNG.value < 0.45f) {
				DoAppend(new Segment(Segment.Kind.Flat, AddFlatPatch(-extent, extent, z, z+50, 0)));
			} else {
				DoAppend(new Segment(Segment.Kind.Hill, AddCosHill(-extent, extent, z, z+RNG.Range(60f, 75f), 0, 0.5f)));
			}
			hazardMulti = 1f;
		
		} else if (!didInitialHill) {
			
			// after the initial flat bit, show a cool hill for a "dukes of hazard" moment early on
			hazardMulti = 0f;
			spawnVictims = false;
			DoAppend(new Segment(Segment.Kind.Slope, AddSmoothSlop(-extent, extent, z, z+25, 0f, 10f)));
			spawnVictims = true;
			DoAppend(new Segment(Segment.Kind.Flat, AddFlatPatch(-extent, extent, z+25, z+125, 10f)));
			didInitialHill = true;
			hazardMulti = 1f;
			
		} else {
		
			// pick next kind
			var nextKind = Segment.Kind.Flat;
			if (tailSegment.kind == Segment.Kind.Hill) {
				
				// anything can come next
				nextKind = (Segment.Kind) RNG.Range(0, Segment.NumKinds);
				
			} else if (tailSegment.kind != Segment.Kind.Ramp) {
				
				// randomly pick a different kind and a patch to "continue from"
				var kindIdx = RNG.Range(0, Segment.NumKinds - 1);
				var prevIdx = (int) tailSegment.kind;
				if (kindIdx >= prevIdx) kindIdx++;
				nextKind = (Segment.Kind) kindIdx;

			}

			// plot next kind
			switch(nextKind) {
				
			case Segment.Kind.Flat: {
				var len = RNG.Range (25f, 100f);
				var y = tailSegment.kind == Segment.Kind.Ramp ?
					tailSegment.patches[0].Y0 :
					tailSegment.patches[0].Y1 ;
				DoAppend(new Segment(Segment.Kind.Flat, AddFlatPatch(-extent, extent, z, z+len, y)));
				break;
			}
				
			case Segment.Kind.Hill: {
				var len = RNG.Range(100f, 200f);
				var y = tailSegment.patches[0].Y1;
				var h = RNG.Range(2f, 5f);
				DoAppend(new Segment(Segment.Kind.Hill, AddCosHill(-extent, extent, z, z+len, y, h)));
				break;
			}
				
			case Segment.Kind.Ramp: {
				var len = 25f;
				var y = tailSegment.patches[0].Y1;
				var h = RNG.Range(1f, 2.5f);
				DoAppend(new Segment(Segment.Kind.Ramp, AddRamp(-extent, extent, z, z+len, y, h)));
				break;
			}
				
			case Segment.Kind.Slope: {
				var len = RNG.Range(50f, 100f);
				var y = tailSegment.patches[0].Y1;
				var dh = RNG.Range(5f, 10f);
				if (y > 20f) { dh = -dh; }
				DoAppend(new Segment(Segment.Kind.Slope, AddSmoothSlop(-extent, extent, z, z+len, y, y+dh)));
				break;
			}
				
			case Segment.Kind.Branch: {
				AppendBranch();
				break;
			}
			}	
		}
	
	}
	
	enum BranchKind {
		
		FlatHill,
		FlatRamp,
		HillHill,
		FlatHillHill,
		HillHillHill,
		FlatHillRamp,
		FlatRampRamp,
		//SlopeSlope,
		//SlopeSlopeSlope,
		
		Count
	}
	
	void AppendBranch() {
		var kind = (BranchKind) RNG.Range(0, (int)BranchKind.Count);
		
		var len = RNG.Range(100f, 200f);
		var dh1 = RNG.Range(4f, 8f);
		var dh2 = dh1 + RNG.Range(2f, 4f);
		var dh3 = dh2 + RNG.Range (1f, 2f);
		
		Patch p1 = null;
		Patch p2 = null;
		Patch p3 = null;
		
		var z0 = tailSegment.Z1;
		var z1 = z0 + len;
		var y = tailSegment.patches[0].Y1;
		
		var ml = Lerp(-extent, extent, 0.3333f);
		var mr = Lerp(-extent, extent, 0.6666f);
		
		switch(kind) {

		case BranchKind.FlatHill: {
			p1 = AddFlatPatch(0, extent, z0, z1, y);
			p2 = AddCosHill(-extent, 0, z0, z1, y, dh1);
			break;
		}
				
		case BranchKind.FlatRamp: {
			p1 = AddFlatPatch(0, extent, z0, z1, y);
			p2 = AddRamp(-extent, 0, z0, z1, y, dh1);
			break;
		}

		case BranchKind.HillHill: {
			p1 = AddCosHill(0, extent, z0, z1, y, dh1);
			p2 = AddCosHill(-extent, 0, z0, z1, y, dh2);
			break;
		}

		case BranchKind.FlatHillHill: {
			p1 = AddFlatPatch(mr, extent, z0, z1, y);
			p2 = AddCosHill(ml, mr, z0, z1, y, dh1);
			p3 = AddCosHill(-extent, ml, z0, z1, y, dh2);
			break;
		}
			
		case BranchKind.HillHillHill: {
			p1 = AddCosHill(mr, extent, z0, z1, y, dh1);
			p2 = AddCosHill(ml, mr, z0, z1, y, dh2);
			p3 = AddCosHill(-extent, ml, z0, z1, y, dh3);
			break;
		}
			
		case BranchKind.FlatHillRamp: {
			p1 = AddFlatPatch(mr, extent, z0, z1, y);
			p2 = AddRamp(ml, mr, z0, z1, y, dh1);
			p3 = AddCosHill(-extent, ml, z0, z1, y, dh2);
			break;
		}

		case BranchKind.FlatRampRamp: {
			p1 = AddFlatPatch(mr, extent, z0, z1, y);
			p2 = AddRamp(ml, mr, z0, z1, y, dh1);
			p3 = AddRamp(-extent, ml, z0, z1, y, dh2);			
			break;
		}
		default:
			break;	
		}	
		
		if (p1 == null) {
			return;
		} else if (p3 == null) {
			DoAppend(new Segment(Segment.Kind.Branch, p1, p2));
		} else {
			DoAppend(new Segment(Segment.Kind.Branch, p1, p2, p3));
		}	
		
	}
	
	void DoAppend(Segment s) {
		tailSegment.next = s;
		tailSegment = s;
	}
	
	Patch AddFlatPatch(float x0, float x1, float z0, float z1, float y0) {
		return PlotPatch(x0, x1, z0, z1, 
			(u)=>y0,
			(u)=>0
		);
	}
	
	Patch AddSmoothSlop(float x0, float x1, float z0, float z1, float y0, float y1) {
		return PlotPatch(x0, x1, z0, z1, 
			(u) => Lerp(y0, y1, SmoothStep(u)), 
			(u) => (y1-y0) * SmoothStepDeriv(u)
		);
	}
	
	Patch AddCosHill(float x0, float x1, float z0, float z1, float y0, float h) {
		return PlotPatch(x0, x1, z0, z1,
			(u) => Lerp(y0, y0+h, 1f-Mathf.Cos (2f*Mathf.PI*u)),
			(u) => h * (2f*Mathf.PI*Mathf.Sin (2f*Mathf.PI*u))
		);
	}
	
	Patch AddRamp(float x0, float x1, float z0, float z1, float y0, float h) {
		var ph = hazardMulti;
		hazardMulti = 0f;
		var result = PlotPatch(x0, x1, z0, z1,
			(u) => Lerp(y0, y0+h, u*u),
			(u) => h * 2f * u
		);
		hazardMulti = ph;
		return result;
	}
	
	Patch PlotPatch(float x0, float x1, float z0, float z1, Func<float,float> h, Func<float,float> d) {
		Patch result = new Patch(transform);
		
		var du = 1f/(patchResolution-1f);
		
		// plot top
		var u=0f;
		for(int i=0; i<patchResolution; ++i) {
			pad.vbuf[i+i  ] = Vec(x1, h(u), Mathf.Lerp (z0, z1, u));
			pad.vbuf[i+i+1] = Vec(x0, h(u), Mathf.Lerp (z0, z1, u));
			pad.nbuf[i+i  ] = Vector3.up;
			pad.nbuf[i+i+1] = Vector3.up;
			u += du;
		}
		float metersPerRepeat = 5f;
		var repeatZ = Mathf.FloorToInt((z1-z0)/metersPerRepeat);
		pad.SetRepeatingStripTexture(Mathf.FloorToInt((x1-x0)/metersPerRepeat), repeatZ);
		pad.ApplyToMesh(result.Filter.sharedMesh);
		result.Collider.sharedMesh = result.Filter.sharedMesh;
		
		// plot side
		u=0f;
		for(int i=0; i<patchResolution; ++i) {
			pad.vbuf[i+i  ] = Vec(x1, h(u)-10f, Mathf.Lerp (z0, z1, u));
			pad.vbuf[i+i+1] = Vec(x1, h(u), Mathf.Lerp (z0, z1, u));
			pad.nbuf[i+i  ] = Vector3.up;
			pad.nbuf[i+i+1] = Vector3.up;
			u += du;
		}
		pad.SetRampTexture(0f, 1f, repeatZ/2);
		pad.ApplyToMesh(result.SideFilter.sharedMesh);
		result.SideCollider.sharedMesh = result.SideFilter.sharedMesh;
		
		result.Renderer.sharedMaterial = defaultGround;
		result.SideRenderer.sharedMaterial = defaultSide;
		
		// set props
		result.x0 = x0;
		result.x1 = x1;
		result.z0 = z0;
		result.z1 = z1;
		result.h = h;
		result.d = d;
		
		// add victims
		var area = (x1-x0) * (z1-z0);
		if (spawnVictims) {
		
			var density = RNG.Range(0.001f, 0.005f);
			//var density = 0.01f;
			var victimCount = Mathf.FloorToInt(area * density);
			for(var i=0; i<victimCount; ++i) {
				var kind = RNG.Range(0, victimPrefabs.Length);
				victimPrefabs[kind].Create(result);
			}
			
		}
		
		// add hazards
		{
			var r = 1f - RNG.value;
			var density = Lerp(0.001f, 0.002f, r*r);
			var hazardCount = Mathf.FloorToInt(hazardMulti * area * density);
			for(int i=0; i<hazardCount; ++i) {
				var x = RNG.Range (x0+0.75f, x1-0.75f);
				var z = RNG.Range (z0, z1);
				var y = result.HeightAtZ(z);
				var kind = RNG.Range(0, hazardPrefabs.Length);
				var rot = Quaternion.identity;
				var prefab = hazardPrefabs[kind];
				
				// hacks
				if (prefab.isAnamorphic) {
					// only place these rocks on the left side
					if (x > 0f) {
						rot = Quaternion.AngleAxis(40f, Vector3.up);
					} else {
						rot = Quaternion.AngleAxis(RNG.Range (0f, 20f), Vector3.up);
					}
				} else {
					rot = Cam.inst.baseRotation;
				}
					
				var hazard = Dup(prefab, Vec(x,y,z), rot);
				hazard.transform.localScale = RNG.Range(hazard.scaleMin, hazard.scaleMax) * Vector3.one;
				result.localObjects.Add(hazard.gameObject);
				
			}
		}
		
		// handle query results
		result.next = headPatch;
		result.prev = null;
		if (headPatch != null) { headPatch.prev = result; }
		headPatch = result;
			
		return result;
	}
	

}






