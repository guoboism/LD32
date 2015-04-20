using UnityEngine;
using System.Collections;

namespace LittlePolygon {
	
	public class RealTime : CustomBehaviour {
		
		// Get real dt, unaffected by time scale
		public static float deltaTime { get { return inst.dt; } }
		
		public static void Touch() {
			if (inst == null) {
				DontDestroyOnLoad(
					new GameObject("RealTime", typeof(RealTime))
				);
			}
		}
		
		//--------------------------------------------------------------------------------
		// BORING PRIVATE DETAILS
		//--------------------------------------------------------------------------------	
		
		static RealTime inst;
		float prevTime;
		float dt;
		
		void Awake() {
			inst = this;
			prevTime = Time.realtimeSinceStartup;
			gameObject.hideFlags = HideFlags.HideInHierarchy;
			dt = 0f;
		}
		
		void OnDestroy() {
			if (inst == this) { inst = null; }
		}
		
		void Update() {
			var nextTime = Time.realtimeSinceStartup;
			dt = nextTime - prevTime;
			prevTime = nextTime;
		}
		
	}
	
}
