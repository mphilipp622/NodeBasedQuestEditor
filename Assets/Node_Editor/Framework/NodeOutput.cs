using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This class handles outputs from the node. Outputs are typically drawn on the right side of the node and can be
/// dragged and dropped onto another node's NodeInput.
/// </summary>
public class NodeOutput : ScriptableObject
{
	/// <summary>The node this output connects to.</summary>
	public Node body;
	public Rect rect = new Rect ();

	/// <summary>The list of inputs this output node connects to. Outputs can have multiple connections to 
	/// different inputs
	/// </summary>
	public List<NodeInput> connections = new List<NodeInput> ();

	public string type;
	[NonSerialized]
	public object value = null;

	/// <summary>
	/// Creates a new NodeOutput in NodeBody of specified type
	/// </summary>
	public static NodeOutput Create (Node NodeBody, string OutputName, Type OutputType)
	{
		NodeOutput output = NodeOutput.CreateInstance (typeof (NodeOutput)) as NodeOutput;
		output.body = NodeBody;
		output.type = OutputType.AssemblyQualifiedName;
		output.name = OutputName;
		NodeBody.Outputs.Add (output);
		return output;
	}

	/// <summary>
	/// Function to automatically draw and update the output with a label for it's name
	/// </summary>
	public void DisplayLayout () 
	{
		DisplayLayout (new GUIContent (name));
	}

	/// <summary>
	/// Function to automatically draw and update the output
	/// </summary>
	public void DisplayLayout (GUIContent content) 
	{
		GUIStyle style = new GUIStyle (UnityEditor.EditorStyles.label);
		style.alignment = TextAnchor.MiddleRight;
		GUILayout.Label (content, style);

		if (Event.current.type == EventType.Repaint) 
			SetRect (GUILayoutUtility.GetLastRect ());
	}
	
	/// <summary>
	/// Set the output rect as labelrect in global canvas space and extend it to the right node edge
	/// </summary>
	public void SetRect (Rect labelRect) 
	{
		rect = new Rect (body.rect.x + labelRect.x, 
		                 body.rect.y + labelRect.y, 
		                 body.rect.width - labelRect.x, 
		                 labelRect.height);
	}
	
	/// <summary>
	/// Get the rect of the knob right to the output
	/// </summary>
	public Rect GetKnob () 
	{
		int knobSize = Node_Editor.editor.knobSize;
		return new Rect (rect.x + rect.width, 
		                 rect.y + (rect.height - knobSize) / 2, 
		                 knobSize, knobSize);
	}
}