using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObjective : QuestObjective
{
	/// <summary>The enemy required for this objective.</summary>
	private Enemy enemy;

	/// <summary>The number of this type of enemy that needs to be killed.</summary>
	private int numberToKill;

	/// <summary>Remaining number of enemies of this type that need to be killed.</summary>
	private int numberRemaining;

	/// <summary>
	/// Creates a new enemy objective.
	/// </summary>
	/// <param name="newEnemy">The type of enemy that must be killed.</param>
	/// <param name="newNumberToKill">The number of the type of enemy to be killed.</param>
	public EnemyObjective(Enemy newEnemy, int newNumberToKill, GameObject newOwner)
	{
		enemy = newEnemy;
		numberToKill = newNumberToKill;
		numberRemaining = newNumberToKill;
		questOwner = newOwner;
		objectiveObject = newEnemy.gameObject;

		// Create a listener for our messaging system. Will be called on by Enemy.Die();
		Messenger.AddListener<Enemy>("Enemy Killed", EnemyKilled); 
	}


	/// <summary>
	/// Updates the objective.
	/// </summary>
	/// <param name="enemyThatWasKilled">The type of enemy that was killed.</param>
	private void EnemyKilled(Enemy enemyThatWasKilled)
	{
		if (questOwner == null)
			return;

		if (questOwner.tag != "Player")
			return;

		if (IsComplete() || !objectiveActive)
			return;

		if (enemy.GetName() == enemyThatWasKilled.GetName())
		{
			// Check and make sure the message we received was for this specific enemy.

			numberRemaining--; // decrement remaining count.

			if (IsComplete() && nextObjective != null)
				nextObjective.SetActiveObjective(); // set the next objective to active.

			Debug.Log("Enemy " + enemyThatWasKilled.GetName() + " Killed " + numberRemaining + " Remain");
		}
	}

	/// <summary>
	/// Returns true if the required number of enemies of this type have been killed.
	/// </summary>
	public override bool IsComplete()
	{
		return numberRemaining <= 0 ? true : false;
	}
}
