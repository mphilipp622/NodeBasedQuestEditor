using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for quest objectives. Used for polymorphism.
/// </summary>
public abstract class QuestObjective
{
	/// <summary>Description of this objective</summary>
	protected string objectiveDescription;

	protected GameObject questOwner, objectiveObject;

	protected bool objectiveActive;

	protected QuestObjective nextObjective;

	/// <summary>
	/// Returns the description of the objective.
	/// </summary>
	public string GetObjectiveDescription()
	{
		return objectiveDescription;
	}

	/// <summary>
	/// Sets this quest objective as the active objective in this objective path.
	/// </summary>
	public void SetActiveObjective()
	{
		objectiveActive = true;
	}

	/// <summary>
	/// Sets the objective that comes after this objective. When this objective is complete, the next objective becomes active.
	/// </summary>
	/// <param name="newNextObjective">The objective this objective unlocks upon completion.</param>
	public void SetNextObjective(QuestObjective newNextObjective)
	{
		nextObjective = newNextObjective;
	}

	/// <summary>
	/// Returns the objective game object.
	/// </summary>
	public GameObject GetObjectiveObject()
	{
		return objectiveObject;
	}

	/// <summary>
	/// Returns true if this objective has been completed.
	/// </summary>
	public abstract bool IsComplete();

	public void TransferOwner(GameObject newOwner)
	{
		questOwner = newOwner;
	}
}