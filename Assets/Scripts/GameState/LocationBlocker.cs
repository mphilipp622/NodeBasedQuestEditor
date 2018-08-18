using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationBlocker : GameStateChecker
{

	private Collider2D collider;

	void Start ()
	{
		Initialize();
	}
	
	void Update ()
	{
		
	}

	protected override void Initialize()
	{
		collider = GetComponent<Collider2D>();
	}

	protected override void ExecuteProcedure()
	{
		Debug.Log("Disabling Collider");
		collider.enabled = false;
		//Destroy(gameObject);
	}

	protected override void RevertExecution()
	{
		Debug.Log("Enabling Collider");
		collider.enabled = true;
	}
}
