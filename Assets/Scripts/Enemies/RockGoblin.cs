using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockGoblin : Enemy
{

	void Start ()
	{

	}
	
	void Update ()
	{
		// This is for testing purposes. When you press K, a Rock Goblin will die. Requires rock goblin prefab to exist in scene. 
		if (Input.GetKeyDown(KeyCode.K))
			Messenger.Broadcast<Enemy>("Enemy Killed", this);
	}
}
