using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestAsset : ScriptableObject
{
	/// <summary>The name of the quest</summary>
	public string questName;

	public GameObject questGiver;

	/// <summary>List of objectives that must be completed for this quest.</summary>
	public List<QuestObjective> objectives;

	/// <summary>Hashtable containing the quests that must be completed before the player can start this quest. Key is the name of the quest.</summary>
	public Dictionary<string, QuestAsset> requiredQuests;

	/// <summary>The dialogueFSM associated with this quest.</summary>
	public DialogueFSM questDialogue;


	/// <summary>
	/// Initializes all the quest data for this quest.
	/// </summary>
	/// <param name="newQuestName">The name of the quest.</param>
	/// <param name="newQuestGiver">The game object of the character who gives this quest.</param>
	/// <param name="newObjectives">The objectives for this quest.</param>
	/// <param name="newRequiredQuests">The prerequisite quests needed to unlock this quest.</param>
	/// <param name="newQuestDialogue">The dialogue exchange associated with this quest.</param>
	public void InitializeData(string newQuestName, GameObject newQuestGiver, List<QuestObjective> newObjectives, Dictionary<string, QuestAsset> newRequiredQuests, DialogueFSM newQuestDialogue)
	{
		questName = newQuestName;
		questGiver = newQuestGiver;
		objectives = newObjectives;
		requiredQuests = newRequiredQuests;
		questDialogue = newQuestDialogue;
	}

}
