using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Object = UnityEngine.Object;

/// <summary>
/// This is the base class for the node editor. Subclasses can be created for handling custom node editors.
/// For example, NodeEditorDialogue.cs inherits from Node_Editor and has specific functionality related to creating
/// conversations. When you click the "Dialogue Creator" -> "Create Conversation" option from the menu bar in Unity,
/// the NodeEditorDialogue script runs.
/// </summary>
public abstract class Node_Editor : EditorWindow 
{

	/// <summary> The node canvas associated with this editor. Node canvas contains data as defined in Node_Canvas_Object.cs </summary>
	protected Node_Canvas_Object _nodeCanvas;

	/// <summary> dynamic node canvas type. This is useful for getting and setting the node canvas as a specific Node_Canvas_Object subclass. </summary>
	public abstract dynamic nodeCanvas { get; set; }

	/// <summary> Static instance of this editor window. </summary>
	public static Node_Editor editor;

	///////////////// 
	// File path variables
	/////////////////

	public const string editorPath = "Assets/Node_Editor/";
	public string openedCanvas = "New Canvas";
	protected string defaultPath;
	public string openedCanvasPath;

	//////////////////////
	// Dimension Variables
	//////////////////////

	public int sideWindowWidth = 400;
	public int knobSize = 16;

	/////////////////
	// Node Variables
	/////////////////

	public Node activeNode; // Handled by Unity. For new Windowing System
	public bool dragNode = false; // Handled by Unity. For new Windowing System
	public NodeOutput connectOutput;

	/////////////////
	// UI Variables
	/////////////////

	public bool navigate = false;
	public bool scrollWindow = false;
	public Vector2 mousePos;

	public static Texture2D ConnectorKnob;
	public static Texture2D Background;
	public static GUIStyle nodeBase;
	public static GUIStyle nodeBox;
	public static GUIStyle nodeLabelBold;
	public static GUIStyle nodeButton;

	/////////////////
	// Miscellaneous
	/////////////////

	protected bool initiated;

	/// <summary> used for tracking how many dialogue states we have. </summary>
	protected int numberOfStates;

	/// <summary> used for tracking how many choice states we have. This is needed for passing control back to choice nodes </summary>
	protected int numberOfChoiceStates;

	/// <summary> Used for adding and removing game state buttons. </summary>
	protected bool addRequiredGameState, removeRequiredGameState;

	/// <summary> The game state asset that's created in this editor window. </summary>
	protected GameStateAsset gameStateAsset;

	/// <summary> List of game state names. Populated using the main Game State Asset file. </summary>
	protected List<string> requiredGameStateNames;

	/// <summary> Texture for the delete button that shows up next to certain items on the side window. </summary>
	protected Texture deleteButton, upButton, downButton;

	public void checkInit () 
	{
		// initialize editor window dimensions and styles.

		if (!initiated || _nodeCanvas == null) 
		{
			ConnectorKnob = EditorGUIUtility.Load ("icons/animationkeyframe.png") as Texture2D;
			Background = AssetDatabase.LoadAssetAtPath (editorPath + "background.png", typeof(Texture2D)) as Texture2D;

			nodeBase = new GUIStyle (GUI.skin.box);
			nodeBase.normal.background = ColorToTex (new Color (0.5f, 0.5f, 0.5f));
			nodeBase.normal.textColor = new Color (0.7f, 0.7f, 0.7f);

			nodeBox = new GUIStyle (nodeBase);
			nodeBox.margin = new RectOffset (8, 8, 5, 8);
			nodeBox.padding = new RectOffset (8, 8, 8, 8);

			nodeLabelBold = new GUIStyle (nodeBase);
			nodeLabelBold.fontStyle = FontStyle.Bold;
			nodeLabelBold.wordWrap = false;

			nodeButton = new GUIStyle (GUI.skin.button);
			nodeButton.normal.textColor = new Color (0.3f, 0.3f, 0.3f);

			NewNodeCanvas ();

			numberOfStates = 0;
			numberOfChoiceStates = 0;

			initiated = true;
		}
	}

	#region GUI

	public void OnGUI () 
	{
		// This runs multiple times per second.

		checkInit();

		InputEvents();

		DrawNodes();

		SetupSideWindow();
	}

