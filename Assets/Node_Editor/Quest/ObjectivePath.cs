using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivePath : ScriptableObject
{
	public string objectivePathName;

	public List<SubObjective> objectives;

	public void Initialize(string newName)
	{
		objectivePathName = newName;
		objectives = new List<SubObjective>();
	}
}
