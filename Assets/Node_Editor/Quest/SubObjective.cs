using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubObjective : ScriptableObject
{
	public int numberToCollect;
	public GameObject objective;
	public bool moveUp, moveDown, delete;

	public void Initialize()
	{
		numberToCollect = 0;
		objective = null;
	}
}
