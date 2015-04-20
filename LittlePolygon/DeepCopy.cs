using UnityEngine;
using System.Collections;

public static class CopyExt {
	
	// Copies an entire transform heirarchy as well as any contained sprites.
	// Doing this with recursive functions for now; will fix if it actually 
	// proves to be problematic :P
	public static Transform DeepCopy(this Transform transform) {
				
		// prepopulate sprite if one is attached to the source and enabled
		GameObject go = null;
		var name = transform.name + " (DeepCopy)";
		var s1 = transform.GetComponent<SpriteRenderer>();
		if (s1 == null || !s1.enabled) {
			go = new GameObject(name);
		} else {
			go = new GameObject(name, typeof(SpriteRenderer));
			var s2 = go.GetComponent<SpriteRenderer>();
			s2.sprite = s1.sprite;
			s2.material = s1.material;
			s2.color = s1.color;
		}		
		
		var result = go.GetComponent<Transform>();
		
		// recursively deep-copy children
		for(int i=0; i<transform.childCount; ++i) {
			var child = transform.GetChild(i);
			if (child.gameObject.activeInHierarchy) {
				child.DeepCopy().parent = result;
			}
		}

		// copy location
		result.localScale = transform.localScale;
		result.localRotation = transform.localRotation;
		result.localPosition = transform.localPosition;
		
		return result;
	}
	
	public static Transform Copy(this SpriteRenderer aSpr) {
		var go = new GameObject(aSpr.name, typeof(SpriteRenderer));
		var spr = go.GetComponent<SpriteRenderer>();
		spr.sprite = aSpr.sprite;
		spr.material = aSpr.material;
		spr.color = aSpr.color;
		var result = go.GetComponent<Transform>();		
		var transform = aSpr.GetComponent<Transform>();
		result.localPosition = transform.position;
		result.localRotation = transform.rotation;
		result.localScale = transform.lossyScale;
		return result;
		
	}
	
	public static void DeepSetLayer(this Transform transform, int layer) {
		transform.gameObject.layer = layer;
		for(int i=0; i<transform.childCount; ++i) {
			transform.GetChild(i).DeepSetLayer(layer);
		}
	}
	
}
