using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains data related to the Dialogue System. Inherits Node_Canvas_Object
/// </summary>
public class DialogueNodeCanvas : Node_Canvas_Object
{
	/// <summary> Specifies how many times this conversation can occur in the game. </summary>
	public int numberOfTimesConversationCanHappen = -1;

	/// <summary> Specifies the probability this conversation occurs. Value must be between 0 and 1.0. </summary>
	public float probabilityOfOccurrence = 1.0f;

	/*
	 * E.G: An NPC has 3 different conversations they can have with the player. Which one occurs can be random 
	 * based on this probability. Contributes to a more dynamic conversation system.
	 */
}
