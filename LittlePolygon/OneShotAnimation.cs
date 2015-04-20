using System;
using System.Collections;
using UnityEngine;

namespace LittlePolygon {
	
	public class OneShotAnimation : PooledObject {
		public SpriteRenderer spr;
		public Sprite[] frames;
		public float framesPerSecond = 30f;
		
		[NonSerialized] public float time;
		
		public OneShotAnimation CreateOneShotAnimation(Vector3 position) {
			var result = Alloc(position) as OneShotAnimation;
			result.spr.sprite = result.frames[0];
			result.time = 0f;
			return result;
		}
		
		protected void Update() {
			time += Time.deltaTime;
			int nextFrame = (int)(time * framesPerSecond);
			if (nextFrame < frames.Length) {
				spr.sprite = frames[nextFrame];
			} else {
				Release();
			}
		}
		
	}
	
}