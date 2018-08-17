using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestLoader : MonoBehaviour
{
	/// <summary>Hashtable for looking up Quest Givers in the scene by a name</summary>
	private Dictionary<string, GameObject> questGivers;

	/// <summary>Hashtable of characters in the game.</summary>
	private Dictionary<string, GameObject> speakers;

	private void Awake()
	{
		Initialize();
	}

	void Start ()
	{
		LoadQuests();
	}
	
	void Update ()
	{
		
	}

	/// <summary>
	/// Initializes member variables
	/// </summary>
	private void Initialize()
	{
		questGivers = new Dictionary<string, GameObject>();
		speakers = new Dictionary<string, GameObject>();

		// Populate the hashtable with all the characters in the game.
		foreach (GameObject character in GameObject.FindGameObjectsWithTag("Character"))
		{
			if(character.GetComponent<QuestGiver>() != null)
				questGivers.Add(character.name, character);

			speakers.Add(character.name, character);
		}

		speakers.Add("Player", GameObject.FindWithTag("Player"));
	}


	/// <summary>
	/// Parses the Dialogue FSM from the node editor and returns a DialgueFSM object.
	/// </summary>
	private DialogueFSM ParseDialogueFromQuest(QuestNodeCanvas questToParse, bool getTurnInQuestDialogue, bool getInProgressDialogue)
	{
		QuestNodeCanvas fsm = questToParse; // grab each Node Canvas Object, which holds all our states
		
		DialogueFSM newFSM = new DialogueFSM(-1, 1.0f); // default values for quest dialogue. Probability is always 100% and can happen infinite number of times if the quest is available.

		string initiator = null; // the character who initiates this fsm
		
		// Iterate over every node in the FSM
		foreach (IQuestNode node in fsm.nodes)
		{
			// the boolean parameters act like switches. If we want a specific interaction, we want to ignore nodes that do not meet that interaction's criteria.
			if (!getTurnInQuestDialogue && !getInProgressDialogue && (node.IsTurnInDialogue() || node.IsInProgressDialogue()))
				continue;
			else if (getTurnInQuestDialogue && !node.IsTurnInDialogue())
				continue;
			else if (getInProgressDialogue && !node.IsInProgressDialogue())
				continue;

			if (node.GetType() == typeof(QuestDialogueNode))
			{
				QuestDialogueNode newNode = (QuestDialogueNode)node;

				if (newNode.isStartState)
				{
					// Assign the initiator to the player if the player initiation toggle has been set on the start state. Otherwise, set it to the npc's name.
					initiator = newNode.playerInitiatesConversation ? "Player" : newNode.speakers[0].name;
					newFSM.SetStartState(newNode.state);
				}

				// Separate the style sprite into a body and tail. Note that the body and tail sprites MUST have the same prefix to their names as the original style. For instance, if the original style is "NormalDialogue", then the body and tail must be "NormalDialogueBody" and "NormalDialogueTail".
				List<Sprite> newBodyStyles = new List<Sprite>();
				List<Sprite> newTailStyles = new List<Sprite>();

				for (int j = 0; j < newNode.styles.Count; j++)
				{
					newBodyStyles.Add(Resources.Load<Sprite>("Sprites/Dialogue/" + newNode.styles[j].name + "Body"));
					newTailStyles.Add(Resources.Load<Sprite>("Sprites/Dialogue/" + newNode.styles[j].name + "Tail"));
				}

				// Parse data from dialogue node. Note that we are using our characters hash table for the speaker. This might not be necessary, but this should ensure that we are grabbing the instances of the gameobjects that are IN the scene instead of in the editor. I'm not sure if there's a distinction between prefabs in the editor and instantiated prefabs in the scene.
				List<GameObject> newSpeakerList = new List<GameObject>();

				foreach (GameObject character in newNode.speakers)
				{
					newSpeakerList.Add(speakers[character.name]);
					newFSM.AddParticipant(speakers[character.name]);
				}

				newFSM.AddQuestState(newSpeakerList, newNode.dialogues, newNode.description, newBodyStyles, newTailStyles, newNode.state, newNode.accept, newNode.reject);

				// Add Transitions for non-choice nodes.
				if (newNode.Outputs[0].connections.Count > 0)
				{
					// Ignore states that transition to a choice state
					if (newNode.Outputs[0].connections[0].body.state != -2)
						// Note that all states fire on the 0 input. However, choices fire on 1 - 9 inputs. Proceeding through dialogue always occurs using 0 input.
						newFSM.AddTransition(newNode.state, newNode.Outputs[0].connections[0].body.state, 0);
				}
				else if (newNode.returnsToChoiceNode)
				{
					// Iterate over the previous choice node and create transitions for this state to go to any of the choice options on the previous choice node.
					for (int j = 0; j < newNode.previousChoiceNode.Outputs.Count; j++)
					{
						if (newNode.previousChoiceNode.Outputs[j].connections.Count == 0)
							continue;

						newFSM.AddTransition(newNode.state, newNode.previousChoiceNode.Outputs[j].connections[0].body.state, j + 1);
					}
				}
				else
					// handle the final state case. A dialogue node with no output connection must be a final state so it transitions to -1.
					newFSM.AddTransition(newNode.state, -1, 0);
			}
			else if (node.GetType() == typeof(QuestChoiceNode))
			{
				QuestChoiceNode newNode = (QuestChoiceNode) node;

				// Parse transitions from choice node. Origin state is what leads us to the choice node. It is the state, all dialogue choices come from.
				for (int j = 0; j < newNode.Outputs.Count; j++)
				{
					if (newNode.Outputs[j].connections.Count > 0)
						newFSM.AddTransition(newNode.originState, newNode.Outputs[j].connections[0].body.state, j + 1);
				}
			}
		}

		// Check and make sure states were added. No states may be added to the FSM if no turn in dialogue options were created.
		if (newFSM.GetStateCount() > 0)
			return newFSM;
		else
			return null;
	}


	/// <summary>
	/// Adds a quest to this npc's quest inventory.
	/// </summary>
	/// <param name="newQuest">The quest to give this npc</param>
	private void AssignQuestToGiver(Quest newQuest)
	{
		questGivers[newQuest.GetQuestGiver().name].GetComponent<QuestGiver>().AddQuest(newQuest);
	}


	/// <summary>
	/// Loads quests from the inspector and assigns them to all the quest givers in the game.
	/// </summary>
	private void LoadQuests()
	{
		// Get All quest canvas objects
		Object[] questCanvases = Resources.LoadAll("QuestCanvas", typeof(QuestNodeCanvas));

		// for each quest, parse the dialogue FSM and update the quest's dialogue.
		foreach (QuestNodeCanvas questCanvas in questCanvases)
		{
			Quest newQuest = new Quest(questCanvas);
			newQuest.AddDialogue(ParseDialogueFromQuest(questCanvas, false, false));
			newQuest.AddTurnInDialogue(ParseDialogueFromQuest(questCanvas, true, false));
			newQuest.AddInProgressDialogue(ParseDialogueFromQuest(questCanvas, false, true));
			AssignQuestToGiver(newQuest);
		}
	}
}