	/// <summary>
	/// Initializes the side window dimensions and draws the side window.
	/// </summary>
	protected virtual void SetupSideWindow()
	{
		sideWindowWidth = Math.Min(600, Math.Max(200, (int)(position.width / 5)));
		GUILayout.BeginArea(sideWindowRect, nodeBox);
		DrawSideWindow();
		GUILayout.EndArea();
	}

	/// <summary>
	/// Loads the main game state asset into memory and adds all the Game States to the requiredGameStateNames list.
	/// </summary>
	protected void InitGameStateData()
	{
		// Modify this file path if needed.
		gameStateAsset = Resources.Load<GameStateAsset>("GameState/GameStateData");

		// Initialize list of game state names. These names will be displayed on the side window.
		requiredGameStateNames = new List<string>();

		// Initialize the list of bools for which game states are selected as prerequisite states for this interaction.
		_nodeCanvas.requiredGameStateSelections = new List<bool>();

		// Iterate over the game states and populate the list of names and bools.
		foreach (GameStateName state in gameStateAsset.gameStates)
		{
			requiredGameStateNames.Add(state.eventName);
			_nodeCanvas.requiredGameStateSelections.Add(false);
		}
	}

	/// <summary>
	/// Draws the game state names on the side window. Each name has a toggle box next to it for choosing it as 
	/// a prereq or not.
	/// </summary>
	protected void DisplayRequiredGameStates()
	{
		// header text
		GUILayout.Label("Required Game States");

		GUILayout.Space(20);

		EditorGUIUtility.labelWidth = 250;

		// Draw the game state names and toggle boxes.
		for (int i = 0; i < gameStateAsset.gameStates.Count; i++)
		{
			GUILayout.BeginHorizontal();

			// Draw a toggle box with the name of the state next to it.
			nodeCanvas.requiredGameStateSelections[i] = EditorGUILayout.Toggle(new GUIContent(gameStateAsset.gameStates[i].eventName, "Select this box to add this game state to the list of required states"), nodeCanvas.requiredGameStateSelections[i]);

			if (nodeCanvas.requiredGameStateSelections[i])
				AddRequiredGameState(gameStateAsset.gameStates[i]);
			else
				RemoveRequiredGameState(gameStateAsset.gameStates[i]);

			GUILayout.EndHorizontal();
		}
		EditorGUIUtility.labelWidth = 0;
	}

	/// <summary>
	/// Adds a game state to the list of prerequisite game states.
	/// </summary>
	/// <param name="gameState">The game state to add.</param>
	protected void AddRequiredGameState(GameStateName gameState)
	{
		if (nodeCanvas.requiredGameStates.Contains(gameState))
			return;

		nodeCanvas.requiredGameStates.Add(gameState);
	}

	/// <summary>
	/// Removes a game state from the list of prerequisite game states.
	/// </summary>
	/// <param name="gameState">The game state to remove.</param>
	protected void RemoveRequiredGameState(GameStateName gameState)
	{
		if (!nodeCanvas.requiredGameStates.Contains(gameState))
			return;

		nodeCanvas.requiredGameStates.Remove(gameState);
	}

	/// <summary>
	/// Draws the nodes in the node editor window.
	/// </summary>
	protected void DrawNodes()
	{
		// draw the nodes
		BeginWindows();
		for (int nodeCnt = 0; nodeCnt < _nodeCanvas.nodes.Count; nodeCnt++)
		{
			if (_nodeCanvas.nodes[nodeCnt] != null)
				_nodeCanvas.nodes[nodeCnt].rect = GUILayout.Window(nodeCnt, _nodeCanvas.nodes[nodeCnt].rect, DrawNode, _nodeCanvas.nodes[nodeCnt].name);
		}
		EndWindows();

		// draw their connectors
		for (int nodeCnt = 0; nodeCnt < _nodeCanvas.nodes.Count; nodeCnt++)
		{
			_nodeCanvas.nodes[nodeCnt].DrawConnectors();
		}
	}
	
	/// <summary>
	/// Draws the side window. Can be overridden in sub classes. Default behavior is to only draw save/load options.
	/// </summary>
	public virtual void DrawSideWindow () 
	{
		DrawSaveLoadSettings();
	}

