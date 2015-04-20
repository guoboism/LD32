using LittlePolygon;
using System.Collections;
using UnityEngine;

public class Title : CustomBehaviour {

	public Transform cone;
	public Transform leftKey;
	public Transform rightKey;

	IEnumerator Start() {
		
		yield return null;
		yield return null;
		
		while(!Input.anyKeyDown) {
			yield return null;
		}
		
		Jukebox.Play("title");
		
		foreach(var t in GetComponentsInChildren<TextMesh>()) {
			Destroy(t.gameObject);
		}
		
		var p0 = transform.localPosition;
		foreach(var t in Transition(0.25f*8f/3f)) {
			transform.localPosition = p0.Above(EaseOut4(t));
			yield return null;
		}
		
		Destroy(gameObject);
		
	}
	
	void Update() {
		cone.localRotation = Quaternion.AngleAxis(Mathf.Sin (Mathf.PI * Time.time * 2) * 5.0f, Vector3.forward);
		
		var bps = 8f/3f;
		var s = Mathf.Max (Mathf.Sin (bps * Mathf.PI * Time.time), 0f);
		var c = Mathf.Max (-Mathf.Sin (bps * Mathf.PI * Time.time), 0f);
		
		leftKey.localPosition = Vec(0, 0.1f * s, 0);
		rightKey.localPosition = Vec(0, 0.1f * c, 0);
	}

}
