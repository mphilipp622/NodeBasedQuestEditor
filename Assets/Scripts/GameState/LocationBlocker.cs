using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationBlocker : GameStateChecker
{
	void Start ()
	{
		Initialize();
	}
	
	void Update ()
	{
		
	}

	protected override void ExecuteProcedure()
	{
		Debug.Log("Destroying Collider");
		Destroy(gameObject);
	}
}
