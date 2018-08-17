using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a dialogue node. Contains information such as who is speaking, where the state transitions to, what is being said and more. Inherits Node.
/// </summary>
[System.Serializable]
public class DialogueNode : Node
{
	/// <summary> List of speakers in this state. </summary>
	public List<GameObject> speakers;

	/// <summary> The images used to represent each speaker. </summary>
	public List<Texture2D> speakerImages;

	/// <summary> The list of dialogue in this state. Each string is associated with a speaker. </summary>
	public List<string> dialogues;

	/// <summary>The list of styles in this state. Each style is associated with a speaker. </summary>
	public List<Sprite> styles;

	/// <summary> List of transitions from this state. Each state is represented using integers. </summary>
	public List<int> transitions;

	/// <summary> If this is true, then the player initiates this conversation. </summary>
	public bool playerInitiatesConversation;

	protected bool addSpeaker, removeSpeaker; // Used for add/remove speaker buttons.

	/// <summary> If this is true, then this state transitions to a previous choice node. </summary>
	public bool returnsToChoiceNode;

	/// <summary> The selected choice state this state returns to if returnsToChoiceNode is true. </summary>
	protected int choiceStateIndex;

	/// <summary>
	/// Creates a new DialogueNode and initializes its data.
	/// </summary>
	public static void Create(Rect NodeRect, int newStateNumber)
	{ // This function has to be registered in Node_Editor.ContextCallback
		DialogueNode node = ScriptableObject.CreateInstance<DialogueNode>();

		node.name = "State " + newStateNumber.ToString();
		node.rect = NodeRect;
		node.state = newStateNumber;
		node.speakers = new List<GameObject>() { null };
		node.dialogues = new List<string>() { null };
		node.description = null;
		node.styles = new List<Sprite>() { null };
		node.speakerImages = new List<Texture2D>() { null };
		node.transitions = new List<int>();
		node.numberOfTransitions = 0;
		node.isStartState = false;
		node.isFinalState = false;
		node.playerInitiatesConversation = false;
		node.expanded = true;
		node.addSpeaker = false;
		node.removeSpeaker = false;
		node.previousChoiceNode = null;
		node.choiceStateIndex = 0;
		node.returnsToChoiceNode = false;

		EditorStyles.textField.wordWrap = true; // turn on text wrapping for the dialogue field

		NodeInput.Create (node, "Previous Dialogue State", typeof(int));
		NodeOutput.Create (node, "Next Dialogue State", typeof(int));

		node.Init();
	}

	/// <summary>
	/// Draws this node in the node editor.
	/// </summary>
	public override void DrawNode()
	{
		DrawExpandedUI();

		if (!expanded)
			return;

		DrawInputOutputHandles();

		DrawStartStateUI();
	
		GUILayout.Space(15);

		DrawPreviousChoiceToggle();

		DrawChoiceResultUI();

		DrawSpeakerUI();

		UpdateTransitions();
	}

	#region UI Functions

	/// <summary>
	/// Updates the transitions from this node.
	/// </summary>
	protected void UpdateTransitions()
	{
		if (GUI.changed || Outputs[0].connections.Count != numberOfTransitions)
		{
			numberOfTransitions = Outputs[0].connections.Count;
			Node_Editor.editor.RecalculateFrom(this);
		}
	}

	/// <summary>
	/// Draws node elements that are specific to states whose input comes from a choice node. 
	/// </summary>
	protected virtual void DrawChoiceResultUI()
	{
		if (Inputs[0].connection != null && Inputs[0].connection.body.state == -2)
		{
			// Draw the description text field.
			description = EditorGUILayout.TextField(new GUIContent("Dialogue Description", "Description is used for identifying branching dialogue options. Only need to use this when this dialogue is a choice between other dialogues."), description);

			// Sets the speaker to the player. Only the player responds immediately to a choice.
			speakers[0] = (GameObject)Resources.Load("Prefabs/Characters/Player");

			GUILayout.Space(10);
		}
	}

	/// <summary>
	/// Draws a toggle and object field for selecting a choice node to return to from this state.
	/// </summary>
	protected void DrawPreviousChoiceToggle()
	{
		if (Inputs[0].connection != null && Outputs[0].connections.Count == 0 && Node_Editor.editor.nodeCanvas.choiceNodes.Count > 0)
		{
			EditorGUIUtility.labelWidth = 250;

			// Draw the toggle.
			returnsToChoiceNode = EditorGUILayout.Toggle(new GUIContent("Transitions to Previous Choice Node", "Check this box if this state transitions to the last choice node"), returnsToChoiceNode);

			EditorGUIUtility.labelWidth = 0;

			// Draw choice state popup for user to select a choice state to return to from this node.
			if (returnsToChoiceNode)
			{
				choiceStateIndex = EditorGUILayout.Popup(choiceStateIndex, Node_Editor.editor.nodeCanvas.choiceNodeNames.ToArray());

				previousChoiceNode = (DialogueChoiceNode) Node_Editor.editor.nodeCanvas.choiceNodes[choiceStateIndex];

				GUILayout.Space(10);
			}

			GUILayout.Space(10);
		}
		else if(Outputs[0].connections.Count != 0)
		{
			returnsToChoiceNode = false;
			previousChoiceNode = null;
		}
	}

