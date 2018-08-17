using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This node is similar to the DialogueNode but includes data for In-Progress and Turn-In conversation options.
/// Inherits DialogueNode and implements IQuestNode interface.
/// </summary>
[System.Serializable]
public class QuestDialogueNode : DialogueNode, IQuestNode
{
	public bool accept, reject; // bools for tracking if this node represents an accept or reject quest state.

	public bool turnInQuest, inProgressDialogue;

	/// <summary>
	/// Creates a new QuestDialogueNode and initialies its data.
	/// </summary>
	public new static void Create(Rect NodeRect, int newStateNumber)
	{ 
		// This function has to be registered in Node_Editor_Quest.ContextCallback
		QuestDialogueNode node = ScriptableObject.CreateInstance<QuestDialogueNode>();

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
		node.accept = false;
		node.reject = false;
		node.addSpeaker = false;
		node.removeSpeaker = false;
		node.turnInQuest = false;
		node.inProgressDialogue = false;
		node.returnsToChoiceNode = false;
		node.previousChoiceNode = null;
		node.choiceStateIndex = 0;

		EditorStyles.textField.wordWrap = true; // turn on text wrapping for the dialogue field

		NodeInput.Create(node, "Previous Dialogue State", typeof(int));
		NodeOutput.Create(node, "Next Dialogue State", typeof(int));

		node.Init();
	}

	/// <summary>
	/// Returns true if this node is part of a turn-in quest conversation.
	/// </summary>
	public bool IsTurnInDialogue()
	{
		return turnInQuest;
	}

	/// <summary>
	/// Returns true if this node is part of an in-progress quest conversation.
	/// </summary>
	public bool IsInProgressDialogue()
	{
		return inProgressDialogue;
	}

	/// <summary>
	/// Renders this node in the node editor.
	/// </summary>
	public override void DrawNode()
	{
		DrawExpandedUI();

		if (!expanded)
			return;

		DrawInputOutputHandles();

		DrawStartStateUI();

		DrawProgressOptions();
	
		GUILayout.Space(15);

		DrawPreviousChoiceToggle();

		DrawChoiceResultUI();

		DrawSpeakerUI();

		UpdateTransitions();
	}

	#region UI Functions

	/// <summary>
	/// Draws turn-in and in-progress toggles on the node. Only renders if this node is a start state.
	/// </summary>
	private void DrawProgressOptions()
	{
		if(isStartState)
		{
			if (!inProgressDialogue)
				turnInQuest = EditorGUILayout.Toggle(new GUIContent("Quest Turn In", "Check this box if this conversation happens upon turning in a quest"), turnInQuest);

			if (!turnInQuest)
				inProgressDialogue = EditorGUILayout.Toggle(new GUIContent("Quest In Progress", "Check this box is this conversation"), inProgressDialogue);
		}
	}

	/// <summary>
	/// Draws data specific to this node if this node is coming from a choice node.
	/// </summary>
	protected override void DrawChoiceResultUI()
	{
		if (Inputs[0].connection != null && Inputs[0].connection.body.state == -2)
		{
			// This runs if this node comes from a choice node.
			description = EditorGUILayout.TextField(new GUIContent("Dialogue Description", "Description is used for identifying branching dialogue options. Only need to use this when this dialogue is a choice between other dialogues."), description);

			if (!turnInQuest && !inProgressDialogue)
			{
				GUILayout.Space(10);
				accept = EditorGUILayout.Toggle(
					new GUIContent("Accept Quest", "Selecting this option sets this choice as accepting the quest"),
					accept);
				reject = EditorGUILayout.Toggle(
					new GUIContent("Reject Quest", "Selecting this sets this choice as a rejection of the quest"),
					reject);
			}

			speakers[0] = (GameObject)Resources.Load("Prefabs/Characters/Player");
		}
	}

	#endregion

	#region Helper Functions

	/// <summary>
	/// Calculates input/output data for this node. Also calculates start/final state information, and turn-in, in-progress status.
	/// Note that turn-in and in-progress status proliferates from the node where those bools are set and outward
	/// using the inputs and outputs of each node. Essentially, the in-progress and turn-in bool is cascaded to
	/// all the other connected nodes.
	/// </summary>
	public override bool Calculate()
	{
		// Calculate start state
		if (Inputs[0].connection == null)
			isStartState = true;
		else
		{
			isStartState = false;

			// Set the input node to in-progress or turn-in if either option are selected.
			if (Inputs[0].connection.body.GetType() == typeof(QuestDialogueNode))
				SetDialogueTypeQuestDialogueNode();
			else if (Inputs[0].connection.body.GetType() == typeof(QuestChoiceNode))
				SetDialogueTypeChoiceNode();
		}

		// Set final state
		if (Outputs[0].connections.Count == 0 && previousChoiceNode == null)
			isFinalState = true;
		else
			isFinalState = false;

		if (Outputs[0].connections.Count > 0)
			Outputs[0].value = 0;

		return true;
	}

	/// <summary>
	/// Sets the input node as part of a turn-in or in-progress conversation.
	/// </summary>
	private void SetDialogueTypeQuestDialogueNode()
	{
		QuestDialogueNode temp = (QuestDialogueNode)Inputs[0].connection.body;

		if (temp.turnInQuest)
			turnInQuest = true;
		if (temp.inProgressDialogue)
			inProgressDialogue = true;
	}

	/// <summary>
	/// sets the input choice node as part of a turn-in or in-progress conversation.
	/// </summary>
	private void SetDialogueTypeChoiceNode()
	{
		QuestChoiceNode temp = (QuestChoiceNode)Inputs[0].connection.body;

		if (temp.turnInQuest)
			turnInQuest = true;
		if (temp.inProgressDialogue)
			inProgressDialogue = true;
	}

	#endregion
}
