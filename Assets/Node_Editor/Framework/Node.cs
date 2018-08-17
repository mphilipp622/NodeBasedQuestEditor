using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

using Object = UnityEngine.Object;

/// <summary>
/// Base class for all Node objects in the editor. Any custom nodes you wish to create must inherit this class.
/// </summary>
public abstract class Node : ScriptableObject
{
	public Rect rect = new Rect();

	/// <summary> The input connections to this node. </summary>
	public List<NodeInput> Inputs = new List<NodeInput>();

	/// <summary> The output connections from this node. </summary>
	public List<NodeOutput> Outputs = new List<NodeOutput>();

	/// <summary> This node's state number. States are integer values. </summary>
	public int state;

	/// <summary> The description of this node. Primarily used for nodes with Choice Node inputs. </summary>
	public string description;

	/// <summary> If this is true, the node is expanded. </summary>
	public bool expanded;

	/// <summary> The number of transitions from this node. </summary>
	protected int numberOfTransitions;

	/// <summary> If true, then this node is a start state. </summary>
	public bool isStartState;
		
	/// <summary> If true, then this node is a final state. </summary>
	public bool isFinalState;

	/// <summary> 
	/// Pointer to a previous choice node. Used for transferring the flow of the FSM back to a 
	/// choice node.
	/// </summary>
	public DialogueChoiceNode previousChoiceNode;

	/// <summary>
	/// Init this node. Has to be called when creating a child node of this
	/// </summary>
	protected void Init ()
	{
		Calculate ();
		Node_Editor.editor.nodeCanvas.nodes.Add (this);

		// Handles choice nodes
		if (this is DialogueChoiceNode)
		{
			// Add the choice node to the node canvas.
			Node_Editor.editor.nodeCanvas.choiceNodes.Add(this);
			Node_Editor.editor.nodeCanvas.choiceNodeNames.Add(this.name);
		}

		if (!String.IsNullOrEmpty (AssetDatabase.GetAssetPath (Node_Editor.editor.nodeCanvas)))
		{
			AssetDatabase.AddObjectToAsset (this, Node_Editor.editor.nodeCanvas);
			for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
				AssetDatabase.AddObjectToAsset (Inputs [inCnt], this);
			for (int outCnt = 0; outCnt < Outputs.Count; outCnt++) 
				AssetDatabase.AddObjectToAsset (Outputs [outCnt], this);

			AssetDatabase.ImportAsset (Node_Editor.editor.openedCanvasPath);
			AssetDatabase.Refresh ();
		}
	}

	/// <summary>
	/// Function implemented by the children to draw the node
	/// </summary>
	public abstract void DrawNode ();

	/// <summary>
	/// Function implemented by the children to calculate their outputs
	/// Should return Success/Fail
	/// </summary>
	public abstract bool Calculate ();

	/// <summary>
	/// Draws the node curves as well as the knobs	
	/// </summary>
	public void DrawConnectors () 
	{
		for (int outCnt = 0; outCnt < Outputs.Count; outCnt++) 
		{
			NodeOutput output = Outputs [outCnt];

			for (int conCnt = 0; conCnt < output.connections.Count; conCnt++) 
			{
				if (output.connections [conCnt] != null) 
					Node_Editor.DrawNodeCurve (output.GetKnob ().center, 
					                           output.connections [conCnt].GetKnob ().center);
				else
					output.connections.RemoveAt (conCnt);
			}

			GUI.DrawTexture (output.GetKnob (), Node_Editor.ConnectorKnob);
		}

		for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
		{
			GUI.DrawTexture (Inputs [inCnt].GetKnob (), Node_Editor.ConnectorKnob);
		}
	}

	/// <summary>
	/// Callback when the node is deleted. Extendable by the child node, but always call base.OnDelete when overriding !!
	/// </summary>
	public virtual void OnDelete () 
	{
		for (int outCnt = 0; outCnt < Outputs.Count; outCnt++) 
		{
			NodeOutput output = Outputs [outCnt];

			for (int conCnt = 0; conCnt < output.connections.Count; conCnt++) 
				output.connections [outCnt].connection = null;
		}

		for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
		{
			if (Inputs [inCnt].connection != null)
				Inputs [inCnt].connection.connections.Remove (Inputs [inCnt]);
		}

		DestroyImmediate (this, true);

		if (!String.IsNullOrEmpty (Node_Editor.editor.openedCanvasPath)) 
		{
			AssetDatabase.ImportAsset (Node_Editor.editor.openedCanvasPath);
			AssetDatabase.Refresh ();
		}
	}

