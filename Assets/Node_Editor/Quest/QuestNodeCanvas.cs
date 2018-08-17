using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Contains data related to the Quest System. Inherits Node_Canvas_Object.
/// </summary>
public class QuestNodeCanvas : Node_Canvas_Object
{
	/// <summary> The name of this quest. </summary>
	public string questName;

	///////////////////
	// Quest Objectives
	///////////////////

	// Objectives can be thought of as a 2D array. The first dimension is a path, which contains N number of objectives. A quest may have N number of objective paths, each containing N number of objectives. This allows for simultaneous objective paths.

	/// <summary> List of objectives required for this quest. </summary>
	public List<GameObject> questObjectives;

	/// <summary> The list of objective paths. </summary>
	public List<ObjectivePath> objectivePaths;

	/// <summary> The number of objectives to collect. E.G: Kill 5 enemies, collect 3 flowers. </summary>
	public List<int> numberOfObjectivesToCollect;

	/// <summary> List of prerequisite quests that must be complete for this quest to be available. </summary>
	public List<QuestAsset> requiredQuests;

	/// <summary> The quest asset for this quest. </summary>
	public QuestAsset newQuest;

	/// <summary> The NPC that gives this quest. </summary>
	public GameObject questGiver;
}
