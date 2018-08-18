using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestListener : MonoBehaviour
{

	private void OnEnable()
	{
		Messenger.AddListener<string>("Game State Update", MyFunction);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener<string>("Game State Update", MyFunction);
	}

	private void MyFunction(string stateName)
	{
		if (stateName == "VOLCANO_ERUPTED")
			Debug.Log("Oh no, a volcano erupted!");
	}
	
	void Start () {
		
	}
	
	void Update () {
		
	}
}