	/// <summary>
	/// Draws the toggle for the expand button and handles logic for true and false cases.
	/// </summary>
	protected virtual void DrawExpandedUI()
	{
		// draw expanded arrow
		GUILayout.BeginHorizontal();
		expanded = EditorGUILayout.Foldout(expanded, "Collapse");

		if (!expanded)
		{
			// if the node isn't expanded, collapse it and render the input/output handles only.
			rect.height = 50;

			if (Inputs[0].connection != null && Inputs[0].connection.body.state == -2)
				GUILayout.Label("Choice: " + description);

			if (Event.current.type == EventType.Repaint)
				Inputs[0].SetRect(GUILayoutUtility.GetLastRect());
			if (Event.current.type == EventType.Repaint)
				Outputs[0].SetRect(GUILayoutUtility.GetLastRect());

			if (GUI.changed || Outputs[0].connections.Count != numberOfTransitions)
			{
				numberOfTransitions = Outputs[0].connections.Count;
				Node_Editor.editor.RecalculateFrom(this);
			}

			GUILayout.EndHorizontal();

			return;
		}

		GUILayout.EndHorizontal();
	}

	protected virtual void DrawInputOutputHandles()
	{
		//////////////////////////////////
		// Render input and output handles
		//////////////////////////////////

		EditorGUIUtility.labelWidth = 0; // Align the label with the left side of the node. 
		GUILayout.BeginHorizontal();

		// Render input handle
		GUILayout.Label("Previous Dialogue State");

		if (Event.current.type == EventType.Repaint)
			Inputs[0].SetRect(GUILayoutUtility.GetLastRect());

		// Render output handle.
		GUILayout.FlexibleSpace(); // allows text to appear on right side of node.
		GUILayout.Label("Next Dialogue State");

		if (Event.current.type == EventType.Repaint)
			Outputs[0].SetRect(GUILayoutUtility.GetLastRect());

		GUILayout.EndHorizontal();
	}

	#region Member Functions

	/// <summary>
	/// Checks if there are no unassigned and no null-value inputs.
	/// </summary>
	public bool allInputsReady () 
	{
		for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
		{
			if (Inputs [inCnt].connection == null || Inputs [inCnt].connection.value == null)
				return false;
		}

		return true;
	}

	/// <summary>
	/// Checks if there are any unassigned inputs.
	/// </summary>
	public bool hasNullInputs () 
	{
		for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
		{
			if (Inputs [inCnt].connection == null)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if there are any null-value inputs.
	/// </summary>
	public bool hasNullInputValues () 
	{
		for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
		{
			if (Inputs [inCnt].connection != null && Inputs [inCnt].connection.value == null)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Returns the input knob that is at the position on this node or null
	/// </summary>
	public NodeInput GetInputAtPos (Vector2 pos) 
	{
		for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
		{ 
			// Search for an input at the position
			if (Inputs [inCnt].GetKnob ().Contains (new Vector3 (pos.x, pos.y)))
				return Inputs [inCnt];
		}

		return null;
	}

	/// <summary>
	/// Returns the output knob that is at the position on this node or null
	/// </summary>
	public NodeOutput GetOutputAtPos (Vector2 pos) 
	{
		for (int outCnt = 0; outCnt < Outputs.Count; outCnt++) 
		{ 
			// Search for an output at the position
			if (Outputs [outCnt].GetKnob ().Contains (new Vector3 (pos.x, pos.y)))
				return Outputs [outCnt];
		}

		return null;
	}

	/// <summary>
	/// Recursively checks whether this node is a child of the other node
	/// </summary>
	public bool isChildOf (Node otherNode)
	{
		if (otherNode == null)
			return false;

		for (int cnt = 0; cnt < Inputs.Count; cnt++) 
		{
			if (Inputs [cnt].connection != null) 
			{
				if (Inputs [cnt].connection.body == otherNode)
					return true;
				else if (Inputs [cnt].connection.body.isChildOf (otherNode)) // Recursively searching
					return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Determins if this node is a start or a final state.
	/// </summary>
	protected void CalculateStartAndFinalStates()
	{
		// If we have no inputs, then we know this state is a start state.
		if (Inputs[0].connection == null)
			isStartState = true;
		else
			isStartState = false;

		// If we have no outputs and no previous choice node, then we know this node is a valid final state.
		if (Outputs[0].connections.Count == 0 && previousChoiceNode == null)
			isFinalState = true;
		else
			isFinalState = false;
	}

	/// <summary> Returns true if the node is safe to save. Overridden by subclasses. </summary>
	public abstract bool IsNodeSafeToSave();

	#endregion

	#region static Functions

	/// <summary>
	/// Check if an output and an input can be connected (same type, ...)
	/// </summary>
	public static bool CanApplyConnection (NodeOutput output, NodeInput input)
	{
		if (input == null || output == null)
			return false;

		if (input.body == output.body || input.connection == output)
			return false;

		if (input.type != output.type)
			return false;

		if (output.body.isChildOf (input.body)) 
		{
			Node_Editor.editor.ShowNotification (new GUIContent ("Recursion detected!"));
			return false;
		}

		return true;
	}

	/// <summary>
	/// Applies a connection between output and input. 'CanApplyConnection' has to be checked before
	/// </summary>
	public static void ApplyConnection (NodeOutput output, NodeInput input)
	{
		if (input != null && output != null) 
		{
			if (input.connection != null) 
			{
				input.connection.connections.Remove (input);
			}

			input.connection = output;
			output.connections.Add (input);

			Node_Editor.editor.RecalculateFrom (input.body);
		}
	}

	#endregion
}
