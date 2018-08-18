using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eppy;

/// <summary>
/// Finite State Machine for handling dialogue interactions. Instances of this class will be created by the Unity Editor Dialogue tool.
/// </summary>
public class DialogueFSM
{
	////////////////////////////////////
	// MEMBER VARIABLES
	////////////////////////////////////

	/// <summary> Hashtable for the states in this FSM. Uses integer key to lookup the state </summary> 
	private Dictionary<int, DialogueState> states;

	/// <summary>Hashtable of participants in this conversation. This will be used for knowing when we can trigger this conversation.</summary>
	private Dictionary<string, GameObject> participants;

	///<summary> Hashtable of all the transition functions in the FSM. The tuple can be read as (Current State, User Input), which returns a valid destination state </summary>
	private Dictionary<Tuple<int, int>, int> transitions;

	/// <summary> List of the required game states needed to execute this dialogue. </summary>
	private List<string> requiredGameStates;

	///<summary> keeps track of the current state we are in. </summary>
	private int currentState;

	/// <summary> Will be used for determining final state </summary>
	private int finalState;

	///<summary> keeps track of the total number of states in the FSM </summary>
	private int numberOfStates;

	/// <summary>This is the number of times this conversation can happen in the entire game.</summary>
	private int numberOfTimesConversationCanHappen;

	/// <summary>This tracks how many times this conversation has happened.</summary>
	private int numberOfTimesConversationHasOccurred;

	/// <summary>This is the probability this conversation will occur when all participants are available.</summary>
	private float probabilityOfOccurrence;

	////////////////////////////////////
	// CONSTRUCTORS
	////////////////////////////////////

	/// <summary>
	/// Default Constructor for Dialogue FSM. Initializes data. Does NOT create any states
	/// </summary>
	public DialogueFSM(int newNumberOfTimesConversationOccurs, float newProbabilityOfOccurrence)
	{
		states = new Dictionary<int, DialogueState>();
		participants = new Dictionary<string, GameObject>();
		transitions = new Dictionary<Tuple<int, int>, int>();
		requiredGameStates = new List<string>();
		currentState = 1;
		numberOfStates = 0;
		finalState = -1; // will always be -1. This will make it easy to check
		numberOfTimesConversationCanHappen = newNumberOfTimesConversationOccurs;
		probabilityOfOccurrence = newProbabilityOfOccurrence;
		numberOfTimesConversationHasOccurred = 0;

		Random.InitState((int)System.DateTime.Now.Ticks); // randomize the seed when we instantiate the state
	}


	////////////////////////////////////
	// MEMBER FUNCTIONS
	////////////////////////////////////

	
	/// <summary>
	/// Sets the starting state for this FSM.
	/// </summary>
	/// <param name="newStartState"></param>
	public void SetStartState(int newStartState)
	{
		currentState = newStartState;
	}

	/// <summary>
	/// Sets the required game states for this dialogue conversation.
	/// </summary>
	public void AddRequiredGameStates(List<string> newRequiredStates)
	{
		requiredGameStates = newRequiredStates;
	}

	/// <summary>
	/// Adds a state to the FSM.
	/// </summary>
	/// <param name="speakers">List of speakers for the state.</param>
	/// <param name="dialogue">List of dialogue for the speakers of the state.</param>
	/// <param name="description">Description of this state. Only used on player nodes coming from a choice node.</param>
	/// <param name="newBodyStyles">List of body styles for this state. Created in DialogueLoader.</param>
	/// <param name="newTailStyles">List of tail styles for this state. Created in DialogueLoader.</param>
	/// <param name="newState">The number of this state.</param>
	public void AddState(List<GameObject> speakers, List<string> dialogue, string description, List<Sprite> newBodyStyles, List<Sprite> newTailStyles, int newState)
	{
		states.Add(newState, new DialogueState(speakers, dialogue, description, newBodyStyles, newTailStyles, newState));
		numberOfStates++;
	}


	/// <summary>
	/// Adds a new quest dialogue state to the FSM.
	/// </summary>
	/// <param name="speakers">List of speakers for the state.</param>
	/// <param name="dialogue">List of dialogue for the speakers of the state.</param>
	/// <param name="description">Description of this state. Only used on player nodes coming from a choice node.</param>
	/// <param name="newBodyStyles">List of body styles for this state. Created in DialogueLoader.</param>
	/// <param name="newTailStyles">List of tail styles for this state. Created in DialogueLoader.</param>
	/// <param name="newState">The number of this state.</param>
	/// <param name="doesStateAcceptQuest">Whether or not this state accepts a quest.</param>
	/// <param name="doesStateRejectQuest">Whether or not this state rejects a quest.</param>
	public void AddQuestState(List<GameObject> speakers, List<string> dialogue, string description, List<Sprite> newBodyStyles, List<Sprite> newTailStyles, int newState, bool doesStateAcceptQuest, bool doesStateRejectQuest)
	{
		states.Add(newState, new DialogueState(speakers, dialogue, description, newBodyStyles, newTailStyles, newState, doesStateAcceptQuest, doesStateRejectQuest));
		numberOfStates++;
	}


	/// <summary>
	/// Adds a transition from startingState to destinationState. Transition occurs when transitionValue condition is met.
	/// </summary>
	/// <param name="startingState">State we are coming from</param>
	/// <param name="destinationState">State we are going to</param>
	/// <param name="transitionValue">Input required for executing this transition</param>
	public void AddTransition(int startingState, int destinationState, int transitionValue)
	{
		transitions.Add(new Tuple<int, int>(startingState, transitionValue), destinationState);
		states[startingState].transitionCount++; // increment the number of transitions from the starting state
	}


