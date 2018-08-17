using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
	// Animator Component
	[SerializeField]
	Animator thisAnimator;

	// will be used for idle animations. We want to know which way we were facing last
	float xDirPrevious, yDirPrevious;

	private void Awake()
	{
		ErrorCheck();
	}

	void Start ()
	{
		
	}
	
	void Update ()
	{
		
	}

	private void ErrorCheck()
	{
		// Check Animator Component for Errors
		if (!thisAnimator)
		{
			thisAnimator = GetComponent<Animator>(); // Attempt to get the animator.
			if (!thisAnimator) // if we still don't have an animator, throw console error
				Debug.LogError("ERROR: thisAnimator component must be assigned on object " + gameObject.name);
		}
	}

	/// <summary>
	/// Sets Blend Tree x and y direction values so that the animation updates accordingly.
	/// Will also set the previous x and y directions.
	/// </summary>
	/// <param name="newXDirection">New x direction of object</param>
	/// <param name="newYDirection">New y direction of object</param>
	/// <param name="prevX"> previous x direction of object</param>
	/// <param name="prevY">previous y direction of objects</param>
	/// <param name="isMoving">Boolean to determine if object is moving or not</param>
	public void SetDirection(float newXDirection, float newYDirection, float prevX, float prevY, bool isMoving)
	{
		thisAnimator.SetFloat("xDir", newXDirection);
		thisAnimator.SetFloat("yDir", newYDirection);
		thisAnimator.SetFloat("xDirLast", prevX);
		thisAnimator.SetFloat("yDirLast", prevY);

		thisAnimator.SetBool("Movement", isMoving); // set whether we should be in idle or movement animation states
	}
}
