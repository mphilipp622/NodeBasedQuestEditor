using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestLocation : MonoBehaviour
{

	private void Awake() {
		
	}
	void Start ()
	{
		
	}

	void Update ()
	{
		
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//Debug.Log("TestCollider");
		if (collision.tag == "Player")
			Messenger.Broadcast<GameObject>("Location Reached", gameObject);
	}
}