	/// <summary>
	/// Transitions from the current state in the FSM to a new state..
	/// </summary>
	/// <param name="transitionInput">The input that triggers a transition function</param>
	public void Transition(int transitionInput)
	{
		Tuple<int, int> transitionFunction = new Tuple<int, int>(currentState, transitionInput);

		// Check if transition function exists for the current state. If so, update current state
		if (transitions.ContainsKey(transitionFunction))
		{
			currentState = transitions[transitionFunction];
			
			// Check for quest acceptance and rejection.
			if(currentState != -1)
			{
				if (states[currentState].acceptQuest)
					Broadcast("AcceptQuest");
				if (states[currentState].rejectQuest)
					Broadcast("RejectQuest");
			}
		}
	}

	/// <summary>
	/// Helper function for Messenger.Broadcast. Wraps broadcast in try-catch block.
	/// </summary>
	private void Broadcast(string eventName)
	{
		try
		{
			Messenger.Broadcast(eventName);
		}
		catch (Messenger.BroadcastException)
		{

		}
	}


	/// <summary>
	/// Returns a list of valid destination states that the current state can get to. Called by DialogueManager when the current state's
	/// number of transitions is greater than 1. The returned list is sorted by user input (1, 2, 3, 4) etc.
	/// </summary>
	public Dictionary<int, DialogueState> GetDestinationStates()
	{
		Dictionary<int, DialogueState> destinations = new Dictionary<int, DialogueState>();

		foreach(KeyValuePair<Tuple<int, int>, int> transition in transitions)
			if (transition.Key.Item1 == states[currentState].stateNumber)
				destinations.Add(transition.Key.Item2, states[transition.Value]);

		return destinations;
	}


	/// <summary>
	/// Returns true if the state machine has reached the final state. Used for exiting dialogue exchange.
	/// </summary>
	public bool IsFinalState()
	{
		return currentState == finalState;
	}


	/// <summary>
	/// Sets the current state back to the start state
	/// </summary>
	public void ResetFSM()
	{
		currentState = 1;
	}


	/// <summary>
	/// Returns true if the current state has multiple transitions.
	/// </summary>
	public bool HasMultipleTransitions()
	{
		return states[currentState].transitionCount > 1;
	}


	/// <summary>
	/// Tells the current state that a character has begun printing to their dialogue box.
	/// </summary>
	/// <param name="character">The character who began printing.</param>
	public void CharacterStartedPrinting(GameObject character)
	{
		GetCurrentState().StartPrinting(character);
	}


	/// <summary>
	/// Tells the current state that a character has finished printing to their dialogue box.
	/// </summary>
	/// <param name="character">The character who stopped printing.</param>
	public void CharacterStoppedPrinting(GameObject character)
	{
		GetCurrentState().FinishPrinting(character);
	}


	/// <summary>
	/// Returns true if all characters in the state have finished printing to their dialogue boxes. Used for handling conversation flow.
	/// </summary>
	public bool IsStateFinishedPrinting()
	{
		return GetCurrentState().AreSpeakersFinishedPrinting();
	}


	/// <summary>
	/// Adds a new participant to the FSM participant hashtable.
	/// </summary>
	/// <param name="newParticipant">Participant that's being added.</param>
	public void AddParticipant(GameObject newParticipant)
	{
		if (participants.ContainsKey(newParticipant.name))
			return; // quit execution if we already have this participant in the list.

		participants.Add(newParticipant.name, newParticipant);
	}


	/// <summary>
	/// Returns the hashtable of participants for this conversation.
	/// </summary>
	public Dictionary<string, GameObject> GetParticipants()
	{
		return participants;
	}


	/// <summary>
	/// Returns the current state.
	/// </summary>
	public DialogueState GetCurrentState()
	{
		return states[currentState];
	}


	/// <summary>
	/// Determines if the characters that are in range of a character are participants in this conversation.
	/// </summary>
	/// <param name="checkParticipants">Pass charactersInRange from DialogueManager</param>
	public bool AreCharactersInRangeParticipants(Dictionary<string, GameObject> checkParticipants)
	{
		bool equal = true;

		foreach (var pair in participants)
		{
			GameObject value;
			if (checkParticipants.TryGetValue(pair.Key, out value))
			{
				// Require value be equal.
				if (value != pair.Value)
				{
					equal = false;
					break;
				}
			}
			else
			{
				// Require key be present.
				equal = false;
				break;
			}
		}

		return equal;
	}


	/// <summary>
	/// Returns true if the player is one of the participants of this FSM. Used by DialogueManager.
	/// </summary>
	public bool ContainsPlayer()
	{
		return participants.ContainsKey(GameObject.FindWithTag("Player").name);
	}


	/// <summary>
	/// Returns true if this conversation has not exceeded the number of times it can occur.
	/// </summary>
	public bool CanOccur()
	{
		// Check game states first.
		if (GameState.gameState != null && !GameState.gameState.AreAllStatesUnlocked(requiredGameStates))
			return false;

		if (numberOfTimesConversationCanHappen == -1)
			return true; // this is the case for infinite amount of times.

		return numberOfTimesConversationHasOccurred < numberOfTimesConversationCanHappen;
	}


	/// <summary>
	/// Performs a random calculation and returns true if this conversation occurs.
	/// </summary>
	public bool DoesConversationOccur()
	{
		return Random.Range(0f, 1.0f) <= probabilityOfOccurrence ? true : false;
	}


	/// <summary>
	/// Increments the number of times this conversation has happened in the game.
	/// </summary>
	public void IncrementNumberOfTimesConversationHasOccurred()
	{
		numberOfTimesConversationHasOccurred++;
	}


	/// <summary>
	/// Returns the number of states in the FSM.
	/// </summary>
	public int GetStateCount()
	{
		return numberOfStates;
	}
}
