using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
	[SerializeField]
	private string gameStateAssetDirectory;

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
			Messenger.Broadcast<string>("Unlock Game State", "VOLCANO_ERUPTED");
		if (Input.GetKeyDown(KeyCode.B))
			Messenger.Broadcast<string>("Lock Game State", "VOLCANO_ERUPTED");
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
		Messenger.AddListener<string>("Unlock Game State", UnlockState);
		Messenger.AddListener<string>("Lock Game State", LockState);
	}

	/// <summary>
	/// Disables messenger listening for each possible state in the game.
	/// </summary>
	private void DisableListeners()
	{
		Messenger.RemoveListener<string>("Unlock Game State", UnlockState);
		Messenger.RemoveListener<string>("Lock Game State", LockState);
	}


	/// <summary>
	/// Loads and parses the data from the GameStatData Scriptable Object Asset.
	/// </summary>
	private void LoadStateData()
	{
		GameStateAsset gameStateAsset = Resources.Load<GameStateAsset>(gameStateAssetDirectory);

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
	/// Returns true if all the required states are unlocked. Primarily accessed by GameStateChecker.cs
	/// </summary>
	/// <param name="requiredStates">The list of states you wish to check</param>
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
	private void UnlockState(string stateName)
	{
		if (gameStates[stateName])
			return; // if state is already unlocked, exit function.

		Debug.Log("Unlocking State: " + stateName);

		gameStates[stateName] = true;

		// Use a try-catch block for broadcasting this message. It's possible no GameStateCheckers exist in the scene.
		try
		{
			Messenger.Broadcast<string>("Game State Update", stateName); // notify GameStateChecker objects of the update
		}
		catch(Messenger.BroadcastException)
		{
		}
	}

	/// <summary>
	/// Locks a game state and notifies listeners of this change.
	/// </summary>
	private void LockState(string stateName)
	{
		if (!gameStates[stateName])
			return; // if state is already locked, exit function.

		Debug.Log("Locking State: " + stateName);

		gameStates[stateName] = false;

		// Use a try-catch block for broadcasting this message. It's possible no GameStateCheckers exist in the scene.
		try
		{
			Messenger.Broadcast<string>("Game State Update", stateName); // notify GameStateChecker objects of the update
		}
		catch (Messenger.BroadcastException)
		{
		}
	}
}
