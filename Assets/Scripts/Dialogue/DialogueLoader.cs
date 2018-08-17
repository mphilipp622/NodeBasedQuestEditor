using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Eppy;

public class DialogueLoader : MonoBehaviour
{
	/// <summary>Hashtable for looking up Game Objects in the scene by a name</summary>
	private Dictionary<string, GameObject> characters;

	private void Awake()
	{
		Initialize();
	}

	void Start ()
	{
		LoadDialogue(); // Might need to move this from Awake as more NPCs are added to the scene.
	}

	void Update ()
	{
		
	}

	/// <summary>
	/// Initializes the hashtable and populates it with the characters in the game.
	/// </summary>
	private void Initialize()
	{
		characters = new Dictionary<string, GameObject>();

		// Populate the hashtable with all the characters in the game.
		foreach (GameObject character in GameObject.FindGameObjectsWithTag("Character"))
			characters.Add(character.name, character);

		characters.Add(GameObject.FindWithTag("Player").name, GameObject.FindWithTag("Player"));
	}


	/// <summary>
	/// Parses the data from the FSM assets that have been created in the Dialogue Creator and creates
	/// DialogueFSM objects on the appropriate characters in the game.
	/// </summary>
	private void LoadDialogue()
	{
		Object[] fsms = Resources.LoadAll("Dialogue", typeof(DialogueNodeCanvas)); // load fsm's from Resources/Dialogue folder
		
		// Iterate over every dialogue FSM that is saved.
		for(int i = 0; i < fsms.Length; i++)
		{
			DialogueNodeCanvas fsm = (DialogueNodeCanvas)fsms[i]; // grab each Node Canvas Object, which holds all our states

			DialogueFSM newFSM = new DialogueFSM(fsm.numberOfTimesConversationCanHappen, fsm.probabilityOfOccurrence);

			string initiator = null; // the character who initiates this fsm

			// Iterate over every node in the FSM
			foreach (Node node in fsm.nodes)
			{
				if (node.GetType() == typeof(DialogueNode))
				{
					DialogueNode newNode = (DialogueNode)node;

					if (newNode.isStartState)
					{
						// Assign the initiator to the player if the player initiation toggle has been set on the start state. Otherwise, set it to the npc's name.
						initiator = newNode.playerInitiatesConversation ? GameObject.FindWithTag("Player").name : newNode.speakers[0].name;

						if (!characters.ContainsKey(initiator))
							return; // stop loading if the initiator doesn't exist.
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

					bool charactersExist = true;

					foreach (GameObject character in newNode.speakers)
					{
						if (!characters.ContainsKey(character.name))
						{
							charactersExist = false;
							break;
						}

						newSpeakerList.Add(characters[character.name]);
						newFSM.AddParticipant(characters[character.name]);
					}

					// If the character is not in the scene, stop loading this FSM.
					if (!charactersExist)
						break;
						
					newFSM.AddState(newSpeakerList, newNode.dialogues, newNode.description, newBodyStyles, newTailStyles, newNode.state);

					// Add Transitions for non-choice nodes.
					if (newNode.Outputs[0].connections.Count > 0)
					{
						// Ignore states that transition to a choice state
						if (newNode.Outputs[0].connections[0].body.state > -2)
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
				else if(node.GetType() == typeof(DialogueChoiceNode))
				{
					DialogueChoiceNode newNode = (DialogueChoiceNode)node;
					
					// Parse transitions from choice node. Origin state is what leads us to the choice node. It is the state, all dialogue choices come from.
					for(int j = 0; j < newNode.Outputs.Count; j++)
					{
						if (newNode.Outputs[j].connections.Count > 0)
							newFSM.AddTransition(newNode.originState, newNode.Outputs[j].connections[0].body.state, j + 1);
					}
				}
			}

			// Parse required quests
			List<string> tempRequiredStates = new List<string>();

			foreach (GameStateName stateName in fsm.requiredGameStates)
				tempRequiredStates.Add(stateName.eventName);

			newFSM.AddRequiredGameStates(tempRequiredStates);

			characters[initiator].GetComponent<DialogueManager>().AddFSM(newFSM); // add the new FSM to the character's FSM list.
		}

		Destroy(gameObject); // destroy the game object after it's finished loading all dialogue
	}
}
