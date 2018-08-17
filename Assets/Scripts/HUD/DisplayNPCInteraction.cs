using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayNPCInteraction : MonoBehaviour
{

	[SerializeField]
	GameObject interactionIcon;

	private void Awake()
	{
		ErrorCheck();
	}
	void Start ()
	{
		interactionIcon.SetActive(false); // set the interaction icon to false by default.
	}
	
	void Update ()
	{
		
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.tag == "Player")
			// turn on icon if player enters trigger.
			interactionIcon.SetActive(true);
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.tag == "Player")
			// turn off the icon if player leaves trigger
			interactionIcon.SetActive(false);
	}

	void ErrorCheck()
	{
		// check for interaction icon. Interaction icon is the icon that displays above an npc's head to let the player know they can press a key to interact with them.
		if (!interactionIcon)
		{
			// if the gameObject is not assigned yet in inspector, then do a get component to assign ti.
			interactionIcon = transform.Find("SelectionIcon").gameObject;
			
			if (!interactionIcon)
				// if we STILL don't have the child game object, then that means the npc does not have the child. Quit if so.
				Debug.LogError("ERROR: GameObject is missing InteractionIcon child: " + transform.name);
		}
	}
}