	/// <summary>
	/// Displays the speakers on this node.
	/// </summary>
	protected void DrawSpeakerUI()
	{
		////////////////////////////
		// Render Number of Speakers
		////////////////////////////

		addSpeaker = GUILayout.Button("Add Speaker");
		removeSpeaker = GUILayout.Button("Remove Speaker");

		GUILayout.Space(15);

		if (addSpeaker)
			AddEmptyDataToLists();
		if (removeSpeaker)
			PopItemsOffLists();

		// Render all the speakers and their related data.
		for (int i = 0; i < speakers.Count; i++)
		{
			speakers[i] = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Speaking Character", "The Game Object of the character who will speak this dialogue"), speakers[i], typeof(GameObject), false);

			// Draw an image of the NPC if the NPC exists.
			if (speakers[i])
			{
				speakerImages[i] = AssetPreview.GetAssetPreview(speakers[i].GetComponent<SpriteRenderer>().sprite);
				GUILayout.Label(speakerImages[i], GUILayout.MaxHeight(75));
			}

			GUILayout.Space(20);

			// Draw the dialogue field for this speaker.
			GUILayout.Label("Dialogue");
			dialogues[i] = EditorGUILayout.TextArea(dialogues[i], GUILayout.Height(50));

			GUILayout.Space(20);

			// Draw the styles for this speaker.
			GUILayout.Label("Speech Bubble Style");
			styles[i] = (Sprite)EditorGUILayout.ObjectField(styles[i], typeof(Sprite), false, GUILayout.Height(75), GUILayout.Width(75));

			GUILayout.Space(10);

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			GUILayout.Space(10);
		}
	}

	/// <summary>
	/// Draws relevant information that is specific to a node if it is a start state.
	/// </summary>
	protected void DrawStartStateUI()
	{
		if (Inputs[0].connection != null)
			isStartState = false;

		// Render player initiation box
		if (isStartState)
		{
			// This only occurs if this state is a start state, which is determined by whether or not the state has an input connection.
			GUILayout.Space(15);
			EditorGUIUtility.labelWidth = 175;

			playerInitiatesConversation = (bool)EditorGUILayout.Toggle(new GUIContent("Player Initiates Conversation", "Check this box if this conversation is initiated by the player"), playerInitiatesConversation);

			EditorGUIUtility.labelWidth = 0;
		}
	}
	#endregion

	#region Helper Functions

	/// <summary>
	/// Returns true if this node has no null reference styles or speakers.
	/// </summary>
	public override bool IsNodeSafeToSave()
	{
		foreach (GameObject speaker in speakers)
		{
			if (speaker == null)
			{
				// Display error message in the editor if we are missing a speaker reference.
				Node_Editor.editor.ShowNotification(new GUIContent("State " + state + " Needs A Game Object assigned to a Speaker"));
				return false;
			}
		}

		foreach (Sprite style in styles)
		{
			if (style == null)
			{
				// Display an error message in the editor if we're missing a style reference.
				Node_Editor.editor.ShowNotification(new GUIContent("State " + state + " Needs A Style Assigned to a Speaker Dialogue"));
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Calculates start and final state status of this node and input/output values.
	/// </summary>
	public override bool Calculate()
	{
		CalculateStartAndFinalStates();

		//CalculatePreviousChoiceNode();

		if (Outputs[0].connections.Count > 0)
			Outputs[0].value = 0;

		return true;
	}

	/// <summary>
	/// Adds empty data to all the lists.
	/// </summary>
	protected void AddEmptyDataToLists()
	{
		speakers.Add(null); // add a new, empty gameobject to the list of speakers.

		speakerImages.Add(null); // add a new texture to speaker images, if number of speakers exceeds # of images.

		dialogues.Add(null); // add a new, empty, dialogue to the list.

		styles.Add(null); // add a new, empty, style to the list.
	}

	/// <summary>
	/// Removes the last item from all the lists.
	/// </summary>
	protected void PopItemsOffLists()
	{
		if (speakers.Count == 0)
			return;

		speakers.Remove(speakers[speakers.Count - 1]);
		speakerImages.Remove(speakerImages[speakerImages.Count - 1]);
		dialogues.Remove(dialogues[dialogues.Count - 1]);
		styles.Remove(styles[styles.Count - 1]);
	}

	#endregion
}
