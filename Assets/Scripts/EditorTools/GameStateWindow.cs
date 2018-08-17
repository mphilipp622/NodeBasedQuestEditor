using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class GameStateWindow : EditorWindow
{
	private bool addNewState, deleteState, saveData;
	private static GameStateAsset gameStateAsset;

	private Vector2 scrollPosition;

	[MenuItem("Game State/Game State Editor")]
	static void Init()
	{
		EditorWindow.GetWindow(typeof(GameStateWindow));

		Selection.activeGameObject = null;
		LoadGameStateAsset();
	}

	public void OnGUI()
	{
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		DisplayAddStateButton();

		GUILayout.Space(5);

		DisplaySaveButton();

		GUILayout.Space(15);

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		DisplayGameStates();

		GUILayout.EndScrollView();

		EditorUtility.SetDirty(gameStateAsset);
	}

	private void OnDisable()
	{
		SaveData();
	}

	private static void LoadGameStateAsset()
	{
		gameStateAsset = AssetDatabase.LoadAssetAtPath<GameStateAsset>("Assets/Resources/GameState/GameStateData.asset");

		if (!gameStateAsset)
			CreateGameStateAsset();
	}

	private static void CreateGameStateAsset()
	{
		gameStateAsset = ScriptableObject.CreateInstance<GameStateAsset>();
		gameStateAsset.Initialize();

		AssetDatabase.CreateAsset(gameStateAsset, "Assets/Resources/GameState/GameStateData.asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private void DisplaySaveButton()
	{
		saveData = GUILayout.Button("Save Data");

		if (saveData)
			SaveData();
	}

	private void SaveData()
	{
		foreach (GameStateName state in gameStateAsset.gameStates)
		{
			if (!AssetDatabase.Contains(state))
				AssetDatabase.CreateAsset(state, "Assets/Resources/GameState/States/" + state.eventName + ".asset");

			//AssetDatabase.AddObjectToAsset(temp, gameStateAsset);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private void DisplayAddStateButton()
	{
		GUILayout.Space(10);

		addNewState = GUILayout.Button(new GUIContent("Add New State", "Adds a new Game State to the list"));

		if (addNewState)
			AddNewState();
	}

	private void DisplayGameStates()
	{
		for (int i = 0; i < gameStateAsset.gameStates.Count; i++)
		{
			GUILayout.BeginHorizontal();

			gameStateAsset.gameStates[i].eventName = EditorGUILayout.TextField(gameStateAsset.gameStates[i].eventName);

			deleteState = GUILayout.Button(Resources.Load("Sprites/EditorUI/DeleteButton") as Texture, GUILayout.Width(30), GUILayout.Height(15));

			if (deleteState)
				DeleteState(i);

			GUILayout.EndHorizontal();

			GUILayout.Space(5);
		}
	}

	private void AddNewState()
	{
		GameStateName temp = ScriptableObject.CreateInstance<GameStateName>();
		gameStateAsset.gameStates.Add(temp);
	}

	private void DeleteState(int index)
	{
		AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gameStateAsset.gameStates[index]));

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		gameStateAsset.gameStates.RemoveAt(index);
	}
}