using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpritePair {
	public Sprite normal;
	public Sprite dead;
}

public class ShrapnelRemapper : MonoBehaviour {
	
	public SpritePair[] pairs;
	internal Dictionary<Sprite,Sprite> map = new Dictionary<Sprite,Sprite>();
	static ShrapnelRemapper inst;
	
	void Awake() {
		inst = this;
		foreach(var p in pairs) {
			map[p.normal] = p.dead;
		}
	}
	
	void OnDestroy() {
		if (inst == this) { inst = null; }
	}
	
	public static Sprite Get(Sprite normal) {
		Sprite result = null;
		if (inst != null) inst.map.TryGetValue(normal, out result);	
		return result;
	}
}
