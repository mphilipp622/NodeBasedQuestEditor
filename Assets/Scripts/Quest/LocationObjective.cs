using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationObjective : QuestObjective
{

	/// <summary>Keeps track of if our destination has been reached or not.</summary>
	bool hasDestinationBeenReached;


	/// <summary>
	/// Creates a new location objective.
	/// </summary>
	/// <param name="newLocation">The required location game object.</param>
	public LocationObjective(GameObject newLocation)
	{
		objectiveObject = newLocation;
		hasDestinationBeenReached = false;

		// Create a new listener for messaging system.
		Messenger.AddListener<GameObject>("Location Reached", LocationReached);
	}


	/// <summary>
	/// Checks if the message we received was for this objective's location. Sets hasDestinationBeenReached to true if so.
	/// </summary>
	/// <param name="locationThatWasReached">The location we received in the message.</param>
	private void LocationReached(GameObject locationThatWasReached)
	{
		if (questOwner.tag != "Player")
			return;

		if (IsComplete() || !objectiveActive)
			return;

		// Compare prefab and instance locations. Since instance is being created at the same location as prefab, the prefab and instance will both have equal positions.
		if (locationThatWasReached.transform.position == objectiveObject.transform.position)
		{
			Debug.Log("Location Objective Reached");
			hasDestinationBeenReached = true;

			if (nextObjective != null)
				nextObjective.SetActiveObjective(); // set the next objective to active.
		}
	}


	/// <summary>
	/// Returns true if the player has reached this location.
	/// </summary>
	public override bool IsComplete()
	{
		return hasDestinationBeenReached;
	}
}
