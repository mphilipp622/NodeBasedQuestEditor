using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

using Object = UnityEngine.Object;

/// <summary>
/// This is the node editor for the quest system. Inherits Node_Editor
/// </summary>
public class NodeEditorQuest : Node_Editor
{

	/// <summary>
	/// Returns a casted version of nodeCanvas that is specific for this editor.
	/// </summary>
	public override dynamic nodeCanvas
	{
		get
		{
			return (QuestNodeCanvas) _nodeCanvas;
		}

		set
		{
			_nodeCanvas = (QuestNodeCanvas) value;
		}
	}

	/// <summary> Used for the side window scrollview. </summary>
	private Vector2 scrollPosition;

	private bool addObjective, removeObjective; // used for add/remove objective buttons.
	private bool addPrerequisiteQuest, removePrerequisiteQuest, addObjectivePath, deleteObjectivePath;

	int objectivePathIndex; // used for GUILayout.Popup when choosing objective paths to edit.

	List<string> objectivePathNames;

	/// <summary>
	/// Instantiates the editor window inside the unity editor.
	/// </summary>
	[MenuItem("Quest/Quest Editor")]
	static void CreateEditor()
	{
		NodeEditorQuest.editor = EditorWindow.GetWindow<NodeEditorQuest>();
		NodeEditorQuest.editor.minSize = new Vector2(800, 600);
	}

	/// <summary>
	/// Draws the side window.
	/// </summary>
	public override void DrawSideWindow()
	{
		DrawSaveLoadSettings();

		GUILayout.Space(15);

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		GUILayout.Space(15);

		DisplayQuestDataUI();

		GUILayout.Space(15);

		DisplayObjectiveUI();

		GUILayout.Space(15);

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		DisplayQuestPrereqUI();

		GUILayout.Space(15);

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		DisplayRequiredGameStates();
	}

	/// <summary>
	/// Initializes side window dimensions and calls DrawSideWindow()
	/// </summary>
	protected override void SetupSideWindow()
	{
		sideWindowWidth = Math.Min(600, Math.Max(365, (int)(position.width / 5)));
		GUILayout.BeginArea(sideWindowRect, nodeBox);
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, nodeBox);
		DrawSideWindow();
		GUILayout.EndScrollView();
		GUILayout.EndArea();

