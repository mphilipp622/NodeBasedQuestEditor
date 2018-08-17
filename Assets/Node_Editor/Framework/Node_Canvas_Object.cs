using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Base class for node canvases. Node canvases are Scriptable Objects that contain serializable data that
/// is important for your node editor. Most of the data that you wish to parse from the asset will go in here.
/// Node_Editor.cs references Node_Canvas_Object.
/// </summary>
public class Node_Canvas_Object : ScriptableObject 
{
	// All necessary things to save placed in the Node Canvas Scriptable Object

	/// <summary> All nodes on the canvas. They include the connections themselves </summary>
	public List<Node> nodes; 

	/// <summary> The list of choice nodes in the editor. </summary>
	public List<Node> choiceNodes;

	/// <summary> The names of the choice nodes. Mostly used for the editor and not for loading at runtime. </summary>
	public List<string> choiceNodeNames;

	/// <summary> The list of game state selections. Used for the editor. Used during loading. </summary>
	public List<bool> requiredGameStateSelections;

	/// <summary> The required game states needed to activate this interaction. </summary>
	public List<GameStateName> requiredGameStates;

	public Vector2 scrollOffset = new Vector2 (); // The Scroll offset
	public float zoom = 2; // Zoom Factor; (1-5)/2: One step to zoom in, three to zoom out. Not implemented yet!
}