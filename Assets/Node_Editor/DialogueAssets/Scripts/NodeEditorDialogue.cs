using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

using Object = UnityEngine.Object;

/// <summary>
/// This is the node editor for the dialogue system. Inherits Node_Editor
/// </summary>
public class NodeEditorDialogue : Node_Editor
{
	/// <summary>
	/// Gets and sets the node canvas in this editor using the proper data type.
	/// </summary>
	public override dynamic nodeCanvas
	{
		get
		{
			return (DialogueNodeCanvas) _nodeCanvas;
		}
		set
		{
			_nodeCanvas = (DialogueNodeCanvas) value;
		}
	}

	/// <summary>
	/// Instantiates the node editor window inside the Unity Editor.
	/// </summary>
	[MenuItem("Dialogue Creator/Create Conversation")]
	static void CreateEditor()
	{
		NodeEditorDialogue.editor = EditorWindow.GetWindow<NodeEditorDialogue>();
		NodeEditorDialogue.editor.minSize = new Vector2(800, 600);
	}

	/// <summary>
	/// Draws the side window for this editor.
	/// </summary>
	public override void DrawSideWindow()
	{
		DrawSaveLoadSettings();

		GUILayout.Space(15);

		EditorGUIUtility.labelWidth = 275;

		// Draw field for how many times this conversation can happen.
		nodeCanvas.numberOfTimesConversationCanHappen = EditorGUILayout.IntField(new GUIContent("Number of Times conversation Can Happen", "This is the number of times this conversation will occur in the game. Use -1 for unlimited number of times."), nodeCanvas.numberOfTimesConversationCanHappen);

		GUILayout.Space(15);

		GUILayout.Label(new GUIContent("Probability of this conversation occurring.", "This is the probability of this conversation occurring when all participants are in conversation range. Values are between 0 and 1. E.G: 0.8 would be 80% chance."));

		// Draw the slider for setting the probability of occurrence for this conversation. 0 - 1.0
		nodeCanvas.probabilityOfOccurrence = EditorGUILayout.Slider(nodeCanvas.probabilityOfOccurrence, 0.0f, 1.0f);

		GUILayout.Space(15);

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		DisplayRequiredGameStates();

		EditorUtility.SetDirty(nodeCanvas);
	}

	/// <summary>
	/// Adds right-click menu options to this node editor.
	/// </summary>
	protected override void AddRightClickMenuItems(GenericMenu menu)
	{
		menu.AddItem(new GUIContent("Add Dialogue"), false, ContextCallback, "dialogueNode");
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Add Dialogue Choice"), false, ContextCallback, "dialogueChoice");
	}

	/// <summary>
	/// Defines behavior for selecting right-click context items. Modify this function if you wish to create custom nodes and context menu behaviors.
	/// </summary>
	public override void ContextCallback(object obj)
	{
		switch (obj.ToString())
		{
			case "dialogueNode":
				numberOfStates++;
				DialogueNode.Create(new Rect(mousePos.x, mousePos.y, 350, 350), numberOfStates);
				break;

			case "dialogueChoice":
				numberOfChoiceStates++;
				DialogueChoiceNode.Create(new Rect(mousePos.x, mousePos.y, 100, 50), numberOfChoiceStates);
				break;

			case "deleteNode":
				Node node = NodeAtPosition(mousePos);
				if (node != null)
				{
					if (node.GetType() == typeof(DialogueNode))
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

					nodeCanvas.nodes.Remove(node);

					if (node.GetType() == typeof(DialogueChoiceNode))
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
	/// Creates a new node canvas and initializes canvas and editor data.
	/// </summary>
	public override void NewNodeCanvas()
	{
		nodeCanvas = ScriptableObject.CreateInstance<DialogueNodeCanvas>();
		_nodeCanvas.requiredGameStates = new List<GameStateName>();
		_nodeCanvas.nodes = new List<Node>();
		_nodeCanvas.choiceNodes = new List<Node>();
		_nodeCanvas.choiceNodeNames = new List<string>();
		openedCanvas = "New Canvas";
		numberOfStates = 0;
		openedCanvasPath = "";
		defaultPath = "Assets/Resources/Dialogue/";
		deleteButton = Resources.Load("Sprites/EditorUI/DeleteButton") as Texture;
		InitGameStateData();
	}

	/// <summary>
	/// Loads node canvas.
	/// </summary>
	public override void LoadNodeCanvas(string path)
	{
		numberOfStates = 0;

		if (String.IsNullOrEmpty(path))
			return;

		Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

		if (objects.Length == 0)
			return;

		DialogueNodeCanvas newNodeCanvas = null;

		for (int cnt = 0; cnt < objects.Length; cnt++)
		{ 
			// We only have to search for the Node Canvas itself in the mess, because it still hold references to all of it's nodes and their connections
			object obj = objects[cnt];

			if (obj.GetType() == typeof(DialogueNodeCanvas))
				newNodeCanvas = obj as DialogueNodeCanvas;
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
}