		EditorUtility.SetDirty(nodeCanvas);
	}

	/// <summary>
	/// Displays quest objective information on the side window.
	/// </summary>
	private void DisplayObjectiveUI()
	{
		// Draw buttons for adding and removing objective paths.
		addObjectivePath = GUILayout.Button(new GUIContent("Add New Objective Path", "Click this button to create a new concurrent objective path"));
		deleteObjectivePath = GUILayout.Button(new GUIContent("Remove Objective Path", "Removes the currently selected objective ppath"));

		if (addObjectivePath)
			AddNewObjectivePath();
		if (deleteObjectivePath)
			RemoveObjectivePath();

		GUILayout.Space(20);

		// Draw each objective path information
		if (nodeCanvas.objectivePaths.Count > 0)
		{
			objectivePathIndex = EditorGUILayout.Popup(objectivePathIndex, objectivePathNames.ToArray());

			GUILayout.Space(10);

			GUILayout.Label("Objective Path Name");

			nodeCanvas.objectivePaths[objectivePathIndex].objectivePathName = GUILayout.TextField(nodeCanvas.objectivePaths[objectivePathIndex].objectivePathName);

			GUILayout.Space(10);

			UpdateObjectivePathNames();

			addObjective = GUILayout.Button("Add Objective");

			if (addObjective)
				AddEmptyDataToObjectivePath();

			GUILayout.Space(10);

			EditorGUIUtility.labelWidth = 215;

			// draw each path's sub objectives.
			for (int i = 0; i < nodeCanvas.objectivePaths[objectivePathIndex].objectives.Count; i++)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label("Sub-Objective " + i.ToString() + ": ");
				GUILayout.FlexibleSpace();

				nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].moveUp = GUILayout.Button(upButton, GUILayout.Width(30), GUILayout.Height(15));

				nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].moveDown = GUILayout.Button(downButton, GUILayout.Width(30), GUILayout.Height(15));

				nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].delete = GUILayout.Button(deleteButton, GUILayout.Width(30), GUILayout.Height(15));

				GUILayout.EndHorizontal();

				GUILayout.Space(10);

				nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].numberToCollect =
					EditorGUILayout.IntField(new GUIContent("\tNumber To Collect/Kill"),
						nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].numberToCollect);

				nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].objective = (GameObject)EditorGUILayout.ObjectField(new GUIContent("\tObject", "The prefab of the game object that we want to be the objective. For instance, an enemy or an item, etc."), nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].objective, typeof(GameObject), false);

				GUILayout.Space(5);

				if (nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].moveUp)
					MoveSubObjectiveUp(i);
				else if (nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].moveDown)
					MoveSubObjectiveDown(i);
				else if (nodeCanvas.objectivePaths[objectivePathIndex].objectives[i].delete)
					DeleteSubObjective(i);

				GUILayout.Space(10);
			}
		}
	}

	/// <summary>
	/// Draws quest data to the side window. Quest name, quest giver.
	/// </summary>
	private void DisplayQuestDataUI()
	{
		GUILayout.Label("Quest Name");
		nodeCanvas.questName = GUILayout.TextField(nodeCanvas.questName);

		GUILayout.Space(15);
		nodeCanvas.questGiver = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Quest Giver", "This is the character that gives the quest to the player"), nodeCanvas.questGiver, typeof(GameObject), false);
	}

	/// <summary>
	/// Draws quest prerequisites to the side window.
	/// </summary>
	private void DisplayQuestPrereqUI()
	{
		EditorGUIUtility.labelWidth = 175;

		GUILayout.Space(20);

		// Add/remove buttons
		addPrerequisiteQuest = GUILayout.Button("Add Prerequisite Quest");
		removePrerequisiteQuest = GUILayout.Button("Remove Prerequisite Quest");

		if (addPrerequisiteQuest)
			AddEmptyDataToLists(nodeCanvas.requiredQuests);
		if (removePrerequisiteQuest)
			PopItemsOffList(nodeCanvas.requiredQuests);

		EditorGUIUtility.labelWidth = 100;

		GUILayout.Space(15);

		// Display each required quest.
		for (int i = 0; i < nodeCanvas.requiredQuests.Count; i++)
		{
			nodeCanvas.requiredQuests[i] = (QuestAsset)EditorGUILayout.ObjectField(new GUIContent("Prerequisite " + i.ToString()), nodeCanvas.requiredQuests[i], typeof(QuestAsset), false);
			GUILayout.Space(5);
		}
	}

	/// <summary>
	/// Creates a new objective path asset.
	/// </summary>
	private void AddNewObjectivePath()
	{
		ObjectivePath newPath = ScriptableObject.CreateInstance<ObjectivePath>();
		newPath.Initialize("ObjectivePath " + (nodeCanvas.objectivePaths.Count + 1));
		nodeCanvas.objectivePaths.Add(newPath);
		objectivePathNames.Add(nodeCanvas.objectivePaths[nodeCanvas.objectivePaths.Count - 1].objectivePathName);
		objectivePathIndex = nodeCanvas.objectivePaths.Count - 1;
	}

	/// <summary>
	/// Updates the objective path names if they have changed.
	/// </summary>
	private void UpdateObjectivePathNames()
	{
		// Make sure objective path names have same number of elements as objective paths.
		if (objectivePathNames.Count != nodeCanvas.objectivePaths.Count)
		{
			int difference = Mathf.Abs(nodeCanvas.objectivePaths.Count - objectivePathNames.Count);

			for (int i = 0; i < difference; i++)
				objectivePathNames.Add(null);
		}

		for (int i = 0; i < nodeCanvas.objectivePaths.Count; i++)
			objectivePathNames[i] = nodeCanvas.objectivePaths[i].objectivePathName;
	}

	/// <summary>
	/// Removes an objective path from the asset and side window.
	/// </summary>
	private void RemoveObjectivePath()
	{
		nodeCanvas.objectivePaths.RemoveAt(objectivePathIndex);
		objectivePathNames.RemoveAt(objectivePathIndex);
		objectivePathIndex = 0;
	}

	/// <summary>
	/// Moves a sub objective up in the list of sub objectives.
	/// </summary>
	/// <param name="index">The index of the sub objective to be moved.</param>
	private void MoveSubObjectiveUp(int index)
	{
		// if we're at top of list, don't do anything.
		if (index == 0)
			return;

		SwapSubObjective(index, index - 1);
	}

	/// <summary>
	/// Moves a sub objective down in the list of sub objectives.
	/// </summary>
	/// <param name="index">The index of the sub objective to be moved.</param>
	private void MoveSubObjectiveDown(int index)
	{
		// if we're at bottom of list, don't do anything.
		if (index == nodeCanvas.objectivePaths[objectivePathIndex].objectives.Count - 1)
			return;

		SwapSubObjective(index, index + 1);
	}

	/// <summary>
	/// Removes a sub objective from the objective path.
	/// </summary>
	/// <param name="index">The index of the sub objective to remove.</param>
	private void DeleteSubObjective(int index)
	{
		nodeCanvas.objectivePaths[objectivePathIndex].objectives.RemoveAt(index);
	}

	/// <summary>
	/// Helper function for swapping sub objectives. Used when moving objectives up and down.
	/// </summary>
	/// <param name="index">First index</param>
	/// <param name="newIndex">Index to swap first index to. </param>
	private void SwapSubObjective(int index, int newIndex)
	{
		SubObjective temp = nodeCanvas.objectivePaths[objectivePathIndex].objectives[index];
		nodeCanvas.objectivePaths[objectivePathIndex].objectives[index] = nodeCanvas.objectivePaths[objectivePathIndex].objectives[newIndex];
		nodeCanvas.objectivePaths[objectivePathIndex].objectives[newIndex] = temp;
	}

	/// <summary>
	/// Generic function for adding empty data to a list.
	/// </summary>
	/// <typeparam name="T">The type of list</typeparam>
	/// <param name="myList">The list to add data to.</param>
	private void AddEmptyDataToLists<T>(List<T> myList) where T : class
	{
		myList.Add(null);
	}

	/// <summary>
	/// Adds empty data to an integer list.
	/// </summary>
	/// <param name="myList">The List of integers to add data to.</param>
	private void AddEmptyDataToLists(List<int> myList)
	{
		myList.Add(0);
	}

	/// <summary>
	/// Adds empty data to an objective path. Used for creating new sub objectives.
	/// </summary>
	private void AddEmptyDataToObjectivePath()
	{
		SubObjective tempSubObjective = ScriptableObject.CreateInstance<SubObjective>();
		tempSubObjective.Initialize();
		nodeCanvas.objectivePaths[objectivePathIndex].objectives.Add(tempSubObjective);
	}

	/// <summary>
	/// Generic function for removing an item from the end of a list.
	/// </summary>
	/// <typeparam name="T">The type of the list.</typeparam>
	/// <param name="myList">The list to remove the item from.</param>
	private void PopItemsOffList<T>(List<T> myList) where T : class
	{
		if (myList.Count == 0)
			return;

		myList.Remove(myList[myList.Count - 1]);
	}

	/// <summary>
	/// Removes an integer from the end of an integer list.
	/// </summary>
	/// <param name="myList">The integer list to remove data from. </param>
	private void PopItemsOffList(List<int> myList)
	{
		if (myList.Count == 0)
			return;

		myList.Remove(myList[myList.Count - 1]);
	}

	/// <summary>
	/// Checks if quest giver game object has a QuestGiver component or not. If it doesn't, it automatically adds one to the prefab.
	/// </summary>
	private void UpdateQuestGiverComponent()
	{
		GameObject newQuestGiver = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Characters/" + nodeCanvas.questGiver.name + ".prefab");

		if (newQuestGiver.GetComponent<QuestGiver>() != null)
			return;

		newQuestGiver.AddComponent<QuestGiver>();
	}

	/// <summary>
	/// Saves the current node canvas as a new asset
	/// </summary>
	public override void SaveNodeCanvas(string path)
	{
		SaveQuest("Assets/Resources/Quests/");

		if (String.IsNullOrEmpty(path))
			return;

		string existingPath = AssetDatabase.GetAssetPath(nodeCanvas);

		if (!String.IsNullOrEmpty(existingPath))
		{
			if (existingPath != path)
			{
				AssetDatabase.CopyAsset(existingPath, path);
				LoadNodeCanvas(path);
			}

			return;
		}

		AssetDatabase.CreateAsset(nodeCanvas, path);

		for (int nodeCnt = 0; nodeCnt < nodeCanvas.nodes.Count; nodeCnt++)
		{ 
			// Add every node and every of it's inputs/outputs into the file. 
		  // Results in a big mess but there's no other way
			Node node = nodeCanvas.nodes[nodeCnt];
			AssetDatabase.AddObjectToAsset(node, nodeCanvas);

			for (int inCnt = 0; inCnt < node.Inputs.Count; inCnt++)
				AssetDatabase.AddObjectToAsset(node.Inputs[inCnt], node);

			for (int outCnt = 0; outCnt < node.Outputs.Count; outCnt++)
				AssetDatabase.AddObjectToAsset(node.Outputs[outCnt], node);
		}

		foreach (ObjectivePath objectivePath in nodeCanvas.objectivePaths)
		{
			AssetDatabase.AddObjectToAsset(objectivePath, nodeCanvas);

			foreach (SubObjective subObjective in objectivePath.objectives)
			{
				AssetDatabase.AddObjectToAsset(subObjective, objectivePath);
			}
		}

		string[] folders = path.Split(new char[] { '/' }, StringSplitOptions.None);
		openedCanvas = folders[folders.Length - 1];
		openedCanvasPath = path;

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Repaint();
	}

	/// <summary>
	/// Updates the QuestAsset data and saves it to the Asset Database.
	/// </summary>
	/// <param name="path">The file path to save the quest asset to.</param>
	private void SaveQuest(string path)
	{
		List<QuestObjective> newObjectives = new List<QuestObjective>();
		Dictionary<string, QuestAsset> newRequiredQuests = new Dictionary<string, QuestAsset>();

		// Parse objectives
		for (int i = 0; i < nodeCanvas.questObjectives.Count; i++)
		{
			if (nodeCanvas.questObjectives[i].gameObject.tag == "Enemy")
				newObjectives.Add(new EnemyObjective(nodeCanvas.questObjectives[i].GetComponent<Enemy>(), nodeCanvas.numberOfObjectivesToCollect[i], nodeCanvas.newQuest.questGiver));
			else if (nodeCanvas.questObjectives[i].gameObject.tag == "Location")
				newObjectives.Add(new LocationObjective(nodeCanvas.questObjectives[i]));
		}

		// Parse required quests
		for (int i = 0; i < nodeCanvas.requiredQuests.Count; i++)
			newRequiredQuests.Add(nodeCanvas.requiredQuests[i].questName, nodeCanvas.requiredQuests[i]);

		UpdateQuestGiverComponent();

		nodeCanvas.newQuest.InitializeData(nodeCanvas.questName, nodeCanvas.questGiver, newObjectives, newRequiredQuests, null);

		string existingPath = AssetDatabase.GetAssetPath(nodeCanvas.newQuest);

		if (!String.IsNullOrEmpty(existingPath))
		{
			if (existingPath != path + nodeCanvas.newQuest.questName + ".asset")
				AssetDatabase.CopyAsset(existingPath, path + nodeCanvas.newQuest.questName + ".asset");

			nodeCanvas.newQuest = AssetDatabase.LoadAssetAtPath<QuestAsset>(path + nodeCanvas.newQuest.questName + ".asset");
			return;
		}

		AssetDatabase.CreateAsset(nodeCanvas.newQuest, path + nodeCanvas.newQuest.questName + ".asset");
	}

	/// <summary>
	/// Loads the node canvas.
	/// </summary>
	/// <param name="path">The path to load from.</param>
	public override void LoadNodeCanvas(string path)
	{
		numberOfStates = 0;

		if (String.IsNullOrEmpty(path))
			return;

		Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

		if (objects.Length == 0)
			return;

		QuestNodeCanvas newNodeCanvas = null;

		for (int cnt = 0; cnt < objects.Length; cnt++)
		{ 
			// We only have to search for the Node Canvas itself in the mess, because it still hold references to all of it's nodes and their connections
			object obj = objects[cnt];

			if (obj.GetType() == typeof(QuestNodeCanvas))
				newNodeCanvas = obj as QuestNodeCanvas;
		}

		if (newNodeCanvas == null)
			return;

		nodeCanvas = newNodeCanvas;

		// Set number of states
		foreach (Node node in nodeCanvas.nodes)
		{
			// count the current number of dialogue states
			if (node.state != -2) // ignore choice nodes.
				numberOfStates++;
		}

		string[] folders = path.Split(new char[] { '/' }, StringSplitOptions.None);
		openedCanvas = folders[folders.Length - 1];
		openedCanvasPath = path;

		InitGameStateData();

		Repaint();
		AssetDatabase.Refresh();
	}

	/// <summary>
	/// Adds items to the right-click context menu. Modify this function if you wish to add new right-click items.
	/// </summary>
	protected override void AddRightClickMenuItems(GenericMenu menu)
	{
		menu.AddItem(new GUIContent("Add Quest Dialogue"), false, ContextCallback, "questDialogueNode");
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Add Dialogue Choice"), false, ContextCallback, "dialogueChoice");
	}

	/// <summary>
	/// Handles functionality for right-click context items. Modify this function if you wish to add new 
	/// right-click functionality.
	/// </summary>
	public override void ContextCallback(object obj)
	{
		switch (obj.ToString())
		{
			case "questDialogueNode":
				numberOfStates++;
				QuestDialogueNode.Create(new Rect(mousePos.x, mousePos.y, 350, 350), numberOfStates);
				break;

			case "dialogueChoice":
				numberOfChoiceStates++;
				QuestChoiceNode.Create(new Rect(mousePos.x, mousePos.y, 100, 50), numberOfChoiceStates);
				break;

			case "deleteNode":
				Node node = NodeAtPosition(mousePos);
				if (node != null)
				{
					if (node.GetType() == typeof(QuestDialogueNode))
						numberOfStates--;
					else if (node.GetType() == typeof(DialogueChoiceNode))
						numberOfChoiceStates--;

					// Find all nodes with higher state values than the deleted node and subtract 1 from their state.
					foreach (Node successorNode in nodeCanvas.nodes)
					{
						// Ignore choice states
						if (successorNode.state != -2 && successorNode.state > node.state)
						{
							successorNode.state--;
							successorNode.name = "State " + successorNode.state;
						}

					}

					_nodeCanvas.nodes.Remove(node);

					if (node is DialogueChoiceNode)
					{
						_nodeCanvas.choiceNodes.Remove(node);
						_nodeCanvas.choiceNodeNames.Remove(node.name);
					}

					// Iterate over all the other nodes and update start/final states
					foreach (Node editorNode in Node_Editor.editor.nodeCanvas.nodes)
						Node_Editor.editor.RecalculateFrom(editorNode);

					node.OnDelete();
				}

				break;
		}
	}

	/// <summary>
	/// Creates a new node canvas and initializes data for the canvas and the editor.
	/// </summary>
	public override void NewNodeCanvas()
	{
		_nodeCanvas = ScriptableObject.CreateInstance<QuestNodeCanvas>();
		nodeCanvas.newQuest = ScriptableObject.CreateInstance<QuestAsset>();
		_nodeCanvas.nodes = new List<Node>();
		_nodeCanvas.choiceNodes = new List<Node>();
		_nodeCanvas.choiceNodeNames = new List<string>();
		nodeCanvas.questObjectives = new List<GameObject>();
		nodeCanvas.requiredQuests = new List<QuestAsset>();
		nodeCanvas.numberOfObjectivesToCollect = new List<int>();
		nodeCanvas.objectivePaths = new List<ObjectivePath>();
		nodeCanvas.requiredGameStates = new List<GameStateName>();
		openedCanvas = "New Canvas";
		openedCanvasPath = "";

		numberOfStates = 0;
		addObjective = false;
		removeObjective = false;
		addPrerequisiteQuest = false;
		removePrerequisiteQuest = false;
		addObjectivePath = false;
		deleteObjectivePath = false;
		objectivePathNames = new List<string>();
		objectivePathIndex = 0;
		defaultPath = "Assets/Resources/QuestCanvas/";
		deleteButton = Resources.Load("Sprites/EditorUI/DeleteButton") as Texture;
		upButton = Resources.Load("Sprites/EditorUI/UpArrow") as Texture;
		downButton = Resources.Load("Sprites/EditorUI/DownArrow") as Texture;

		InitGameStateData();
	}
}
