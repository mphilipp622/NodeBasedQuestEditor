using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class GameStateChecker : MonoBehaviour
{
	[SerializeField]
	private List<string> requiredStates;

	protected abstract void ExecuteProcedure();

	protected virtual void Initialize()
	{
		//requiredStates = new Dictionary<string, bool>();
		
		//for(int i = 0; i < stateNames.Count; i++)
		//{
		//	if(stateValues[i])
		//		requiredStates.Add(stateNames[i], stateValues[i]);
		//}

		InitListeners();
	}

	/// <summary>
	/// Initializes messenger listening for each possible state in the game.
	/// </summary>
	protected void InitListeners()
	{
		//foreach (KeyValuePair<string, bool> state in gameStates)
		Messenger.AddListener<string>("Game State Update", UnlockState);
	}

	/// <summary>
	/// Disables messenger listening for each possible state in the game.
	/// </summary>
	protected void DisableListeners()
	{
		//foreach (KeyValuePair<string, bool> state in gameStates)
		Messenger.RemoveListener<string>("Game State Update", UnlockState);
	}

	protected void UnlockState(string stateName)
	{
		if (GameState.gameState.AreAllStatesUnlocked(requiredStates))
			ExecuteProcedure();
	}

	public List<string> GetRequiredStates()
	{
		if (requiredStates == null)
			requiredStates = new List<string>();

		return requiredStates;
	}

	public void AddRequiredState(string stateName)
	{
		if (requiredStates == null)
			requiredStates = new List<string>();

		if (requiredStates.Contains(stateName))
			return;

		requiredStates.Add(stateName);
	}

	public void RemoveRequiredState(string stateName)
	{
		if (requiredStates == null)
			requiredStates = new List<string>();

		if (!requiredStates.Contains(stateName))
			return;

		requiredStates.Remove(stateName);
	}
}
