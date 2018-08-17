using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{

	/// <summary>hashtable containing all the quests this NPC has available.</summary>
	private Dictionary<string, Quest> availableQuests;

	private void OnEnable()
	{
		Messenger.AddListener("QuestLoadingDone", CheckQuestCount);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener("QuestLoadingDone", CheckQuestCount);
	}

	private void Awake()
	{
		availableQuests = new Dictionary<string, Quest>();
	}

	void Start ()
	{

		
		
	}
	
	void Update ()
	{
		
	}


	/// <summary>
	/// Adds a new quest to the quest giver's pool of available quests.
	/// </summary>
	/// <param name="newQuest">The quest to add to the giver's pool.</param>
	public void AddQuest(Quest newQuest)
	{
		if(availableQuests == null)
			availableQuests = new Dictionary<string, Quest>();

		availableQuests.Add(newQuest.GetQuestName(), newQuest);
	}

	/// <summary>
	/// Returns a valid quest.
	/// </summary>
	public Quest GetQuest()
	{
		Quest returnQuest; //

		//Debug.Log("GETTING QUEST");

		// iterate over all the available quests on this NPC.
		foreach (KeyValuePair<string, Quest> quest in availableQuests)
		{
			if (quest.Value.CheckPrerequisites())
			{
				// If the quest meets the prereqs, return it and kill the function.

				returnQuest = quest.Value;
				//RemoveQuest(quest.Value); // take quest out of the NPC's list of available quests.
				return returnQuest;
			}
		}

		return null;
		
	}

	/// <summary>
	/// Returns true if this NPC has a valid quest. Checks against prerequisites and game events.
	/// </summary>
	public bool HasValidQuestAvailable()
	{
		// Final version will need to look at game state and determine if there's a valid quest
		// if(availableQuests.ContainsKey(GameState.currentGameState)) return availableQuests[GameState.currentGameState]

		if (availableQuests.Count < 1)
			return false;

		foreach (KeyValuePair<string, Quest> quest in availableQuests)
		{
			if (quest.Value.CheckPrerequisites())
				return true;
		}

		return false;
	}


	/// <summary>
	/// Removes quest from the NPC's list of available quests.
	/// </summary>
	/// <param name="questToRemove">Quest that will be removed.</param>
	public void RemoveQuest(Quest questToRemove)
	{
		availableQuests.Remove(questToRemove.GetQuestName());
	}


	/// <summary>
	/// Checks the number of quests this giver has. If the number is 0, remove the component.
	/// </summary>
	void CheckQuestCount()
	{
		if (availableQuests.Count == 0)
			Destroy(this);
	}
}