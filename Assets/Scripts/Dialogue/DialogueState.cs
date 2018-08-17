using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueState
{
	///<summary> This is the character who speaks this line of dialogue </summary>
	private List<GameObject> _speakers;

	///<summary> This is the line the character speaks</summary>
	private List<string> _dialogues;

	/// <summary>This is a short description of this dialogue. Will be displayed as dialogue options when multiple choices are available.</summary>
	private string _description;

	///<summary> This is the style associated with this dialogue. Will be used by the Dialogue Box for the body.</summary>
	private List<Sprite> _bodyStyles;

	/// <summary> Tail style associated with this dialogue. Will be used on Dialogue Box to display the tail.</summary>
	private List<Sprite> _tailStyles;

	/// <summary>the integer value that indicates which state this is. Primarily used for connecting transition functions</summary>
	private int _stateNumber;

	/// <summary> Keeps track of the number of transitions from this state</summary>
	private int _transitionCount;

	/// <summary>Keeps track of the printing status of the speakers in this state.</summary>
	private Dictionary<GameObject, bool> printStatus;

	/// <summary>Specifies if this state is an accept state for a quest.</summary>
	private bool _acceptQuest;

	/// <summary>Specifies if this state is a reject state for a quest.</summary>
	private bool _rejectQuest;

	////////////////////
	// PUBLIC PROPERTIES
	////////////////////
	
	///<summary> This is the character who speaks this line of dialogue </summary>
	public List<GameObject> speakers
	{
		get
		{
			return _speakers;
		}
	}

	///<summary> This is the line the character speaks</summary>
	public List<string> dialogues
	{
		get
		{
			return _dialogues;
		}
	}

	/// <summary>This is a short description of this dialogue. Will be displayed as dialogue options when multiple choices are available.</summary>
	public string description
	{
		get
		{
			return _description;
		}
	}

	///<summary> This is the style associated with this dialogue. Will be used by the Dialogue Box for the body.</summary>
	public List<Sprite> bodyStyles
	{
		get
		{
			return _bodyStyles;
		}
	}

	/// <summary> Tail style associated with this dialogue. Will be used on Dialogue Box to display the tail.</summary>
	public List<Sprite> tailStyles
	{
		get
		{
			return _tailStyles;
		}
	}

	/// <summary>the integer value that indicates which state this is. Primarily used for connecting transition functions</summary>
	public int stateNumber
	{
		get
		{
			return _stateNumber;
		}
	}

	/// <summary> Keeps track of the number of transitions from this state</summary>
	public int transitionCount
	{
		get
		{
			return _transitionCount;
		}
		set
		{
			_transitionCount = value;
		}
	}


	/// <summary>
	/// Returns true if this state accepts a quest
	/// </summary>
	public bool acceptQuest
	{
		get
		{
			return _acceptQuest;
		}
	}


	/// <summary>
	/// Returns true if this state rejects a quest.
	/// </summary>
	public bool rejectQuest
	{
		get
		{
			return _rejectQuest;
		}
	}


	/// <summary>
	/// Constructor for a new Dialogue State.
	/// </summary>
	/// <param name="newSpeakers">List of speakers in this state.</param>
	/// <param name="newDialogues">List of dialogues for the speakers in this state.</param>
	/// <param name="newDescription">Description for this state. Only used in a player state that came from a choice state.</param>
	/// <param name="newBodyStyles">List of body styles for this state. Created in DialogueLoader.</param>
	/// <param name="newTailStyles">List of tail styles for this state. Created in DialogueLoader.</param>
	/// <param name="newStateNumber">State number for this new state.</param>
	public DialogueState(List<GameObject> newSpeakers, List<string> newDialogues, string newDescription, List<Sprite> newBodyStyles, List<Sprite> newTailStyles, int newStateNumber)
	{
		_speakers = newSpeakers;
		_dialogues = newDialogues;
		_bodyStyles = newBodyStyles;
		_tailStyles = newTailStyles;
		_description = newDescription;
		_stateNumber = newStateNumber;
		_acceptQuest = false;
		_rejectQuest = false;
		_transitionCount = 0;

		printStatus = new Dictionary<GameObject, bool>();

		foreach (GameObject speaker in speakers)
			printStatus.Add(speaker, false);
	}


	/// <summary>
	/// This constructor is used for creating dialogue states related to a Quest.
	/// </summary>
	/// <param name="newSpeakers">List of speakers in this state.</param>
	/// <param name="newDialogues">List of dialogues for the speakers in this state.</param>
	/// <param name="newDescription">Description for this state. Only used in a player state that came from a choice state.</param>
	/// <param name="newBodyStyles">List of body styles for this state. Created in DialogueLoader.</param>
	/// <param name="newTailStyles">List of tail styles for this state. Created in DialogueLoader.</param>
	/// <param name="newStateNumber">State number for this new state.</param>
	/// <param name="doesStateAcceptQuest">Whether or not this state accepts a quest.</param>
	/// <param name="doesStateRejectQuest">Whether or not this state rejects a quest.</param>
	public DialogueState(List<GameObject> newSpeakers, List<string> newDialogues, string newDescription, List<Sprite> newBodyStyles, List<Sprite> newTailStyles, int newStateNumber, bool doesStateAcceptQuest, bool doesStateRejectQuest)
	{
		_speakers = newSpeakers;
		_dialogues = newDialogues;
		_bodyStyles = newBodyStyles;
		_tailStyles = newTailStyles;
		_description = newDescription;
		_stateNumber = newStateNumber;
		_acceptQuest = doesStateAcceptQuest;
		_rejectQuest = doesStateRejectQuest;
		_transitionCount = 0;

		printStatus = new Dictionary<GameObject, bool>();

		foreach (GameObject speaker in speakers)
			printStatus.Add(speaker, false);
	}


	/// <summary>
	/// Sets a character's print status to true.
	/// </summary>
	/// <param name="character"></param>
	public void StartPrinting(GameObject character)
	{
		printStatus[character] = true;
	}


	/// <summary>
	/// Sets a character's print status to false.
	/// </summary>
	/// <param name="character"></param>
	public void FinishPrinting(GameObject character)
	{
		printStatus[character] = false;
	}


	/// <summary>
	/// Returns true if all the speakers of this state have finished printing dialogue to their text boxes.
	/// </summary>
	public bool AreSpeakersFinishedPrinting()
	{
		foreach(GameObject speaker in speakers)
		{
			if (printStatus[speaker])
				return false; // if any of the speakers are still printing, return false
		}

		return true;
	}
}
