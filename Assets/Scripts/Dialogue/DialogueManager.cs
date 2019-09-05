using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class must be attached to any character prefab that will be able to speak in the game. It communicates with the Conversation Handler and detects nearby npc's for communication.
/// </summary>
public class DialogueManager : MonoBehaviour
{
	// This list will only contain FSM's whose starting participant is this character
	private List<DialogueFSM> dialogueTrees;

	//private List<DialogueFSM> questDialogueTrees;

	/// <summary>Tracks the characters who are in range of this character</summary>
	private Dictionary<string, GameObject> charactersInRange;

	// Flag for knowing if conversation is active
	private bool playerConversationActive, conversationActive;

	// This will keep track of user dialogue selection
	private int userDialogueSelection;

	// Tracks the ID of the conversation we are currently in.
	private int currentConversationID;

	private DialogueBox dialogueBox;

	private void Awake()
	{
		Initialize();
	}

	void Start()
	{
	}
	
	void Update ()
	{
		
	}


	/// <summary>
	/// Initializes lists and hashtables and adds necessary data to them. Initializes booleans and integers to default values.
	/// </summary>
	private void Initialize()
	{
		dialogueTrees = new List<DialogueFSM>();
		//questDialogueTrees = new List<DialogueFSM>();
		charactersInRange = new Dictionary<string, GameObject>();

		charactersInRange.Add(gameObject.name, gameObject); // add this character by default

		conversationActive = false;
		playerConversationActive = false;

		// initialize values to -1 for safety purposes. Don't think it's necessary but doing it anyway.
		currentConversationID = -1;
		userDialogueSelection = -1;
	}


	/// <summary>
	/// Locks the character into a conversation with other NPC's. These conversations can be interrupted by the player.
	/// </summary>
	public void StartConversation(int newConversationID)
	{
		conversationActive = true;
		currentConversationID = newConversationID;
		InitializeDialogueBox();
	}


	/// <summary>
	/// Locks this character into a conversation with the player.
	/// </summary>
	public void StartPlayerConversation(int newConversationID)
	{
		// Interrupt and NPC conversation if it's occurring.
		if(conversationActive)
			ConversationHandler.conversationHandler.CancelConversation(currentConversationID);
			
		// Lock the player's movement.
		if (gameObject.tag == "Player")
			Messenger.Broadcast("LockPlayerMovement");

		playerConversationActive = true;
		currentConversationID = newConversationID;
		InitializeDialogueBox();
	}


	/// <summary>
	/// Allows this character to talk to other characters again and closes the character's dialoge box.
	/// </summary>
	public void EndConversation()
	{
		conversationActive = false;
		playerConversationActive = false;
		currentConversationID = -1;

		DestroyDialogueBox();

		if (gameObject.tag == "Player")
		{
			try {
				Messenger.Broadcast("UnlockPlayerMovement");
				Messenger.Broadcast("QuestComplete");
			}
			catch(Messenger.BroadcastException) {

			}
			
		}
	}


	/// <summary>
	/// Returns true if this character is not currently engaged in a conversation
	/// </summary>
	public bool CanTalk()
	{
		// At some point, may need to extend this functionality so that player cannot talk to character if they are in a fight.
		return !conversationActive && !playerConversationActive;
	}


	/// <summary>
	/// Prints dialogue and waits for user input to proceed.
	/// </summary>
	/// <param name="currentState">The current state of the FSM.</param>
	public void PrintDialogueAndWaitForInput(string dialogue, Sprite bodyStyle, Sprite tailStyle)
	{
		PrintToDialogueBox(dialogue, bodyStyle, tailStyle);

		//StartCoroutine(GetNextLine());
		// Tell conversation handler to wait for input, if it isn't already. This will help avoid race conditions when multiple speakers exist in a state.
		ConversationHandler.conversationHandler.WaitingForPlayerInput(currentConversationID);
	}

	/// <summary>
	/// Print dialogue to a text box and then print the responding choices afterwards.
	/// </summary>
	/// <param name="dialogue">Dialogue to print initially.</param>
	/// <param name="bodyStyle">Dialolgue Box Body Style for the dialogue.</param>
	/// <param name="tailStyle">Dialogue box tail style for the dialogue.</param>
	public IEnumerator PrintDialogueWithChoices(string dialogue, Sprite bodyStyle, Sprite tailStyle)
	{
		// Print the speaker's dialogue
		PrintToDialogueBox(dialogue, bodyStyle, tailStyle);

		ConversationHandler.conversationHandler.CharacterStartedPrinting(currentConversationID, gameObject);

		// Wait for first dialogue box to finish printing.
		while (!dialogueBox.IsPrintingFinished())
			yield return null;

		while(!Input.GetButtonDown("NextLine"))
			yield return null;

		// Once dialogue box is done printing, open the player's dialogue box and show dialogue options
		ConversationHandler.conversationHandler.CharacterStoppedPrinting(currentConversationID, gameObject);

		// Once the player dialogue box has opened, allow choice selection.
		ConversationHandler.conversationHandler.WaitingForChoiceSelect(currentConversationID);
	}


	/// <summary>
	/// Print dialogue and do not wait for input. Used for NPC conversations.
	/// </summary>
	/// <param name="dialogue">Dialogue to print.</param>
	/// <param name="bodyStyle">Body style for dialogue box.</param>
	/// <param name="tailStyle">Tail style for dialogue box.</param>
	public void PrintDialogueWithNoInput(string dialogue, Sprite bodyStyle, Sprite tailStyle)
	{
		PrintToDialogueBox(dialogue, bodyStyle, tailStyle);

		StartCoroutine(NPCConversation());
	}