	/// <summary>
	/// Draws save, load, new, and recalculate buttons.
	/// </summary>
	protected void DrawSaveLoadSettings()
	{
		// Draw the name of this asset
		GUILayout.Label(new GUIContent("Node Editor (" + openedCanvas + ")", "The currently opened canvas in the Node Editor"), nodeLabelBold);

		// Draw a message about saving.
		GUILayout.Label(new GUIContent("Do note that changes will be saved automatically!", "All changes are automatically saved to the currently opened canvas (see above) if it's present in the Project view."), nodeBase);

		// Draw the save button.
		if (GUILayout.Button(new GUIContent("Save Canvas", "Saves the canvas as a new Canvas Asset File in the Assets Folder"), nodeButton))
		{
			if (!IsSafeToSave()) // check if the data in the editor is ready to be saved.
				return;

			// save the file.
			SaveNodeCanvas(EditorUtility.SaveFilePanelInProject("Save Node Canvas", "Node Canvas", "asset", "Saving to a file is only needed once.", defaultPath));
		}

		// Draw the load button.
		if (GUILayout.Button(new GUIContent("Load Canvas", "Loads the canvas from a Canvas Asset File in the Assets Folder"), nodeButton))
		{
			// Open a file panel and populate the load path.
			string path = EditorUtility.OpenFilePanel("Load Node Canvas", defaultPath, "asset");

			if (!path.Contains(Application.dataPath))
			{
				// If the chosen path does not reside in the project directory, send a notification.
				if (path != String.Empty)
					ShowNotification(new GUIContent("You should select an asset inside your project folder!"));
				return;
			}

			path = path.Replace(Application.dataPath, "Assets");
			LoadNodeCanvas(path);
		}

		// Draw new canvas button.
		if (GUILayout.Button(new GUIContent("New Canvas", "Creates a new Canvas (remember to save the previous one to a referenced Canvas Asset File at least once before! Else it'll be lost!)"), nodeButton))
		{
			NewNodeCanvas();
		}

		// Draw recalculate button.
		if (GUILayout.Button(new GUIContent("Recalculate All", "Starts to calculate from the beginning off."), nodeButton))
		{
			RecalculateAll();
		}

		// Draw knob size slider.
		knobSize = EditorGUILayout.IntSlider(new GUIContent("Handle Size", "The size of the handles of the Node Inputs/Outputs"), knobSize, 8, 32);
	}

	#endregion

	#region Calculation

	List<Node> workList;

	/// <summary>
	/// Recalculate from every Input Node.
	/// Usually does not need to be called at all, the smart calculation system is doing the job just fine
	/// </summary>
	public void RecalculateAll () 
	{
		workList = new List<Node> ();
		for (int nodeCnt = 0; nodeCnt < _nodeCanvas.nodes.Count; nodeCnt++) 
		{
			if (_nodeCanvas.nodes [nodeCnt].Inputs.Count == 0) 
			{
				workList.Add (_nodeCanvas.nodes [nodeCnt]);
				ClearChildrenInput (_nodeCanvas.nodes [nodeCnt]);
			}
		}
		Calculate ();
	}

	/// <summary>
	/// Recalculate from node. 
	/// Usually does not need to be called manually
	/// </summary>
	public void RecalculateFrom (Node node) 
	{
		workList = new List<Node> { node };
		ClearChildrenInput (node);
		Calculate ();
	}

	/// <summary>
	/// Iterates through the worklist and calculates everything, including children
	/// </summary>
	protected void Calculate () 
	{
		// this blocks iterates through the worklist and starts calculating
		// if a node returns false state it stops and adds the node to the worklist
		// later on, this worklist is reworked
		bool limitReached = false;

		for (int roundCnt = 0; !limitReached; roundCnt++)
		{ 
			// Runs until every node that can be calculated are calculated
			limitReached = true;

			for (int workCnt = 0; workCnt < workList.Count; workCnt++) 
			{
				Node node = workList [workCnt];

				if (node.Calculate ())
				{ 
					// finished Calculating, continue with the children
					for (int outCnt = 0; outCnt < node.Outputs.Count; outCnt++)
					{
						NodeOutput output = node.Outputs [outCnt];

						for (int conCnt = 0; conCnt < output.connections.Count; conCnt++)
							ContinueCalculation (output.connections [conCnt].body);
					}

					if (workList.Contains (node))
						workList.Remove (node);

					limitReached = false;
				}
				else if (!workList.Contains (node)) 
				{ 
					// Calculate returned false state (due to missing inputs / whatever), add it to check later
					workList.Add (node);
				}
			}
		}
	}

