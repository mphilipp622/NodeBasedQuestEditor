using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameLizard : Enemy {
	
	void Start ()
	{
		
	}
	
	void Update ()
	{
		// This is for testing purposes. When you press J, a flame lizard will die. Requires Flame Lizard prefab to exist in scene. EnemyObjective class listens for this.
		if (Input.GetKeyDown(KeyCode.J))
			Messenger.Broadcast<Enemy>("Enemy Killed", this);
	}
}
