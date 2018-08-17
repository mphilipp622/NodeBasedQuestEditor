using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Choice node for dialogue. Takes a DialogueNode input and has up to 9 outputs. Inherits Node.
/// </summary>
[System.Serializable]
public class DialogueChoiceNode : Node
{
	/// <summary>This is the state that lead us to the choice</summary>
	public int originState;

	/// <summary>
	/// Creates a new Dialogue Choice Node.
	/// </summary>
	public static void Create(Rect NodeRect, int choiceStateNumber)
	{
		DialogueChoiceNode node = ScriptableObject.CreateInstance<DialogueChoiceNode>();

		node.name = "Choice State " + choiceStateNumber;
		node.rect = NodeRect;
		node.originState = -1; // I just chose -1. I don't think this matters. Will need to enforce that this state has an input though.
		node.state = -2; // values <= -2 are choice states.
		node.numberOfTransitions = 0;
		node.isStartState = false;
		node.expanded = true;

		// Instantiate 9 outputs for the choice selection
		for (int i = 0; i < 9; i++)
			NodeOutput.Create(node, "Dialogue Option " + (i + 1).ToString(), typeof(int));

		// Create input for previous state
		NodeInput.Create(node, "Previous Dialogue State", typeof(int));

		node.Init();
	}

	/// <summary>
	/// Draws this node in the node editor.
	/// </summary>
	public override void DrawNode()
	{
		// Render the expanded arrow.
		expanded = EditorGUILayout.Foldout(expanded, "Collapse");

		if (!expanded)
		{
			// If the node is not expanded, then we don't want to render all the other fields.
			rect.height = 50; // Collapse the height of the node

			// Paint the inputs and outputs so we can see the flow from state to state even if we are collapsed
			if (Event.current.type == EventType.Repaint)
				Inputs[0].SetRect(GUILayoutUtility.GetLastRect());
			if (Event.current.type == EventType.Repaint)
			{
				for(int i = 0; i < Outputs.Count; i++)
					// iterate over each output and display it.
					Outputs[i].SetRect(GUILayoutUtility.GetLastRect());
			}
				
			if (GUI.changed || Outputs[0].connections.Count != numberOfTransitions)
			{
				numberOfTransitions = Outputs[0].connections.Count;
				Node_Editor.editor.RecalculateFrom(this);
			}

			return; // return so we don't render the rest of the controls.
		}

		// Render the previous state handle
		GUILayout.BeginHorizontal();

		GUILayout.Label("Previous Dialogue State");

		if (Event.current.type == EventType.Repaint)
			Inputs[0].SetRect(GUILayoutUtility.GetLastRect());

		GUILayout.EndHorizontal();

		GUILayout.Space(25);

		// Render the outputs.
		for (int i = 0; i < 9; i++)
		{
			GUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();

			GUILayout.Label("Dialogue Option " + (i + 1).ToString()); // convert the transition number to + 1

			if (Event.current.type == EventType.Repaint)
				Outputs[i].SetRect(GUILayoutUtility.GetLastRect());

			Node_Editor.editor.RecalculateFrom(this);

			GUILayout.EndHorizontal();
		}

		if (GUI.changed)
			Node_Editor.editor.RecalculateFrom(this);

		CalculateStartAndFinalStates();
	}

	/// <summary>
	/// Calculates input/output data for the node.
	/// </summary>
	public override bool Calculate()
	{
		for(int i = 0; i < Outputs.Count; i++)
		{
			if (Outputs[i].connections.Count > 0)
				Outputs[i].connections[0].connection.value = i + 1;
		}
		
		if (Inputs[0].connection != null && Inputs[0].connection.value != null)
			originState = Inputs[0].connection.body.state;

		return true;
	}

	/// <summary>
	/// Always returns true. Must be overridden because base class implementation is abstract.
	/// </summary>
	public override bool IsNodeSafeToSave()
	{
		return true;
	}
}
