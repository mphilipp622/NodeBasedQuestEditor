using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enables an NPC when a set of game states is true.
/// </summary>
public class NPCLoader : GameStateChecker
{
	
	void Start ()
	{
		//Initialize();
	}
	
	void Update ()
	{
		
	}

	protected override void Initialize()
	{

		// Disable all components at start.
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild(i).gameObject.SetActive(false);

		foreach (Behaviour comp in gameObject.GetComponents(typeof(Behaviour)))
		{
			if (comp.GetType() == typeof(NPCLoader))
				continue;

			comp.enabled = false;
		}

		foreach (Renderer rend in gameObject.GetComponents(typeof(Renderer)))
			rend.enabled = false;
	}

	protected override void ExecuteProcedure()
	{
		// Enable all components when conditions are met.
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild(i).gameObject.SetActive(true);

		foreach (Behaviour comp in gameObject.GetComponents(typeof(Behaviour)))
			comp.enabled = true;

		foreach (Renderer rend in gameObject.GetComponents(typeof(Renderer)))
			rend.enabled = true;
	}

	protected override void RevertExecution()
	{
		// Disable all components when reverting.
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild(i).gameObject.SetActive(false);

		foreach (Behaviour comp in gameObject.GetComponents(typeof(Behaviour)))
			comp.enabled = false;

		foreach (Renderer rend in gameObject.GetComponents(typeof(Renderer)))
			rend.enabled = false;
	}
}