	/// <summary>
	/// A recursive function to clear all inputs that depend on the outputs of node. 
	/// Usually does not need to be called manually
	/// </summary>
	protected void ClearChildrenInput (Node node) 
	{
		node.Calculate ();

		for (int outCnt = 0; outCnt < node.Outputs.Count; outCnt++)
		{
			NodeOutput output = node.Outputs [outCnt];
			output.value = null;

			for (int conCnt = 0; conCnt < output.connections.Count; conCnt++)
				ClearChildrenInput (output.connections [conCnt].body);
		}
	}

	/// <summary>
	/// Continues calculation on this node to all the child nodes
	/// Usually does not need to be called manually
	/// </summary>
	protected void ContinueCalculation (Node node) 
	{
		if (node.Calculate ())
		{ 
			// finished Calculating, continue with the children
			for (int outCnt = 0; outCnt < node.Outputs.Count; outCnt++)
			{
				NodeOutput output = node.Outputs [outCnt];

				for (int conCnt = 0; conCnt < output.connections.Count; conCnt++)
				{
					ContinueCalculation (output.connections [conCnt].body);
				}
			}
		}
		else if (!workList.Contains (node))
			workList.Add (node);
	}
	#endregion

	#region Events

	/// <summary>
	/// Processes input events
	/// </summary>
	protected void InputEvents () 
	{
		Event e = Event.current;
		mousePos = e.mousePosition;

		Node clickedNode = null;

		if (e.type == EventType.MouseDown || e.type == EventType.MouseUp)
			clickedNode = NodeAtPosition (e.mousePosition);

		if (e.type == EventType.Repaint) 
		{ 
			// Draw background when repainting
			Vector2 offset = new Vector2 (_nodeCanvas.scrollOffset.x%Background.width - Background.width,
										  _nodeCanvas.scrollOffset.y%Background.height - Background.height);
			int tileX = Mathf.CeilToInt ((position.width + (Background.width - offset.x)) / Background.width);
			int tileY = Mathf.CeilToInt ((position.height + (Background.height - offset.y)) / Background.height);
			
			for (int x = 0; x < tileX; x++) 
			{
				for (int y = 0; y < tileY; y++) 
				{
					Rect texRect = new Rect (offset.x + x*Background.width, 
					                         offset.y + y*Background.height, 
					                         Background.width, Background.height);
					GUI.DrawTexture (texRect, Background);
				}
			}
		}
		
		if (e.type == EventType.MouseDown) 
		{
			activeNode = clickedNode;
			connectOutput = null;

			if (clickedNode != null) 
			{ 
				// A click on a node
				if (e.button == 1)
				{ 
					// Right click -> Node Context Click
					GenericMenu menu = new GenericMenu ();
					
					menu.AddItem (new GUIContent ("Delete Node"), false, ContextCallback, "deleteNode");
					
					menu.ShowAsContext ();
					e.Use();
				}
				else if (e.button == 0)
				{ 
					/* // Handled by Unity. For new Windowing System
					// Left click -> check for drag on the header and for transition edits, else let it pass for gui elements
					if (new Rect (clickedNode.rect.x, clickedNode.rect.y, clickedNode.rect.width, 40).Contains (mousePos))
					{ // We clicked the header, so we'll drag the node
						dragNode = true;
						e.delta = new Vector2 (0, 0);
					}*/

					// If a Connection was left clicked, try edit it's transition
					NodeOutput nodeOutput = clickedNode.GetOutputAtPos (mousePos);

					if (nodeOutput != null)
					{ // Output Node -> New Connection drawn from this
						connectOutput = nodeOutput;
						e.Use();
					}
					else 
					{ // no output clicked, check input
						NodeInput nodeInput = clickedNode.GetInputAtPos (mousePos);

						if (nodeInput != null && nodeInput.connection != null)
						{ // Input node -> Loose and edit Connection
							connectOutput = nodeInput.connection;
							nodeInput.connection.connections.Remove (nodeInput);
							nodeInput.connection = null;
							RecalculateFrom (clickedNode);
							e.Use();
						} // Nothing interesting for us in the node clicked, so let the event pass to gui elements
					}
				}
			}
			else if (!sideWindowRect.Contains (mousePos))
			{ 
				// A click on the empty canvas
				if (e.button == 2 || e.button == 0)
				{
					// Left/Middle Click -> Start scrolling
					scrollWindow = true;
					e.delta = new Vector2 (0, 0);
				}
				else if (e.button == 1) 
				{ 
					// Right click -> Editor Context Click
					GenericMenu menu = new GenericMenu ();

					AddRightClickMenuItems(menu);

					menu.ShowAsContext ();
					e.Use();
				} 
			}
		}
		else if (e.type == EventType.MouseUp) 
		{
			if (connectOutput != null) 
			{ 
				// Apply a connection if theres a clicked input
				if (clickedNode != null && !clickedNode.Outputs.Contains (connectOutput)) 
				{	
					// If an input was clicked, it'll will now be connected
					NodeInput clickedInput = clickedNode.GetInputAtPos (mousePos);

					if (Node.CanApplyConnection (connectOutput, clickedInput)) 
					{ 
						// If it can connect (type is equals, it does not cause recursion, ...)
						Node.ApplyConnection (connectOutput, clickedInput);
					}
				}
				e.Use();
			}
			else if (e.button == 2 || e.button == 0)
			{ 
				// Left/Middle click up -> Stop scrolling
				scrollWindow = false;
			}

			connectOutput = null;
		}
		else if (e.type == EventType.KeyDown)
		{
			if (e.keyCode == KeyCode.N) // Start Navigating (curve to origin)
				navigate = true;
		}
		else if (e.type == EventType.KeyUp)
		{
			if (e.keyCode == KeyCode.N) // Stop Navigating
				navigate = false;
		}
		else if (e.type == EventType.Repaint) 
		{
			if (navigate) 
			{ 
				// Draw a curve to the origin/active node for orientation purposes
				DrawNodeCurve (_nodeCanvas.scrollOffset, (activeNode != null? activeNode.rect.center : e.mousePosition)); 
				Repaint ();
			}
			if (connectOutput != null)
			{ 
				// Draw the currently drawn connection
				DrawNodeCurve (connectOutput.GetKnob ().center, e.mousePosition);
				Repaint ();
			}
		}
		if (scrollWindow) 
		{ 
			// Scroll everything with the current mouse delta
			_nodeCanvas.scrollOffset += e.delta / 2;

			for (int nodeCnt = 0; nodeCnt < _nodeCanvas.nodes.Count; nodeCnt++)
				_nodeCanvas.nodes [nodeCnt].rect.position += e.delta / 2;

			Repaint ();
		}
		/* // Handled by Unity. For new Windowing System
		if (dragNode) 
		{ // Drag the active node with the current mouse delt
			activeNode.rect.position += e.delta / 2;
			Repaint ();
		}*/
	}

