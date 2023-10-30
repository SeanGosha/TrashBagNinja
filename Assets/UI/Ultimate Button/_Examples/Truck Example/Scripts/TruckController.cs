/* Written by Kaz Crowe */
/* TruckController.cs */
using UnityEngine;
using System.Collections;

public class TruckController : MonoBehaviour
{
	[Header( "Assigned Variables" )]
    public Transform cameraTransform;
	public Rigidbody2D myRigidbody;
	public WheelJoint2D frontWheel;
	public WheelJoint2D rearWheel;
	public Rigidbody2D frontRigidbody;
	public Rigidbody2D backRigidbody;
	public Transform groundCheck;

	[Header( "Speeds and Times" )]
	public int maxSpeed = 1200;
	public int reverseSpeed = 250;
	public float accelerationSpeed = 10.0f;
	public float decelerationSpeed = 5.0f;
	public float resetTimerMax = 2.5f;

	// Misc. Private Variables
	float motorSpeed = 0;
	Vector3 cameraDefaultPosition;
	float resetTimer = 0.0f;
	bool isResetting = false;
	bool isGrounded = false;
	float resetTruckTimer = 0.0f;
	float resetTruckTimerMax = 5.0f;


	void Start ()
	{
		// Store the current position of the camera in relation to the truck.
		cameraDefaultPosition = cameraTransform.position - transform.position;
		
		// Apply the resetTimerMax to the resetTimer.
		resetTimer = resetTimerMax;
	}

	void Update ()
	{
		// If the button has been pressed down on this frame, and the truck is not moving faster than -250, then set the motor speed to -250.
		if( UltimateButton.GetButtonDown( "Gas" ) && motorSpeed > -250 )
			motorSpeed = -250;

		// If the ResetTruck button have been pressed and the reset timer is reset, then reset the truck.
		if( UltimateButton.GetButtonDown( "ResetTruck" ) && resetTruckTimer <= 0 )
			ResetTruck();

		if( resetTruckTimer > 0 )
			resetTruckTimer -= Time.deltaTime;

		UltimateButton.GetUltimateButton( "ResetTruck" ).UpdateCooldown( resetTruckTimer, resetTruckTimerMax );
	}
	
	void FixedUpdate ()
	{
		// Determine if the truck is grounded or not by using a Line cast.
		isGrounded = Physics2D.Linecast( transform.position, groundCheck.position, 1 << LayerMask.NameToLayer( "Default" ) );

		// If the gas button is down...
		if( UltimateButton.GetButton( "Gas" ) )
		{
			// If the motor speed is less than the max speed, then accelerate.
			if( motorSpeed > -maxSpeed )
				motorSpeed -= Time.deltaTime * accelerationSpeed;

			// If the truck is grounded, then add force to help the truck have more power.
			if( isGrounded == true )
				myRigidbody.AddForceAtPosition( new Vector2( 20, 0 ), new Vector2( transform.position.x - 1, transform.position.y - 1 ) );
		}
		else
		{
			// If the reverse button is down and the motor is not going at all...
			if( UltimateButton.GetButton( "Reverse" ) && motorSpeed >= 0 )
			{
				// Then set the motor speed.
				if( motorSpeed != reverseSpeed )
					motorSpeed = reverseSpeed;

				// If the truck is grounded, then add force to help give more power.
				if( isGrounded == true )
					myRigidbody.AddForceAtPosition( new Vector2( -10, 0 ), new Vector2( transform.position.x, transform.position.y - 1 ) );
			}
			else
			{
				// If the motor is running, then decelerate the truck.
				if( motorSpeed < 0 )
					motorSpeed += Time.deltaTime * decelerationSpeed;
				// Else if the motor speed is not stopped correctly and the truck is not in reverse, then set the motor speed to 0.
				else if( motorSpeed > 0 && !UltimateButton.GetButton( "Reverse" ) )
					motorSpeed = 0;

				// If the truck is grounded, the wheel speed is nearly stopped, and the truck is still moving, then add force to slow the truck down.
				if( isGrounded == true && motorSpeed > -100 && myRigidbody.velocity.x > 2 )
					myRigidbody.AddForceAtPosition( new Vector2( -30, 0 ), new Vector2( transform.position.x, transform.position.y - 1 ) );
			}
		}
		
		JointMotor2D newMotor = new JointMotor2D();
		newMotor.maxMotorTorque = 10000;
		newMotor.motorSpeed = motorSpeed;

		// Apply the motor force.
		rearWheel.motor = newMotor;
		frontWheel.motor = newMotor;

		// Make the camera follow the truck.
		cameraTransform.position = transform.position + cameraDefaultPosition;

		// If the truck is not grounded and the truck is not moving, and the truck is not in the middle of resetting...
		if( !isGrounded && myRigidbody.velocity.magnitude <= 0.5f && !isResetting )
		{
			// Reduce the reset timer.
			resetTimer -= Time.deltaTime;

			// If the timer is less than zero, then reset the timer and start the ResetTruck coroutine.
			if( resetTimer <= 0 )
				ResetTruck();
		}
		// Else if the truck is grounded and the timer isn't at max, then reset the timer to max.
		else if( isGrounded && resetTimer != resetTimerMax )
			resetTimer = resetTimerMax;
	}

	void ResetTruck ()
	{
		if( isResetting )
			return;

		resetTimer = resetTimerMax;
		StartCoroutine( "ResetTruckCoroutine" );

		resetTruckTimer = resetTruckTimerMax;
	}

	IEnumerator ResetTruckCoroutine ()
	{
		// Set isResetting to true so that other functions know this function is running.
		isResetting = true;

		// Set the rigidbody to isKinematic to not allow physics movements to it.
		myRigidbody.isKinematic = true;
		myRigidbody.velocity = Vector3.zero;
		
		// Store the positions of where the truck currently is, and where the trucks ending position should be.
		Vector2 originalPos = transform.position;
		Vector2 endPos = new Vector2( transform.position.x, transform.position.y + 3f );

		// Store the original rotation of the truck.
		Quaternion originalRot = transform.rotation;

		for( float t = 0.0f; t < 1.0f; t += Time.deltaTime * 1.5f )
		{
			// Lerp the position from the original to the end position.
			transform.position = Vector2.Lerp( originalPos, endPos, t );

			// Slerp the rotation from the original to a 0 rotation.
			transform.rotation = Quaternion.Slerp( originalRot, Quaternion.identity, t );
			
			// Wait for the Fixed Update so that all movement is fluid.
			yield return new WaitForFixedUpdate();
		}

		// Set the back and front tire's velocity to 0 to avoid moving the truck at all.
		backRigidbody.velocity = Vector2.zero;
		backRigidbody.rotation = 0.0f;
		frontRigidbody.velocity = Vector2.zero;

		// Allow for physics movements again.
		myRigidbody.isKinematic = false;
		
		// Set isResetting to false so that this script can call this function again.
		isResetting = false;
	}
}