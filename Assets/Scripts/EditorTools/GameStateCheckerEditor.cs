using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameStateChecker), true)]
[CanEditMultipleObjects]
public class GameStateCheckerEditor : Editor
{
	private GameStateAsset gameState;

	private bool addState, removeState;

	public List<bool> requiredStateSelections;

	private GameStateChecker checker;

	private Vector2 scrollPosition;

	private void OnEnable()
	{
		checker = (GameStateChecker)target;

		gameState = Resources.Load<GameStateAsset>("GameState/GameStateData");

		requiredStateSelections = new List<bool>();

		// Load serialized data from GameStateChecker into our lists.
		for (int i = 0; i < gameState.gameStates.Count; i++)
		{
			if (checker.GetRequiredStates().Contains(gameState.gameStates[i].eventName))
				requiredStateSelections.Add(true);
			else
				requiredStateSelections.Add(false);
		}
	}

	public override void OnInspectorGUI()
	{
		DisplayGameStates();

		//EditorUtility.SetDirty(checker);
	}

	private void DisplayGameStates()
	{
		GUILayout.Space(5);

		GUILayout.Label("Select Required States");

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		GUILayout.Space(5);

		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));

		EditorGUIUtility.labelWidth = 250;
		for (int i = 0; i < gameState.gameStates.Count; i++)
		{
			GUILayout.BeginHorizontal();

			requiredStateSelections[i] = EditorGUILayout.Toggle(new GUIContent(gameState.gameStates[i].eventName, "Select this box to add this game state to the list of required states"), requiredStateSelections[i]);

			if (requiredStateSelections[i])
				checker.AddRequiredState(gameState.gameStates[i].eventName);
			else
				checker.RemoveRequiredState(gameState.gameStates[i].eventName);

			GUILayout.EndHorizontal();
		}

		EditorGUIUtility.labelWidth = 0;

		GUILayout.EndScrollView();

	}
}