	/// <summary>
	/// Populates the list of items that show up in the right-click menu in the node editor. Overridden by subclasses.
	/// </summary>
	protected abstract void AddRightClickMenuItems(GenericMenu menu);

	/// <summary>
	/// Context Click selection. Here you'll need to register your own using a string identifier
	/// </summary>
	public abstract void ContextCallback(object obj);

	#endregion

	#region GUI Functions

	/// <summary>
	/// Returns the rect for the side window.
	/// </summary>
	public Rect sideWindowRect 
	{
		get { return new Rect (position.width - sideWindowWidth, 0, sideWindowWidth, position.height); }
	}

	/// <summary>
	/// Converts a color to a texture.
	/// </summary>
	/// <param name="col">Color to convert.</param>
	public static Texture2D ColorToTex (Color col) 
	{
		Texture2D tex = new Texture2D (1,1);
		tex.SetPixel (1, 1, col);
		tex.Apply ();
		return tex;
	}

	public static Texture2D Tint (Texture2D tex, Color col) 
	{
		for (int x = 0; x < tex.width; x++) 
		{
			for (int y = 0; y < tex.height; y++) 
			{
				tex.SetPixel (x, y, tex.GetPixel (x, y) * col);
			}
		}
		tex.Apply ();
		return tex;
	}

