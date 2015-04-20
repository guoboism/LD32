#if UNITY_EDITOR
#define HIDE_IN_HEIRARCHY
#endif

using System;
using System.Collections;
using UnityEngine;

namespace LittlePolygon {
	
	// CRTP-Style Generic Base Class, e.g. class Foo : PooledObject<Foo>
	public class PooledObject : CustomBehaviour {
		
		// Pooled instances for a linked-list of freenodes from their prefab
		PooledObject prefab, next;
		
		public PooledObject PoolPrefab { get { return prefab; } }
		public bool IsPooledInstance { get { return prefab != null; } }
		
		protected PooledObject Alloc(Vector3 position)  {
			
			#if UNITY_EDITOR
			// INTENTDED TO BE CALLED ON PREFABS
			Assert(prefab == null);
			#endif
			
			if (next != null) {
				
				// RECYCLE INSTANCE
				var result = next;
				next = result.next;
				result.next = null;
				result.transform.position = position;
				result.transform.rotation = Quaternion.identity;
				result.gameObject.SetActive(true);
				return result;
				
			} else {
				
				// CREATE NEW INSTANCE
				var result = Dup(this, position);
				result.prefab = this;
				#if HIDE_IN_HEIRARCHY
				result.gameObject.hideFlags = HideFlags.HideInHierarchy;
				#endif
				return result;
	
			}
		}
		
		public void Release()  {
			if (prefab != null) {
				
				// DEACTIVATE AND PREPEND TO PREFAB'S FREELIST
				transform.parent = null;
				gameObject.SetActive(false);
				next = prefab.next;
				prefab.next = this;
				
			} else if (gameObject) {
				
				// THIS INSTANCE ACTIVE BUT NOT POOLED
				Destroy(gameObject);
				
			}
			
		}
		
		
	}

}
