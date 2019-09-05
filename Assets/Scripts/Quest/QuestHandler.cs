using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestHandler : MonoBehaviour
{
	/// <summary>hashtable of the current quests in the user's quest log</summary>
	private Dictionary<string, Quest> quests;  

	/// <summary>hashtable of completed quests. key is name of quest, value is the quest</summary>
	private Dictionary<string, Quest> completedQuests;

	/// <summary>hashtable that contains all the quest givers in the game</summary>
	private Dictionary<string, QuestGiver> questGivers;

	/// <summary>list of NPC's who we are in range of and who have a quest giver component.</summary>
	private List<GameObject> nearbyQuestGivers;

	private Quest questToWaitFor;

	private bool waitingForReward;

	private void OnEnable()
	{
		Messenger.AddListener("AcceptQuest", AddQuest);
		Messenger.AddListener("RejectQuest", RejectQuest);
		Messenger.AddListener("QuestComplete", CompleteQuest);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener("AcceptQuest", AddQuest);
		Messenger.RemoveListener("RejectQuest", RejectQuest);
		Messenger.RemoveListener("QuestComplete", CompleteQuest);
	}

	void Start ()
	{
		quests = new Dictionary<string, Quest>();
		completedQuests = new Dictionary<string, Quest>();
		questGivers = new Dictionary<string, QuestGiver>();
		nearbyQuestGivers = new List<GameObject>();
		waitingForReward = false;

		// Initialize all the quest givers in the game.
		foreach(GameObject questGiver in GameObject.FindGameObjectsWithTag("Character"))
		{
			QuestGiver tempQuestGiver = questGiver.GetComponent<QuestGiver>();

			if (tempQuestGiver != null)
				questGivers.Add(questGiver.name, tempQuestGiver);
		}
	}
	
	void Update ()
	{
		
	}

	/// <summary>
	/// Checks if the npc we're interacting with has a valid quest to give. Adds quest to quests hashtable if valid quest exists.
	/// </summary>
	public void StartQuestDialogue()
	{
		if (nearbyQuestGivers.Count < 1)
			return; // need at least 1 nearby NPC to begin looking if they have a quest.

		Quest tempQuest = questGivers[nearbyQuestGivers[0].name].GetQuest(); // get the quest

		if (tempQuest == null)
			return; // if the quest returns null, then we know there's no valid quest

		Debug.Log("Starting Quest Conversation");

		// activate quest dialogue at this point.
		ConversationHandler.conversationHandler.StartConversation(tempQuest.GetQuestDialogue());

		questToWaitFor = tempQuest;
	}


	/// <summary>
	/// Adds a quest to the player's inventory of quests. Called by DialogueFSM.Transition() Messenger.Broadcast.
	/// </summary>
	public void AddQuest()
	{
		Debug.Log("Adding Quest: " + questToWaitFor.GetQuestName());
		questToWaitFor.TransferOwnership(gameObject);
		questGivers[questToWaitFor.GetQuestGiver().name].RemoveQuest(questToWaitFor);
		quests.Add(questToWaitFor.GetQuestGiver().name, questToWaitFor);
		InstantiateObjectives(questToWaitFor);
	}


	/// <summary>
	/// Instantiates necessary objectives in the scene.
	/// </summary>
	private void InstantiateObjectives(Quest newQuest)
	{
		foreach (List<QuestObjective> objectivePath in newQuest.GetObjectives())
		{
			foreach (QuestObjective subObjective in objectivePath)
			{
				if (subObjective.GetType() == typeof(LocationObjective))
					Instantiate(subObjective.GetObjectiveObject(), subObjective.GetObjectiveObject().transform.position, Quaternion.identity);
			}
		}
	}

	/// <summary>
	/// Sets the quest we're waiting for to null.
	/// </summary>
	public void RejectQuest()
	{
		questToWaitFor = null;
	}

	/// <summary>
	/// Turns in a completed quest with the NPC we're interacting with.
	/// </summary>
	public void TurnInQuest()
	{
		if (nearbyQuestGivers.Count == 0)
			return;
		if (!quests.ContainsKey(nearbyQuestGivers[0].gameObject.name))
			return; // if we don't have a quest from this npc, quit function

		if (!quests[nearbyQuestGivers[0].gameObject.name].IsQuestComplete())
		{
			ConversationHandler.conversationHandler.StartConversation(quests[nearbyQuestGivers[0].gameObject.name].GetInProgressDialogue());

			return; // if the quest isn't complete, then exit function. This will need to be updated later to display a waiting dialogue.
		}

		waitingForReward = true;

		if (quests[nearbyQuestGivers[0].gameObject.name].GetTurnInDialogue() != null)
		{
			ConversationHandler.conversationHandler.StartConversation(quests[nearbyQuestGivers[0].gameObject.name].GetTurnInDialogue());
			questToWaitFor = quests[nearbyQuestGivers[0].gameObject.name];
			return;
		}

		CompleteQuest();
		
	}


	/// <summary>
	/// Completes the quest and disburses the reward. Called on by DialogueManager.EndConversation()
	/// </summary>
	private void CompleteQuest()
	{
		if (!waitingForReward)
			return;

		// if we get here, we know the quest is complete.
		Debug.Log("Quest Complete " + quests[nearbyQuestGivers[0].gameObject.name].GetQuestName());

		// add the quest to our list of completed quests
		completedQuests.Add(quests[nearbyQuestGivers[0].gameObject.name].GetQuestName(), quests[nearbyQuestGivers[0].gameObject.name]);

		// remove the quest from the quests hashtable since it is now done.
		RemoveQuest(nearbyQuestGivers[0].gameObject.GetComponent<QuestGiver>());

		waitingForReward = false;
	}

	/// <summary>
	/// Returns a hashtable containing the completed quests.
	/// </summary>
	public Dictionary<string, Quest> GetCompletedQuests()
	{
		return completedQuests;
	}


	/// <summary>
	/// Removes a quest from the list of current quests.
	/// </summary>
	/// <param name="questGiver">QuestGiver component from the NPC who we have turned in the quest to.</param>
	private void RemoveQuest(QuestGiver questGiver)
	{
		quests.Remove(questGiver.gameObject.name);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!questGivers.ContainsKey(collision.gameObject.name))
			return; // if the NPC does not have a quest giver component, exit the function.

		nearbyQuestGivers.Add(collision.gameObject);
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if(nearbyQuestGivers.Contains(collision.gameObject))
			nearbyQuestGivers.Remove(collision.gameObject);
	}
}
