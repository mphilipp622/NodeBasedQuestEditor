using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
	private Dictionary<string, bool> gameStates;

	public static GameState gameState;


	private void OnDisable()
	{
		DisableListeners();
	}

	private void Awake()
	{
		InitSingleton();
		LoadStateData();
		InitListeners();
	}

	void Start ()
	{
		
	}
	
	void Update ()
	{
		// these are for testing purposes
		if (Input.GetKeyDown(KeyCode.V))
			Messenger.Broadcast<string>("Game State Update", "VOLCANO_ERUPTED");
	}

	private void InitSingleton()
	{
		if (gameState == null)
			gameState = this;
		else if (gameState != this)
			Destroy(gameObject);
	}

	/// <summary>
	/// Initializes messenger listening for each possible state in the game.
	/// </summary>
	private void InitListeners()
	{
		//foreach (KeyValuePair<string, bool> state in gameStates)
		Messenger.AddListener<string>("Game State Update", UnlockState);
	}

	/// <summary>
	/// Disables messenger listening for each possible state in the game.
	/// </summary>
	private void DisableListeners()
	{
		//foreach (KeyValuePair<string, bool> state in gameStates)
		Messenger.RemoveListener<string>("Game State Update", UnlockState);
	}


	/// <summary>
	/// Loads and parses the data from the GameStatData Scriptable Object Asset.
	/// </summary>
	private void LoadStateData()
	{
		GameStateAsset gameStateAsset = Resources.Load<GameStateAsset>("GameState/GameStateData");

		gameStates = new Dictionary<string, bool>();

		// Initialize hash table with all game states set to false.
		foreach (GameStateName state in gameStateAsset.gameStates)
			gameStates.Add(state.eventName, false);
	}

	/// <summary>
	/// Returns true if stateName has been unlocked.
	/// </summary>
	/// <param name="stateName">Name of the state to check.</param>
	public bool IsStateUnlocked(string stateName)
	{
		return gameStates[stateName];
	}

	/// <summary>
	/// Returns true if all the required states are unlocked.
	/// </summary>
	public bool AreAllStatesUnlocked(List<string> requiredStates)
	{
		foreach(string stateName in requiredStates)
		{
			if (!gameStates[stateName])
				return false;
		}

		return true;
	}


	/// <summary>
	/// Unlocks a game state.
	/// </summary>
	/// <param name="stateName">Game state to unlock.</param>
	public void UnlockState(string stateName)
	{
		Debug.Log("Unlocking State: " + stateName);
		gameStates[stateName] = true;
	}
}
