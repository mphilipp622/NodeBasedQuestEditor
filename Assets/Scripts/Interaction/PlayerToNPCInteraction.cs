using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToNPCInteraction : MonoBehaviour
{
	[SerializeField]
	private DialogueManager playerDialogueManager;

	[SerializeField]
	private QuestHandler playerQuestHandler;

	void Start ()
	{
		InitializeScripts();	
	}
	
	void Update ()
	{
		if (Input.GetButtonDown("InteractDialogue"))
			// Player input to interact with an NPC
			DialogueInteraction();
		else if (Input.GetButtonDown("InteractQuest"))
			QuestInteraction();
		else if (Input.GetButtonDown("InteractTurnInQuest"))
			TurnInQuest();
	}

	/// <summary>
	/// Initialize the dialogue manager and quest handler on the player
	/// </summary>
	void InitializeScripts()
	{
		if (!playerDialogueManager)
			playerDialogueManager = GetComponent<DialogueManager>();
		if (!playerQuestHandler)
			playerQuestHandler = GetComponent<QuestHandler>();

		if (!playerDialogueManager)
			Debug.LogError("ERROR: Player must contain DialogueManager script");
		if (!playerQuestHandler)
			Debug.LogError("ERROR: PLayer must contain QuestHandler script");
	}

	/// <summary>
	/// Will attempt to perform a dialogue interaction with an NPC
	/// </summary>
	private void DialogueInteraction()
	{
		playerDialogueManager.CheckForAndActivateConversation();
	}

	/// <summary>
	/// Will attempt to get a quest from an NPC
	/// </summary>
	private void QuestInteraction()
	{
		playerQuestHandler.StartQuestDialogue();
	}

	/// <summary>
	/// Will attempt to turn in a quest.
	/// </summary>
	private void TurnInQuest()
	{
		playerQuestHandler.TurnInQuest();
	}
}
