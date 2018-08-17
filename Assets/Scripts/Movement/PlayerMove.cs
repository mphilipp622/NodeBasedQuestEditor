using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
	// Animator Components
	[SerializeField]
	AnimationManager animationManager;

	Rigidbody2D rigidBody;

	// movement boolean for blend tree. Initialized to false by default
	bool moving = false;

	// direction variables. Initialize previous values to 0
	float xDir, yDir, xDirPrev = 0, yDirPrev = 0;

	// Physics variables
	[SerializeField]
	float movementSpeed = 2.0f;

	// Previous position vector. Used for determining if we've moved or not.
	Vector3 previousPosition;

	bool lockPlayerMovement;

	private void OnEnable()
	{
		Messenger.AddListener("LockPlayerMovement", LockPlayerMovement);
		Messenger.AddListener("UnlockPlayerMovement", UnlockPlayerMovement);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener("LockPlayerMovement", LockPlayerMovement);
		Messenger.RemoveListener("UnlockPlayerMovement", UnlockPlayerMovement);
	}

	void Start ()
	{
		rigidBody = GetComponent<Rigidbody2D>();
		previousPosition = transform.position; // initialize previous position to starting position of object
		lockPlayerMovement = false;
	}
	
	void Update ()
	{
		
	}

	public void SetSpeed(float newSpeed)
	{
		movementSpeed = newSpeed;
	}

	public float GetMovementSpeed()
	{
		return movementSpeed;
	}

	private void FixedUpdate()
	{
		Move();
	}

	void Move()
	{
		if (lockPlayerMovement)
			return;

		//Vector2 newPos = ( (new Vector2(transform.position.x, transform.position.y)) + (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))) / movementSpeed );

		rigidBody.AddForce(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * movementSpeed * Time.deltaTime, ForceMode2D.Impulse);

		//rigidBody.MovePosition( newPos );
		//transform.position += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * movementSpeed * Time.deltaTime;

		if (transform.position - previousPosition == Vector3.zero)
			// This will be true if we haven't moved
			moving = false;
		else
			moving = true;

		previousPosition = transform.position; // set previous position to the current position;

		// Pass the axis values to SetDirections. These values should be constrained between -1 and 1. Only execute if we've moved
		SetDirections(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
	}

	void SetDirections(float newXDirection, float newYDirection)
	{
		if(moving)
		{
			// first set previous directions to the current directions
			xDirPrev = xDir;
			yDirPrev = yDir;
		}
		

		// clamp x and y directions to 1 or -1
		if (newXDirection < 0)
			newXDirection = -1.0f;
		else if (newXDirection > 0)
			newXDirection = 1.0f;
		if (newYDirection < 0)
			newYDirection = -1.0f;
		else if (newYDirection > 0)
			newYDirection = 1.0f;

		// Set x and y directions to new values.
		xDir = newXDirection;
		yDir = newYDirection;

		// Send new directions to Animator
		SendDirectionsToAnimator();
	}

	/// <summary>
	/// Sends current x and y directions and previous x and y directions to AnimationManager to execute blend tree animations.
	/// </summary>
	void SendDirectionsToAnimator()
	{
		animationManager.SetDirection(xDir, yDir, xDirPrev, yDirPrev, moving);
	}

	/// <summary>
	/// Locks player from moving. Also tells the animator to stop animating movement.
	/// </summary>
	void LockPlayerMovement()
	{
		lockPlayerMovement = true;

		moving = false;
		SendDirectionsToAnimator();
	}

	void UnlockPlayerMovement()
	{
		lockPlayerMovement = false;
	}

	void ErrorCheck()
	{
		// Check AnimationManager Component for Errors
		if (!animationManager)
		{
			animationManager = GetComponent<AnimationManager>(); // Attempt to get the animator.
			if (!animationManager) // if we still don't have an animator, throw console error
				Debug.LogError("ERROR: animationManager component must be assigned on object " + gameObject.name);
		}
	}

	public void ModifyMovementSpeedWithScale( float scalePercentage)
	{
		Debug.Log(scalePercentage);
		movementSpeed *= scalePercentage;
	}
}
