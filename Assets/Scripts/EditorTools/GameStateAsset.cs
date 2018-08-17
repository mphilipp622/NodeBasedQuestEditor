using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateAsset : ScriptableObject
{
	public List<GameStateName> gameStates;

	public void Initialize()
	{
		gameStates = new List<GameStateName>();
	}
}
