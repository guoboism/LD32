/*

The MIT License (MIT)
	
Copyright (c) 2014 Little Polygon LLC
		
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/		

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Diagnostics = System.Diagnostics;
using uRandom = UnityEngine.Random;
using uObject = UnityEngine.Object;

namespace LittlePolygon {	

	// MonoBehaviour shim; no additional cost (all methods are static), but hella convenient
	public class CustomBehaviour : MonoBehaviour {
		
		// TAU MANIFESTO!
		public static float TAU = Mathf.PI + Mathf.PI;
		
		// Shader-Style Vector Shorthand
		public static Vector2 Vec(float x, float y) { return new Vector2(x, y); }
		public static Vector3 Vec(float x, float y, float z) { return new Vector3(x, y, z); }
		public static Vector3 Vec(float x, Vector2 yz) { return new Vector3(x, yz.x, yz.y); }
		public static Vector3 Vec(Vector2 v, float z) { return new Vector3(v.x, v.y, z); }
		public static Vector4 Vec(float x, float y, float z, float w) { return new Vector4(x, y, z, w); }
		public static Vector4 Vec(Vector3 v, float w) { return new Vector4(v.x, v.y, v.z, w); }
			
		public static Vector2 Cmul(Vector2 u, Vector2 v) { return new Vector2(u.x*v.x-u.y*v.y, u.x*v.y + u.y*v.x); }
	
		public static Vector2 PolarVec(float r, float theta) { return r * (new Vector2(Mathf.Cos(theta), Mathf.Sin(theta))); }
		public static Vector2 UnitVec(float theta) { return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)); }
	
		public static Quaternion RandomOrientation()  { 
			return Quaternion.AngleAxis(360f * uRandom.value, uRandom.onUnitSphere); 
		}
	
		public static Quaternion QDegrees(float degrees) { return Quaternion.AngleAxis(degrees, Vector3.forward); }
		public static Quaternion QRadians(float radians) { return Quaternion.AngleAxis(radians * Mathf.Rad2Deg, Vector3.forward); }
		public static Quaternion QDir(Vector2 dir) { return Quaternion.AngleAxis(dir.Degrees(), Vector3.forward); }
		
		// time funcs
		public static float TimeSince(float t) { return Time.time - t; }
		public static float RealTimeSince(float t) { return Time.realtimeSinceStartup - t; }
		public static float FixedTimeSince(float t) { return Time.fixedTime - t; }
		
		// aliases to random funcs, because System and UnityEngine have naming conflicts and
		// using fully-qualified names constantly in math expressions is a pain
		public static float GetRand() { return UnityEngine.Random.value; }
		public static float GetRand(float u, float v) { return UnityEngine.Random.Range(u, v); }
		public static float GetRand(int x, int y) { return UnityEngine.Random.Range(x, y); }
		
		// Runtime assertions which only run in the editor
		#if UNITY_EDITOR
		public static void Assert(bool cond) { 
			if (!cond) {
				Debug.LogError("Assertion Failed, Dawg");
				Application.Quit();
			}
		}
		#endif
		
		// Some easing functions
		public static float Parabola(float x) { return 1f - (x=1f-x-x)*(x); }
		public static float ParabolaDeriv(float x) { return 4f*(1f-x-x); }
		public static float EaseIn2(float u) { return u*u; }
		public static float EaseIn4(float u) { return u*u*u*u; }
		public static float EaseOut2(float u) { return 1f-(u=1f-u)*u; }
		public static float EaseOut4(float u) { return 1f-(u=1f-u)*u*u*u; }
		public static float SmoothStep(float u) { return 3f * u * u - 2f * u * u * u; }
		public static float SmoothStepDeriv(float u) { return 6f * u - 6f * u * u; }
		
		
		public static float Modf(float x, float radix) {
			return ((x % radix) + radix) % radix;
		}
		
		public static float EaseOutElastic(float t, float p=0.3f) {
			return Mathf.Pow(2f,-10f*t) * Mathf.Sin((t-p/4f)*(2f*Mathf.PI)/p) + 1f;
		}
		
		public static float EaseInOutBack(float t)  {
			var v = t + t;
			var s = 1.70158f * 1.525f;
			if (v < 1.0f) {
				return 0.5f * (v * v * ((s + 1.0f) * v - s));
			} else {
				v -= 2.0f;
				return 0.5f * (v * v * ((s + 1.0f) * v + s) + 2.0f);
			}
		}
		
		public static float EaseOutBack(float t) { 
			t-=1.0f; 
			return t*t*((1.70158f+1.0f)*t + 1.70158f) + 1.0f; 
		}
		
		public static float AdjustEasingRate(float easing, float dt) {
			return 1f - Mathf.Pow(1f - easing, 60f * dt);
		}
		

		public float Lerp(float v0, float v1, float u) { return v0 + u * (v1 - v0); }
		public Vector2 Lerp(Vector2 v0, Vector2 v1, float u) { return v0 + u * (v1 - v0); }
		public Vector3 Lerp(Vector3 v0, Vector3 v1, float u) { return v0 + u * (v1 - v0); }
		
		public static float Expovariate(float dt, float uMin=0.05f, float uMax=0.95f) { 
			return -dt * Mathf.Log(1f-uRandom.Range(uMin,uMax)); 
		}
		
		// Generic versions of Unity calls that take a type argument
		public static T Dup<T> (T obj) where T : uObject { return Instantiate(obj) as T; }
		public static T Dup<T> (T obj, Vector3 pos) where T : uObject { return Instantiate(obj, pos, Quaternion.identity) as T; }
		public static T Dup<T> (T obj, Vector3 pos, Quaternion q) where T : uObject {  return Instantiate(obj, pos, q) as T; }
		
		public static T CreateInstance<T> (string name) where T : uObject { return Dup<T>(Resources.Load<T>(name)); }
		
		public static T FindObject<T> () where T : UnityEngine.Object { return GameObject.FindObjectOfType<T>(); }
		public static T[] FindObjects<T>() where T : UnityEngine.Object { return (T[]) GameObject.FindObjectsOfType<T>(); }
			
		// Prolly in the std lib, but whatever
		public static void Swap<T>(ref T u, ref T v)  {
			var tmp = u;
			u = v;
			v = tmp;
		}
		
		// Easy color literals
		public static Color32 RGB(uint hex) {
			return new Color32(
				(byte)((0xff0000 & hex) >> 16),
				(byte)((0x00ff00 & hex) >>  8),
				(byte)((0x0000ff & hex)      ),
				(byte)255
			);		
		}
		
		public static Color32 RGBA(uint hex) {
			return new Color32(
				(byte)((0xff000000 & hex) >> 24),
				(byte)((0x00ff0000 & hex) >> 16),
				(byte)((0x0000ff00 & hex) >>  8),
				(byte)((0x000000ff & hex)      )
			);
		}
		
		public static Color RGB(float r, float g, float b) { return new Color(r, g, b); }
		public static Color RGBA(float r, float g, float b, float a) { return new Color(r, g, b, a); }
		public static Color RGBA(Color c, float a) { return new Color(c.r, c.g, c.b, a); }
		
		public static IEnumerable<float> Transition(float duration) {
			var k = 1f / duration;
			for(var t=0f; t<duration; t+=Time.deltaTime) {
				yield return k * t;
			}
			yield return 1f;
		}
		
		public static IEnumerable<float> RawTransition(float duration) {
			var k = 1f / duration;
			var startTime = Time.realtimeSinceStartup;
			var deadline = startTime + duration;
			for(var t=startTime; t<deadline; t=Time.realtimeSinceStartup) {
				yield return k * (t - startTime);
			}
			yield return 1f;
		}
		
		public static Vector2 Abs(Vector2 v)  { return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y)); }
		public static Vector3 Abs(Vector3 v)  { return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z)); }
		public static Vector2 Min(Vector2 u, Vector2 v)  { return new Vector2(Mathf.Min(u.x, v.x), Mathf.Min(u.y, v.y)); }
		public static Vector3 Min(Vector3 u, Vector3 v)  { return new Vector3(Mathf.Min(u.x, v.x), Mathf.Min(u.y, v.y), Mathf.Min(u.z, v.z)); }
		public static Vector2 Max(Vector2 u, Vector2 v)  { return new Vector2(Mathf.Max(u.x, v.x), Mathf.Max(u.y, v.y)); }
		public static Vector3 Max(Vector3 u, Vector3 v)  { return new Vector3(Mathf.Max(u.x, v.x), Mathf.Max(u.y, v.y), Mathf.Max(u.z, v.z)); }
		
		
		public static void DrawArcGizmo(float radius, float a0, float a1) {
			var da = a1 - a0;
			var curr = PolarVec(radius, a0);
			var rotor = PolarVec(1f, da/49f);
			for(int i=0; i<50; ++i) {
				var next = Cmul(curr, rotor);
				if (i % 2 == 0) {
					Gizmos.DrawLine(curr, next);
				}
				curr = next;
			}	
		}
		
		public static void DrawArrowGizmo(Vector3 p0, Vector3 p1, float r=1f) {
			Gizmos.DrawLine(p0, p1);
			var u = r * (p1 - p0).normalized;
			var wibble = UnitVec(0.2f*Mathf.PI);
			Gizmos.DrawLine(p1, p1 - Vec(Cmul(u.xy(), wibble.Conjugate()), 0));
			Gizmos.DrawLine(p1, p1 - Vec(Cmul(u.xy(), wibble), 0));
		}
		
		public static bool Approx(Color c1, Color c2) {
			return Mathf.Approximately(c1.r, c2.r) && 
			       Mathf.Approximately(c1.g, c2.g) && 
			       Mathf.Approximately(c1.b, c2.b) && 
			       Mathf.Approximately(c1.a, c2.a) ;
				
		}
		
	}
	
}
	
