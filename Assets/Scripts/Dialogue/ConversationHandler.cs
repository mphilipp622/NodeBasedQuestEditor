using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConversationHandler : MonoBehaviour
{
	/// <summary>Static Getter for the Conversation Handler.</summary>
	public static ConversationHandler conversationHandler;

	/// <summary>This is a hashtable that contains all the DialogueManagers in the game.</summary>
	private Dictionary<GameObject, DialogueManager> dialogueManagers;

	/// <summary>This hashtable will work sort of like a ticket system. When a new conversation is created, an integer value is assigned to that conversation. That value is then handed out to every participant who then checks it in and out with the ConversationHandler.</summary>
	private Dictionary<int, DialogueFSM> activeConversations;

	private bool waitingForPlayerInput, waitingForChoiceSelect;

	void Awake()
	{
		InitSingleton();
		InitializeDialogueManagers();

		activeConversations = new Dictionary<int, DialogueFSM>();
		waitingForPlayerInput = false;
	}

	void Start ()
	{
		
	}
	
	void Update ()
	{
		
	}


	/// <summary>
	/// Implements the singleton pattern on this class. Only a single instance of this class will exist in the whole game and it can be accessed by anyone.
	/// </summary>
	void InitSingleton()
	{
		if (conversationHandler == null)
			conversationHandler = this;
		else if (conversationHandler != this)
			Destroy(gameObject);
	}


	/// <summary>Initializes the dialogueManagers hashtable.</summary>
	void InitializeDialogueManagers()
	{
		// create instance of hashtable.
		dialogueManagers = new Dictionary<GameObject, DialogueManager>();

		// Iterate over every character game object in the game and add their dialogue manager if they have one
		foreach(GameObject character in GameObject.FindGameObjectsWithTag("Character"))
		{
			DialogueManager tempManager = character.GetComponent<DialogueManager>();

			if (tempManager) // if this character has a dialogue manager, add it.
				dialogueManagers.Add(character, tempManager);
		}

		// add player to the hashtable.
		dialogueManagers.Add(GameObject.FindWithTag("Player"), GameObject.FindWithTag("Player").GetComponent<DialogueManager>());
	}


	/// <summary>
	/// Starts a new conversation.
	/// </summary>
	/// <param name="conversation">The conversation that is being started.</param>
	/// <param name="participants">The participants who take part in the conversation.</param>
	public void StartConversation(DialogueFSM conversation)
	{
		// First, check if this state has exceeded the number of times it can occur
		if (!conversation.CanOccur())
			return;
		
		// Next, check the probability and randomize to see if the conversation occurs.
		if (!conversation.DoesConversationOccur())
			return;

		// next, we need to iterate over the participants and make sure all of them are available for a conversation.
		// Note that each character will check if the necessary participants are in range of them. This means that this
		// function will only ever be called from a character who is in close proximity to each participant.
		if(!conversation.ContainsPlayer())
		{
			// Note that the player can interrupt a conversation to start a new one. NPC's, however, must go through this process first.
			foreach (KeyValuePair<string, GameObject> participant in conversation.GetParticipants())
			{
				if (!dialogueManagers[participant.Value].CanTalk()) // if a participant is not available to talk, exit the function.
					return;
			}
		}
		else if (conversation.ContainsPlayer() && !dialogueManagers[GameObject.FindWithTag("Player")].CanTalk())
			return;

		// If we get here, then we've confirmed all participants are available. Therefore, we can start the conversation.
		// Add a new conversation to the hashtable of active conversations.
		int newID = GenerateConversationID(); // Custom key is created for the conversation.
		activeConversations.Add(newID, conversation);

		// Now, we need to lock each participant out of other conversations.
		if (conversation.ContainsPlayer())
		{
			// Special case when the player is involved in the conversation.
			foreach (KeyValuePair<string, GameObject> participant in conversation.GetParticipants())
				dialogueManagers[participant.Value].StartPlayerConversation(newID);
				
		}
		else
		{
			foreach (KeyValuePair<string, GameObject> participant in conversation.GetParticipants())
				dialogueManagers[participant.Value].StartConversation(newID);
		}

		// Now we can execute our first print statement. If this state has a choice following it, we want to execute a different function to allow user choice.
		if (conversation.HasMultipleTransitions())
			StartCoroutine(PrintDialogueWithChoices(newID));
		else
			PrintDialogue(newID);
	}


	/// <summary>
	/// Ends all the participant's involvement in the conversation and removes the conversation from the list of active conversations.
	/// </summary>
	/// <param name="conversationID">ID of the conversation you wish to cancel.</param>
	public void CancelConversation(int conversationID)
	{
		// iterate over all participants of the conversation and end their conversation.
		foreach (KeyValuePair<string, GameObject> participant in activeConversations[conversationID].GetParticipants())
			dialogueManagers[participant.Value].EndConversation();

		DestroyConversation(conversationID);
	}


	/// <summary>
	/// Tells the speakers for the current state to display some dialogue.
	/// </summary>
	/// <param name="conversationID">The FSM's ID #</param>
	private void PrintDialogue(int conversationID)
	{
		// If the player is part of the conversation, we want to execute a function that waits for user input to proceed.
		if (activeConversations[conversationID].ContainsPlayer())
		{
			DialogueState newState = activeConversations[conversationID].GetCurrentState();

			for (int i = 0; i < activeConversations[conversationID].GetCurrentState().speakers.Count; i++)
				dialogueManagers[newState.speakers[i]].PrintDialogueAndWaitForInput(newState.dialogues[i], newState.bodyStyles[i], newState.tailStyles[i]);
		}
		else
		{
			// If the player is not part of the conversation, then we can start an automated conversation.
			DialogueState newState = activeConversations[conversationID].GetCurrentState();

			for (int i = 0; i < activeConversations[conversationID].GetCurrentState().speakers.Count; i++)
				dialogueManagers[newState.speakers[i]].PrintDialogueWithNoInput(newState.dialogues[i], newState.bodyStyles[i], newState.tailStyles[i]);
		}
	}


	/// <summary>
	/// Prints dialogue to a box followed by the choices, which show up in the player's dialogue box.
	/// </summary>
	/// <param name="conversationID">ID of the conversation you're referencing.</param>
	private IEnumerator PrintDialogueWithChoices(int conversationID)
	{
		for (int i = 0; i < activeConversations[conversationID].GetCurrentState().speakers.Count; i++)
			StartCoroutine(dialogueManagers[activeConversations[conversationID].GetCurrentState().speakers[i]].PrintDialogueWithChoices(activeConversations[conversationID].GetCurrentState().dialogues[i], activeConversations[conversationID].GetCurrentState().bodyStyles[i], activeConversations[conversationID].GetCurrentState().tailStyles[i]));

		while (!activeConversations[conversationID].IsStateFinishedPrinting())
			yield return null;

		PrintDialogueChoicesToDialogueBox(conversationID);
	}


	/// <summary>
	/// Sends transition states to the player DialogueManager for printing.
	/// </summary>
	public void PrintDialogueChoicesToDialogueBox(int conversationID)
	{
		dialogueManagers[GameObject.FindGameObjectWithTag("Player")].PrintChoices(activeConversations[conversationID].GetDestinationStates());
	}


	/// <summary>
	/// Transitions the conversation to the next state. Also checks if conversation has ended and will update participants if so.
	/// </summary>
	/// <param name="conversationID">The id of the conversation the character is participating in.</param>
	/// <param name="transitionValue">The input value that determines which state to transition to.</param>
	public void TransitionConversation(int conversationID, int transitionValue)
	{
		// Tell CM to stop waiting for inputs.
		waitingForPlayerInput = false;
		waitingForChoiceSelect = false;

		if (!activeConversations.ContainsKey(conversationID))
			return; // if the conversation no longer exists, exit the function.
		
		activeConversations[conversationID].Transition(transitionValue);
		
		// check to see if we've ended the conversation.
		if (activeConversations[conversationID].IsFinalState())
		{
			activeConversations[conversationID].IncrementNumberOfTimesConversationHasOccurred();
			DestroyConversation(conversationID);
			return;
		}

		// If this new state has choices following it, execute a different function.
		if (activeConversations[conversationID].HasMultipleTransitions())
			StartCoroutine(PrintDialogueWithChoices(conversationID));
		else
			PrintDialogue(conversationID);
			
	}


	/// <summary>
	/// Used to end a conversation. Tells all participants the conversation is over and resets the FSM.
	/// </summary>
	/// <param name="conversationID">The ID for the conversation we are ending</param>
	private void DestroyConversation(int conversationID)
	{
		// If conversation has ended, we want to end the conversation for all participants and reset FSM.
		foreach (KeyValuePair<string, GameObject> character in activeConversations[conversationID].GetParticipants())
			dialogueManagers[character.Value].EndConversation();

		activeConversations[conversationID].ResetFSM(); // reset the FSM
		activeConversations.Remove(conversationID); // remove the conversation from the active conversations.
	}


	/// <summary>
	/// Tells current state that a character has started printing.
	/// </summary>
	/// <param name="conversationID"></param>
	/// <param name="character">Character that began printing.</param>
	public void CharacterStartedPrinting(int conversationID, GameObject character)
	{
		activeConversations[conversationID].CharacterStartedPrinting(character);
	}


	/// <summary>
	/// Tells the current state that a character has finished printing. Will transition if the character is the last one we're waiting for.
	/// </summary>
	/// <param name="conversationID"></param>
	/// <param name="character">The character who finished printing.</param>
	public void CharacterStoppedPrinting(int conversationID, GameObject character)
	{
		activeConversations[conversationID].CharacterStoppedPrinting(character);
	}


	/// <summary>
	/// Returns if all current dialogues have finished printing.
	/// </summary>
	/// <param name="conversationID">ID for the conversation you're checking.</param>
	public bool IsStateFinishedPrinting(int conversationID)
	{
		return activeConversations[conversationID].IsStateFinishedPrinting();
	}


	/// <summary>
	/// Creates a unique key for a conversation.
	/// </summary>
	private int GenerateConversationID()
	{
		int newKey = (int) (Random.Range(0, 10000));

		while(activeConversations.ContainsKey(newKey))
			newKey = (int)(Random.Range(0, 10000)); // re-roll the key until we get a unique ID that does not already exist.

		return newKey;
	}

	/// <summary>
	/// Parses user input during dialogue conversations. Will then transition the state machine for the dialogue.
	/// </summary>
	public IEnumerator GetNextLine(int conversationID)
	{
		int userDialogueSelection = -1; // reset input to -1

		while (userDialogueSelection == -1)
		{
			if (Input.GetButtonDown("NextLine"))
				userDialogueSelection = 0;

			yield return null;
		}

		TransitionConversation(conversationID, userDialogueSelection);
	}

	/// <summary>
	/// Tells Conversation Handler to wait for player input. This helps avoid race conditions when multiple speakers
	/// exist in a single state.
	/// </summary>
	public void WaitingForPlayerInput(int conversationID)
	{
		if (waitingForPlayerInput)
			return;
		else
		{
			waitingForPlayerInput = true;
			StartCoroutine(GetNextLine(conversationID));
		}

		
	}

	/// <summary>
	/// Waits for user input for dialogue choices then tells Conversation Handler what option was selected.
	/// </summary>
	private IEnumerator GetChoiceSelection(int conversationID)
	{
		int userDialogueSelection = -1; // reset input to -1

		while (userDialogueSelection == -1)
		{
			if (Input.GetButtonDown("DialogueSelect1"))
				userDialogueSelection = 1;
			else if (Input.GetButtonDown("DialogueSelect2"))
				userDialogueSelection = 2;
			else if (Input.GetButtonDown("DialogueSelect3"))
				userDialogueSelection = 3;
			else if (Input.GetButtonDown("DialogueSelect4"))
				userDialogueSelection = 4;
			else if (Input.GetButtonDown("DialogueSelect5"))
				userDialogueSelection = 5;
			else if (Input.GetButtonDown("DialogueSelect6"))
				userDialogueSelection = 6;
			else if (Input.GetButtonDown("DialogueSelect7"))
				userDialogueSelection = 7;
			else if (Input.GetButtonDown("DialogueSelect8"))
				userDialogueSelection = 8;
			else if (Input.GetButtonDown("DialogueSelect9"))
				userDialogueSelection = 9;

			yield return null;
		}

		TransitionConversation(conversationID, userDialogueSelection);

		waitingForChoiceSelect = false;
	}

	public void WaitingForChoiceSelect(int conversationID)
	{
		if (waitingForChoiceSelect)
			return;
		else
		{
			waitingForChoiceSelect = true;
			StartCoroutine(GetChoiceSelection(conversationID));
		}
	}
}
