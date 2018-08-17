using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
	[SerializeField]
	/// <summary>The name of the quest</summary>
	private string questName;

	[SerializeField]
	private GameObject questGiver;

	/// <summary>List of objectives that must be completed for this quest.</summary>
	private List<List<QuestObjective>> objectives;

	/// <summary> List of required game states that must be true for this quest to be available.</summary>
	private List<string> requiredGameStates;

	/// <summary>Hashtable containing the quests that must be completed before the player can start this quest. Key is the name of the quest.</summary>
	private List<string> requiredQuests;

	/// <summary>The dialogueFSM associated with this quest.</summary>
	private DialogueFSM questDialogue;

	private DialogueFSM turnInDialogue;

	private DialogueFSM inProgressDialogue;


	/// <summary>
	/// Constructor for Quest Loading in the QuestLoader script.
	/// </summary>
	public Quest(QuestNodeCanvas questCanvas)
	{
		questName = questCanvas.newQuest.questName;
		questGiver = questCanvas.newQuest.questGiver;

		objectives = new List<List<QuestObjective>>();
		questDialogue = null;
		turnInDialogue = null;

		// Parse concurrent objectives into our quest. Grab Objective Paths first and then sub objectives.
		foreach(ObjectivePath path in questCanvas.objectivePaths)
		{
			objectives.Add(new List<QuestObjective>());

			foreach(SubObjective subObjective in path.objectives)
			{
				if (subObjective.objective.tag == "Enemy")
					objectives[objectives.Count - 1].Add(new EnemyObjective(subObjective.objective.GetComponent<Enemy>(), subObjective.numberToCollect, questCanvas.newQuest.questGiver));
				else if(subObjective.objective.tag == "Location")
					objectives[objectives.Count - 1].Add(new LocationObjective(subObjective.objective));
				//else if(subObjective.objective.tag == "Item")
				//	objectives[objectives.Count - 1].Add(new ItemObjective(subObjective.objective.GetComponent<Item>()));
			}
		}

		// Set the first objective in each path to active.
		for (int i = 0; i < objectives.Count; i++)
			objectives[i][0].SetActiveObjective();

		// set up the linked list of next objectives
		foreach(List<QuestObjective> objectivePath in objectives)
		{
			for(int i = 0; i < objectivePath.Count - 1; i++)
				objectivePath[i].SetNextObjective(objectivePath[i + 1]);
		}

		requiredQuests = new List<string>();

		foreach (QuestAsset quest in questCanvas.requiredQuests)
			requiredQuests.Add(quest.questName);

		// Parse required game states
		requiredGameStates = new List<string>();

		foreach (GameStateName stateName in questCanvas.requiredGameStates)
			requiredGameStates.Add(stateName.eventName);
	}

	
	/// <summary>
	/// Returns true if all objectives are complete.
	/// </summary>
	public bool IsQuestComplete()
	{
		// When objective is complete, pop it from collection. Check container sizes instead of iterating over data.

		foreach (List<QuestObjective> objectivePath in objectives)
		{
			foreach (QuestObjective objective in objectivePath)
			{
				if (!objective.IsComplete())
					return false; // if an objective is not complete, return false, kill function.
			}
		}

		return true; // We only get here if we've not come across an incomplete objective. So, return true.
	}


	/// <summary>
	/// Tells this quest's objectives who the new owner is. This is used for determining when to update objectives.
	/// </summary>
	public void TransferOwnership(GameObject newOwner)
	{
		foreach (List<QuestObjective> objectivePath in objectives)
		{
			foreach (QuestObjective subObjective in objectivePath)
				subObjective.TransferOwner(newOwner);
		}
	}

	/// <summary>
	/// Returns true if the prerequisites for this quest have been completed.
	/// </summary>
	public bool CheckPrerequisites()
	{
		QuestHandler playerQuestHandler = GameObject.FindWithTag("Player").GetComponent<QuestHandler>(); // get quest handler

		// Check the game state
		if (!GameState.gameState.AreAllStatesUnlocked(requiredGameStates))
			return false;

		// iterate over all the required quests and determine if the player has completed them.
		foreach(string requiredQuest in requiredQuests)
		{
			if (!playerQuestHandler.GetCompletedQuests().ContainsKey(requiredQuest))
				return false;
		}

		return true;
	}


	/// <summary>
	/// Adds a Dialogue exchange to this quest.
	/// </summary>
	/// <param name="newDialogue">The dialogue exchange to add.</param>
	public void AddDialogue(DialogueFSM newDialogue)
	{
		questDialogue = newDialogue;
	}


	/// <summary>
	/// Adds a turn-in dialogue to this quest.
	/// </summary>
	/// <param name="newDialogue">The dialogue FSM to add.</param>
	public void AddTurnInDialogue(DialogueFSM newDialogue)
	{
		turnInDialogue = newDialogue;
	}


	/// <summary>
	/// Adds an in-progress dialogue to this quest.
	/// </summary>
	/// <param name="newDialogue">The dialogue FSM to add.</param>
	public void AddInProgressDialogue(DialogueFSM newDialogue)
	{
		inProgressDialogue = newDialogue;
	}

	/// <summary>
	/// Returns this quests' dialogue exchange.
	/// </summary>
	public DialogueFSM GetQuestDialogue()
	{
		return questDialogue;
	}

	public DialogueFSM GetTurnInDialogue()
	{
		return turnInDialogue;
	}

	public DialogueFSM GetInProgressDialogue()
	{
		return inProgressDialogue;
	}

	/// <summary>
	/// Returns the name of this quest.
	/// </summary>
	public string GetQuestName()
	{
		return questName;
	}

	/// <summary>
	/// Returns the Game Object of the character who gives this quest.
	/// </summary>
	public GameObject GetQuestGiver()
	{
		return questGiver;
	}

	public List<List<QuestObjective>> GetObjectives()
	{
		return objectives;
	}

	public List<string> GetRequiredQuests()
	{
		return requiredQuests;
	}
}