	/// <summary>
	/// Returns the node at the position
	/// </summary>
	public Node NodeAtPosition (Vector2 pos)
	{	
		if (sideWindowRect.Contains (pos))
			return null;
		// Check if we clicked inside a window (or knobSize pixels left or right of it at outputs, for easier knob recognition)
		for (int nodeCnt = _nodeCanvas.nodes.Count-1; nodeCnt >= 0; nodeCnt--) 
		{ // From top to bottom because of the render order (though overwritten by active Window, so be aware!)
			Rect NodeRect = new Rect (_nodeCanvas.nodes [nodeCnt].rect);
			NodeRect = new Rect (NodeRect.x - knobSize, NodeRect.y, NodeRect.width + knobSize*2, NodeRect.height);
			if (NodeRect.Contains (pos))
				return _nodeCanvas.nodes [nodeCnt];
		}
		return null;
	}
	
	/// <summary>
	/// Draws the node
	/// </summary>
	protected void DrawNode (int id)
	{
		_nodeCanvas.nodes [id].DrawNode ();
		GUI.DragWindow ();

		/* // Handled by Unity. For new Windowing System
		Rect headerRect = new Rect (node.rect.x, node.rect.y, node.rect.width, 20);
		Rect bodyRect = new Rect (node.rect.x, node.rect.y + 20, node.rect.width, node.rect.height - 40);
		GUI.Label (headerRect, new GUIContent (node.name));
		//GUI.Box (bodyRect, GUIContent.none, GUI.skin.box);
		GUILayout.BeginArea (bodyRect, nodeBox);
		node.DrawNode ();
		GUILayout.EndArea ();
		*/
	}
	
	/// <summary>
	/// Draws a node curve from start to end (with three shades of shadows! :O )
	/// </summary>
	public static void DrawNodeCurve (Vector2 start, Vector2 end) 
	{
		Vector3 startPos = new Vector3 (start.x, start.y);
		Vector3 endPos = new Vector3 (end.x, end.y);
		Vector3 startTan = startPos + Vector3.right * 50;
		Vector3 endTan = endPos + Vector3.left * 50;
		Color shadowColor = new Color (0, 0, 0, 0.1f);

		for (int i = 0; i < 3; i++) // Draw a shadow with 3 shades
			Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowColor, null, (i + 1) * 4); // increasing width for fading shadow
		Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 4);
	}

	#endregion

	#region Node Canvas
	
	/// <summary>
	/// Saves the current node canvas as a new asset. Can be overridden by subclasses.
	/// </summary>
	public virtual void SaveNodeCanvas (string path) 
	{
		if (String.IsNullOrEmpty (path)) // check if path exists. Exit if it doesn't.
			return;

		string existingPath = AssetDatabase.GetAssetPath (_nodeCanvas); // get the current path for the asset.

		if (!String.IsNullOrEmpty (existingPath))
		{
			// If we've chosen to save the file to a new path, then copy the asset and put it in the new path.
			if (existingPath != path) 
			{
				AssetDatabase.CopyAsset (existingPath, path);
				LoadNodeCanvas (path);
			}

			return;
		}

		// If we get here, we need to create the asset.
		AssetDatabase.CreateAsset (_nodeCanvas, path);

		for (int nodeCnt = 0; nodeCnt < _nodeCanvas.nodes.Count; nodeCnt++) 
		{ 
			// Add every node and every of it's inputs/outputs into the file. 
			// Results in a big mess but there's no other way
			Node node = _nodeCanvas.nodes [nodeCnt];
			AssetDatabase.AddObjectToAsset (node, _nodeCanvas);

			for (int inCnt = 0; inCnt < node.Inputs.Count; inCnt++) 
				AssetDatabase.AddObjectToAsset (node.Inputs [inCnt], node);

			for (int outCnt = 0; outCnt < node.Outputs.Count; outCnt++) 
				AssetDatabase.AddObjectToAsset (node.Outputs [outCnt], node);
		}

		string[] folders = path.Split (new char[] {'/'}, StringSplitOptions.None);
		openedCanvas = folders [folders.Length-1];
		openedCanvasPath = path;

		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh ();
		Repaint ();
	}

	/// <summary> Returns true if all nodes are safe to save, which is defined by the Node class. </summary>
	protected bool IsSafeToSave()
	{
		foreach(Node node in _nodeCanvas.nodes)
		{
			if (!node.IsNodeSafeToSave())
				return false;
		}

		return true;
	}

	/// <summary>
	/// Loads the a node canvas from an asset
	/// </summary>
	public abstract void LoadNodeCanvas(string path);

	/// <summary>
	/// Creates and opens a new empty node canvas
	/// </summary>
	public abstract void NewNodeCanvas();
	
	#endregion
}
