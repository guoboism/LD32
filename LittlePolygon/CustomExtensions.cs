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
using CB = LittlePolygon.CustomBehaviour;
using uRandom = UnityEngine.Random;

namespace LittlePolygon
{
	
	// Convenience extension methods for common unity objects
	public static class CustomExtensions {
	
		// Shader-style swizzling
		public static Vector2 yx(this Vector2 v) { return new Vector2(v.y, v.x); }
		public static Vector2 xy(this Vector3 v) { return new Vector3(v.x, v.y); }
		public static Vector2 yx(this Vector3 v) { return new Vector3(v.y, v.x); }
		public static Vector2 xz(this Vector3 v) { return new Vector3(v.x, v.z); }
		public static Vector2 yz(this Vector3 v) { return new Vector3(v.y, v.z); }
		public static Vector2 zx(this Vector3 v) { return new Vector3(v.z, v.x); }
		public static Vector2 zy(this Vector3 v) { return new Vector3(v.z, v.y); }
		public static Vector3 xy0(this Vector3 v) { return new Vector3(v.x, v.y, 0); }
		public static Vector3 x0z(this Vector3 v) { return new Vector3(v.x, 0, v.z); }
	
		public static Vector2 Clockwise(this Vector2 v) { return new Vector2(-v.y, v.x); }
		public static Vector2 Anticlockwise(this Vector2 v) { return new Vector2(v.y, -v.x); }
	
		public static Vector3 Above(this Vector3 v, float dh) { return new Vector3(v.x, v.y+dh, v.z); }
		public static Vector3 Below(this Vector3 v, float dh) { return new Vector3(v.x, v.y-dh, v.z); }
		public static Vector3 Ahead(this Vector3 v, float dz) { return new Vector3(v.x, v.y, v.z+dz); }
		public static Vector3 Behind(this Vector3 v, float dz) { return new Vector3(v.x, v.y, v.z-dz); }
		public static Vector3 Left(this Vector3 v, float dx) { return new Vector3(v.x-dx, v.y, v.z); }
		public static Vector3 Right(this Vector3 v, float dx) { return new Vector3(v.x+dx, v.y, v.z); }
		
		public static Vector2 Conjugate(this Vector2 v) { return new Vector2(v.x, -v.y); }		
		public static float Radians(this Vector2 v) { return Mathf.Atan2(v.y, v.x); }
		public static float Degrees(this Vector2 v) { return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg; }
		
		
		public static T TakeFirst<T>(this IEnumerable<T> coll) {
			var i = coll.GetEnumerator();
			i.MoveNext();
			return i.Current;
		}
		
		public static T TakeRandom<T>(this IList<T> li) {
			return li[uRandom.Range(0, li.Count)];
		}
		
		// Random shuffle
		public static void Shuffle<T>(this IList<T> list) {  
			var n = list.Count;  
			while (n > 1) {  
				n--;  
				var k = uRandom.Range(0, n+1);
				var value = list[k];  
				list[k] = list[n];  
				list[n] = value;  
			}  
		}
		
		public static T PeekLast<T>(this IList<T> list) {
			return list[list.Count-1];
		}
		
		// Treating lists as stacks
		public static T PopLast<T>(this IList<T> list) {
			var result = list[list.Count-1];
			list.RemoveAt(list.Count-1);
			return result;
		}
	
		// Time-independent Easing
		public static float AdjustEasingRate(this float easing) {
			return CustomBehaviour.AdjustEasingRate(easing, Time.deltaTime);
		}
		
		public static float EaseTowards(this float curr, float target, float easing) {
			return Mathf.Lerp(curr, target, AdjustEasingRate(easing));
		}
		
		public static Quaternion EaseTowards(this Quaternion curr, Quaternion target, float easing) {
			return Quaternion.Slerp(curr, target, AdjustEasingRate(easing));
		}
		
		public static Vector2 EaseTowards(this Vector2 curr, Vector2 target, float easing) {
			return Vector2.Lerp(curr, target, AdjustEasingRate(easing));
		}
		
		public static Vector3 EaseTowards(this Vector3 curr, Vector3 target, float easing) {
			return Vector3.Lerp(curr, target, AdjustEasingRate(easing));
		}
	
		public static Vector4 EaseTowards(this Vector4 curr, Vector4 target, float easing) {
			return Vector4.Lerp(curr, target, AdjustEasingRate(easing));
		}
		
		public static Color EaseTowards(this Color curr, Color target, float easing)  {
			return Color.Lerp(curr, target, AdjustEasingRate(easing));
		}
		
		// Like the UI command
		public static void Reset(this Transform t) {
			t.localPosition = Vector3.zero;
			t.localScale = Vector3.one;
			t.localRotation = Quaternion.identity;
		}
		
		public static void SetToMatch(this Transform t, Transform other) {
			t.position = other.position;
			t.localScale = other.lossyScale;
			t.rotation = other.rotation;
		}
		
		public static float ToFloat(this int x) {
			return (float) x;
		}
		
		public static void SetColor(this Mesh mesh, Color32 color) {
			var c = mesh.colors32;
			if (c == null) {
				c = new Color32[mesh.vertexCount];
			}
			for(int i=0; i<c.Length; ++i) {
				c[i] = color;
			}
			mesh.colors32 = c;
		}
		
		
		public static Color GetColor(this Mesh mesh) {
			var c = mesh.colors32;
			if (c == null) {
				c = new Color32[mesh.vertexCount];
				for(int i=0; i<c.Length; ++i) {
					c[i] = Color.white;
				}
				mesh.colors32 = c;
			}
			return c[0];
		}
		
		public static void SetOpacity(this SpriteRenderer spr, float u) {
			var c = spr.color;
			spr.color = new Color(c.r, c.g, c.b, u);
		}
		
		public static void SetOpacity(this TextMesh text, float u) {
			var c = text.color;
			text.color = new Color(c.r, c.g, c.b, u);
		}
		
		public static void SetOpacity(this Material mat, float u) {
			var c = mat.color;
			mat.color = new Color(c.r, c.g, c.b, u);
		}
		
		public static void SetTint(this SpriteRenderer spr, Color c) {
			spr.color = new Color(c.r, c.g, c.b, spr.color.a);
		}
		
		public static void SetTint(this TextMesh text, Color c) {
			text.color = new Color(c.r, c.g, c.b, text.color.a);
		}
		
		public static void SetTint(this Material mat, Color c) {
			mat.color = new Color(c.r, c.g, c.b, mat.color.a);
		}
		
		
		public static Transform InsertShadowRoot(this Transform t) {
			// Inserts a transform between this node and all it's children with a identity-local 
			// transform for doing "local" transforms that doesn't affect the root location
			var result = new GameObject("Shadow").GetComponent<Transform>();
			
			var children = new Transform[t.childCount];
			for(int i=0; i<t.childCount; ++i) {
				children[i] = t.GetChild(i);
			}
			result.parent = t;
			result.Reset();
			foreach(var child in children) {
				child.parent = result;
			}
			return result;
		}
		
		public static T GetInterface<T>(this Component com) where T : class {
			return com.GetComponent(typeof(T)) as T;
		}
	}
	
	
}


