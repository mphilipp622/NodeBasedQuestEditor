using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class GameStateChecker : MonoBehaviour
{
	[SerializeField]
	private List<string> requiredStates;

	protected abstract void ExecuteProcedure();

	protected abstract void RevertExecution();

	protected abstract void Initialize();

	protected virtual void OnEnable()
	{
		InitListeners();
	}

	protected virtual void OnDisable()
	{
		DisableListeners();
	}

	/// <summary>
	/// Initializes messenger listening for each possible state in the game.
	/// </summary>
	protected void InitListeners()
	{
		Messenger.AddListener<string>("Game State Update", CheckStatesAndExecute);
	}

	/// <summary>
	/// Disables messenger listening for each possible state in the game.
	/// </summary>
	protected void DisableListeners()
	{
		Messenger.RemoveListener<string>("Game State Update", CheckStatesAndExecute);
	}

	/// <summary>
	/// Checks with GameState.cs to see if all the required states are unlocked. Executes this object's procedure if
	/// all states are unlocked.
	/// </summary>
	/// <param name="stateName"></param>
	protected void CheckStatesAndExecute(string stateName)
	{
		if (GameState.gameState.AreAllStatesUnlocked(requiredStates))
			ExecuteProcedure();
		else
			RevertExecution();
	}

	/// <summary>
	/// Returns a list of required states on this object.
	/// </summary>
	public List<string> GetRequiredStates()
	{
		if (requiredStates == null)
			requiredStates = new List<string>();

		return requiredStates;
	}

	/// <summary>
	/// Adds a state to the list of required states on this object.
	/// </summary>
	public void AddRequiredState(string stateName)
	{
		if (requiredStates == null)
			requiredStates = new List<string>();

		if (requiredStates.Contains(stateName))
			return;

		requiredStates.Add(stateName);
	}

	/// <summary>
	/// Removes a state from the list of required states on this object.
	/// </summary>
	public void RemoveRequiredState(string stateName)
	{
		if (requiredStates == null)
			requiredStates = new List<string>();

		if (!requiredStates.Contains(stateName))
			return;

		requiredStates.Remove(stateName);
	}
}
