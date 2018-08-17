using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Choice node for the quest system. This node differs from a normal DialogueChocieNode by having references to
/// Turn in and In-Progress dialogue options. Inherits DialogueChoiceNode and implements IQuestNode interface. 
/// </summary>
[System.Serializable]
public class QuestChoiceNode : DialogueChoiceNode, IQuestNode
{
	public bool turnInQuest, inProgressDialogue;

	/// <summary>
	/// Creates a new quest choice node.
	/// </summary>
	public new static void Create(Rect NodeRect, int choiceStateNumber)
	{
		QuestChoiceNode node = ScriptableObject.CreateInstance<QuestChoiceNode>();

		node.name = "Choice State " + choiceStateNumber;
		node.rect = NodeRect;
		node.originState = -1; // I just chose -1. I don't think this matters. Will need to enforce that this state has an input though.
		node.state = -2;
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
	/// Returns true if this choice is part of a quest turn-in dialogue.
	/// </summary>
	public bool IsTurnInDialogue()
	{
		return turnInQuest;
	}

	/// <summary>
	/// Returns true if this choice is part of an in-progress dialogue.
	/// </summary>
	public bool IsInProgressDialogue()
	{
		return inProgressDialogue;
	}

	/// <summary>
	/// Calculates input/output data for this node. Also calculates turn-in and in-progress status for this node.
	/// </summary>
	public override bool Calculate()
	{
		for(int i = 0; i < Outputs.Count; i++)
		{
			if (Outputs[i].connections.Count > 0)
				Outputs[i].connections[0].connection.value = i + 1;
		}

		if (Inputs[0].connection != null && Inputs[0].connection.value != null)
		{
			originState = Inputs[0].connection.body.state;

			QuestDialogueNode temp = (QuestDialogueNode)Inputs[0].connection.body;

			if (temp.turnInQuest)
				turnInQuest = true;
			if (temp.inProgressDialogue)
				inProgressDialogue = true;

		}

		return true;
	}
}
