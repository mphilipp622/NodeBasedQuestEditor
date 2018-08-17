using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[SerializeField]
	protected int hp;

	[SerializeField]
	protected float moveSpeed;
	
	[SerializeField]
	protected int defense;

	[SerializeField]
	protected int attack;

	[SerializeField]
	protected string name;

	void Start ()
	{
		
	}

	void Update ()
	{
		
	}


	/// <summary>
	/// Kills enemy and sends a message to quest objectives for updating.
	/// </summary>
	protected void Die()
	{
		// EnemyObjective class listens for this message
		Messenger.Broadcast<Enemy>("Enemy Killed", this);
	}

	public int GetHP()
	{
		return hp;
	}

	public float GetMoveSpeed()
	{
		return moveSpeed;
	}

	public int GetDefense()
	{
		return defense;
	}

	public int GetAttack()
	{
		return attack;
	}

	public string GetName()
	{
		return name;
	}
}