	/// <summary>
	/// Sends dialogue choices to dialogue box for printing.
	/// </summary>
	/// <param name="currentState"></param>
	/// <param name="transitionStates"></param>
	public void PrintChoices(Dictionary<int, DialogueState> transitionStates)
	{
		dialogueBox.gameObject.SetActive(true);

		if (!dialogueBox.IsPrintingFinished())
			return; // kill the function if we're already printing.

		dialogueBox.PrintChoices(transitionStates);
	}

	/// <summary>
	/// Prints NPC conversations and waits a set amount of time between dialogues.
	/// </summary>
	private IEnumerator NPCConversation()
	{
		ConversationHandler.conversationHandler.CharacterStartedPrinting(currentConversationID, gameObject);

		while (conversationActive && !dialogueBox.IsPrintingFinished())
			yield return null;

		// Note that the conversation can be interrupted by the player, which will cause the active conversation to be destroyed. We need to safety check to ensure we still have a valid conversation going.

		if (!conversationActive)
			yield break; // safety check.

		yield return new WaitForSeconds(2.0f); // wait for 5 seconds before reading in next line

		if (!conversationActive)
			yield break; // safety check.

		ConversationHandler.conversationHandler.CharacterStoppedPrinting(currentConversationID, gameObject);

		if (ConversationHandler.conversationHandler.IsStateFinishedPrinting(currentConversationID))
			// if state has finished printing all dialogue boxes, transition to next state
			ConversationHandler.conversationHandler.TransitionConversation(currentConversationID, 0);
	}


	/// <summary>
	/// Adds a new character to the hashtable of characters who are in range of this character.
	/// </summary>
	/// <param name="newCharacter">Character being added to hashtable.</param>
	private void AddInRangeCharacter(GameObject newCharacter)
	{
		if(!charactersInRange.ContainsValue(newCharacter))
			charactersInRange.Add(newCharacter.name, newCharacter);
	}


	/// <summary>
	/// Removes a character from the hashtable of characters that are in range of this character.
	/// </summary>
	/// <param name="removedCharacter">Character being removed from hashtable.</param>
	private void RemoveInRangeCharacter(GameObject removedCharacter)
	{
		if (charactersInRange.ContainsValue(removedCharacter))
			charactersInRange.Remove(removedCharacter.name);
	}


	/// <summary>
	/// Adds a new FSM to the list of FSM's for this character. Called on by DialogueLoader.
	/// </summary>
	/// <param name="newFSM">FSM that is being added.</param>
	public void AddFSM(DialogueFSM newFSM)
	{
		dialogueTrees.Add(newFSM);
	}


	///// <summary>
	///// Adds a new quest dialogue exchange to the list of quest dialogues for this character. Called by DialogueLoader.
	///// </summary>
	///// <param name="newFSM"></param>
	//public void AddQuestDialogueFSM(DialogueFSM newFSM)
	//{
	//	questDialogueTrees.Add(newFSM);
	//}


	/// <summary>
	/// Finds if a conversation for the nearby participants and activates it if it exists.
	/// </summary>
	public void CheckForAndActivateConversation()
	{
		if (charactersInRange.Count < 1)
			return; // we need to have at least 1 nearby npc to attempt to talk to them.
		
		foreach (DialogueFSM fsm in dialogueTrees)
		{
			if (fsm.AreCharactersInRangeParticipants(charactersInRange))
			{
				ConversationHandler.conversationHandler.StartConversation(fsm);

				if(!CanTalk())
					break; // only break if the conversation activated. If it didn't then that means our probability didn't hit. So, check the rest of the FSM's and see if we can activate them.
			}
		}
	}


	/// <summary>
	/// Sets up a dialogue box above this character's head with the proper style.
	/// </summary>
	private void InitializeDialogueBox()
	{
		Transform boxPosition = transform.Find("DialoguePosition");

		// Instantiate a dialogue box and assign the DialogueBox script from the new spawn to the variable.
		dialogueBox = ((GameObject)Instantiate(Resources.Load("Prefabs/Dialogue/DialogueBox"), GameObject.FindGameObjectWithTag("DialogueCanvas").transform)).GetComponent<DialogueBox>();

		// All we need to do at first is set the owner and then disable the dialogue box until we need to print text.
		dialogueBox.SetOwner(boxPosition);
		dialogueBox.gameObject.SetActive(false);
	}


	/// <summary>
	/// Sets the style of the dialogue box, enables it, then prints the dialogue to the box.
	/// </summary>
	/// <param name="newDialogue">The dialogue to be printed.</param>
	/// <param name="dialogueBodyStyle">The style of the body.</param>
	/// <param name="dialogueTailStyle">The style of the tail.</param>
	private void PrintToDialogueBox(string newDialogue, Sprite dialogueBodyStyle, Sprite dialogueTailStyle)
	{
		dialogueBox.SetStyle(dialogueBodyStyle, dialogueTailStyle);
		dialogueBox.gameObject.SetActive(true);
		dialogueBox.PrintText(newDialogue);
	}


	/// <summary>
	/// Removes the dialogue box object associated with this character from the game and memory.
	/// </summary>
	private void DestroyDialogueBox()
	{
		Destroy(dialogueBox.gameObject);
	}


	private void OnTriggerEnter2D(Collider2D collision)
	{
		// Add characters to proximity list
		if (collision.tag == "Character" || collision.tag == "Player")
		{
			AddInRangeCharacter(collision.gameObject);

			// If this character is NOT the player, then see if a valid conversation is available and execute it if so.
			if (gameObject.tag != "Player")
				CheckForAndActivateConversation();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Character" || collision.tag == "Player")
			RemoveInRangeCharacter(collision.gameObject);
	}
}
