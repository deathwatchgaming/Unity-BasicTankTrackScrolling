/*
 * UnityTank: TankControl.cs
 * Version: Unity 6+ (New Input)
 * Edits By: DeathwatchGaming
 * License: MIT
 * Description: This script controls the movement and behavior of the tank, including handling player input for acceleration, braking, and steering. It uses Unity's WheelCollider components to simulate the physics of the tank's tracks and wheels, allowing for realistic movement and interaction with the terrain. The script also updates the visual representation of the wheels to match their physical state, ensuring that the tank's appearance reflects its movement and interactions with the environment.
 */

// Import necessary namespaces
using System;
using UnityEngine;
using System.Collections.Generic;

// Define the namespace for the tank control script
namespace UnityTank.Scripts
{
	public class TankControl : MonoBehaviour
	{
		[Serializable]
		public struct LeftWheel
		{
			[Tooltip("The wheel collider.")]
			// The wheel collider is a special type of collider in Unity that simulates the physical behavior of a wheel, including its interaction with the terrain and its response to forces such as motor torque and braking. It is responsible for handling the physics calculations related to the wheel's movement and contact with the ground.
			public WheelCollider wheelCollider;
			[Tooltip("The wheel mesh.")]
			// The wheel mesh is the visual representation of the wheel that will be updated to match the position and rotation of the corresponding wheel collider. This allows the tank's wheels to visually reflect their physical interactions with the terrain, such as rolling and steering.
			public Transform wheelMesh;
			[Tooltip("The motorized bool.")]
			// The motorized boolean indicates whether this wheel should receive motor torque to propel the tank. Setting this to true allows the wheel to contribute to the tank's movement, while setting it to false means the wheel will only roll freely without providing any driving force.
			public bool motorized;
		}

		[Serializable]
		public struct RightWheel
		{
			[Tooltip("The wheel collider.")]
			// The wheel collider is a special type of collider in Unity that simulates the physical behavior of a wheel, including its interaction with the terrain and its response to forces such as motor torque and braking. It is responsible for handling the physics calculations related to the wheel's movement and contact with the ground.
			public WheelCollider wheelCollider;
			[Tooltip("The wheel mesh.")]
			// The wheel mesh is the visual representation of the wheel that will be updated to match the position and rotation of the corresponding wheel collider. This allows the tank's wheels to visually reflect their physical interactions with the terrain, such as rolling and steering.
			public Transform wheelMesh;
			[Tooltip("The motorized bool.")]
			// The motorized boolean indicates whether this wheel should receive motor torque to propel the tank. Setting this to true allows the wheel to contribute to the tank's movement, while setting it to false means the wheel will only roll freely without providing any driving force.
			public bool motorized;
		}

		[Header("Track Wheels")]
		[Tooltip("The left wheels.")]
		// Initialize the leftWheels list with 9 default LeftWheel structs to ensure it has the correct size in the inspector
		[SerializeField] private List<LeftWheel> leftWheels = new List<LeftWheel>(new LeftWheel[9]);
		[Tooltip("The right wheels.")]
		// Initialize the rightWheels list with 9 default RightWheel structs to ensure it has the correct size in the inspector
		[SerializeField] private List<RightWheel> rightWheels = new List<RightWheel>(new RightWheel[9]);

		[Header("Tank Properties")]
		[Tooltip("The rigidbody mass amount.")]
		// Set a default mass value for the rigidbody to ensure the tank has appropriate physics behavior
		[SerializeField] private float rigidBodyMass = 9000f;
		[Tooltip("The center of gravity offset amount.")]
		// Set a default center of gravity offset value to lower the center of mass and improve stability, which helps prevent the tank from rolling over during sharp turns or on uneven terrain
		[SerializeField] private float centerOfGravityOffset = -1f;
		[Tooltip("The motor torque amount.")]
		// Set a default motor torque value to ensure the tank has sufficient power to move and accelerate effectively
		[SerializeField] private float motorTorque = 2000f;
		[Tooltip("The brake torque amount.")]
		// Set a default brake torque value to ensure the tank can decelerate and stop effectively when the brake is applied
		[SerializeField] private float brakeTorque = 2000f;
		[Tooltip("The maximum speed amount.")]
		// Set a default maximum speed value to limit the tank's top speed, which helps maintain control and prevent excessive speeds that could lead to instability or loss of control
		[SerializeField] private float maxSpeed = 10f;
	    
		private Rigidbody rigidBody;
		private Vector3 centerOfMass;
		private Vector3 leftWheelPosition;
		private Quaternion leftWheelRotation;
		private Vector3 rightWheelPosition;
		private Quaternion rightWheelRotation;		

		private TankInputActions tankControls; // Reference to the new input system
		private Vector2 inputVector;	
		
		private float motorInput;
		private float steerInput;
		private float forwardSpeed;
		private float speedFactor;
		private float currentMotorTorque;
		private bool isAccelerating;
		private bool isBrakingKey;

		// Awake is called when the script instance is being loaded	
		private void Awake()
		{
			// Create a new instance of the TankInputActions class
			tankControls = new TankInputActions(); // Initialize Input Actions				
		}

		// Start is called before the first frame update
		private void Start()
		{
			// Get the Rigidbody component
			rigidBody = GetComponent<Rigidbody>();

			// Set the rigidbody mass
			rigidBody.mass = rigidBodyMass; // Set the mass of the rigidbody to the specified value

			// Adjust center of mass to improve stability and prevent rolling
			centerOfMass = rigidBody.centerOfMass; // Get the current center of mass
			centerOfMass.y += centerOfGravityOffset; // Lower the center of mass by the specified offset to improve stability
			rigidBody.centerOfMass = centerOfMass; // Apply the adjusted center of mass to the rigidbody
		}

		// Enable the input actions when the object is enabled
		private void OnEnable()
		{
			tankControls.Enable();
		}

		// Disable the input actions when the object is disabled
		private void OnDisable()
		{
			tankControls.Disable();
		}	

		// Update is called every frame
		private void Update()
		{
			// Check braking button value
			isBrakingKey = tankControls.Tank.Brake.IsPressed();

			UpdateWheels();
		}

		// FixedUpdate is called at a fixed time interval
		private void FixedUpdate()
		{
			// Read the Vector2 input from the new Input System
			inputVector = tankControls.Tank.Movement.ReadValue<Vector2>();

			// Get player input for acceleration
			motorInput = inputVector.y; // Forward / backward input
			steerInput = inputVector.x; // Steering input

			// Calculate current speed along the tank's forward axis
			forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity); // Speed in the forward direction
			speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed)); // Normalized speed factor

			// Reduce motor torque and steering at high speeds for better handling
			currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

			// Determine if the player is accelerating or trying to reverse
			isAccelerating = Mathf.Sign(motorInput) == Mathf.Sign(forwardSpeed);

			// Apply motor torque and braking to each wheel
			foreach (var leftWheel in leftWheels)
			{
				// Determine if the player is accelerating or trying to reverse
				if (isAccelerating)
				{
					// Apply motor torque to motorized wheels
					if (leftWheel.motorized)
					{
						leftWheel.wheelCollider.motorTorque = motorInput * currentMotorTorque; // Apply forward/backward torque
						leftWheel.wheelCollider.motorTorque += motorTorque * steerInput; // Apply steering torque (positive for left, negative for right)
					}

					// Apply braking if brake key is pressed
					if (isBrakingKey)
					{
						// Apply brakes
						leftWheel.wheelCollider.motorTorque = 0f; // Stop applying motor torque when braking
						leftWheel.wheelCollider.brakeTorque = Mathf.Abs(motorInput) * brakeTorque; // Apply brake torque proportional to the input for smoother braking
					}

					// Release brakes when accelerating and brake key is not pressed
					else if (!isBrakingKey)
					{
						// Release brakes when accelerating
						leftWheel.wheelCollider.brakeTorque = 0f; // Release brake torque to allow movement when accelerating
					}										
 				}

				// Apply brakes when reversing direction
 				else 
 				{
					// Apply brakes when reversing direction
					leftWheel.wheelCollider.motorTorque = 0f; // Stop applying motor torque when changing direction
					leftWheel.wheelCollider.brakeTorque = Mathf.Abs(motorInput) * brakeTorque; // Apply brake torque proportional to the input for smoother braking when changing direction
 				}
 			}

			// Apply motor torque and braking to each wheel
			foreach (var rightWheel in rightWheels)
			{
				// Determine if the player is accelerating or trying to reverse
				if (isAccelerating)
				{
					// Apply torque to motorized wheels
					if (rightWheel.motorized)
					{
						rightWheel.wheelCollider.motorTorque = motorInput * currentMotorTorque; // Apply forward/backward torque
						rightWheel.wheelCollider.motorTorque -= motorTorque * steerInput; // Apply steering torque (negative for right, positive for left)
					}

					// Apply braking if brake key is pressed
					if (isBrakingKey)
					{
						// Apply brakes
						rightWheel.wheelCollider.motorTorque = 0f; // Stop applying motor torque when braking
						rightWheel.wheelCollider.brakeTorque = Mathf.Abs(motorInput) * brakeTorque; // Apply brake torque proportional to the input for smoother braking
					}

					// Release brakes when accelerating and brake key is not pressed
                    else if (!isBrakingKey)
                    {
						// Release brakes when accelerating
						rightWheel.wheelCollider.brakeTorque = 0f; // Release brake torque to allow movement when accelerating
					}										
 				}

				// Apply brakes when reversing direction
 				else 
 				{
					// Apply brakes when reversing direction
					rightWheel.wheelCollider.motorTorque = 0f; // Stop applying motor torque when changing direction
					rightWheel.wheelCollider.brakeTorque = Mathf.Abs(motorInput) * brakeTorque; // Apply brake torque proportional to the input for smoother braking when changing direction
 				}
 			}
		}

		// Update the wheel visuals
		private void UpdateWheels()
		{
			// Update the position and rotation of each left wheel mesh to match the corresponding left wheel collider
			foreach (var leftWheel in leftWheels)
			{
				// Get the Left Wheel collider's world pose values and
				// use them to set the left wheel model's position and rotation
				leftWheel.wheelCollider.GetWorldPose(out leftWheelPosition, out leftWheelRotation);
				leftWheel.wheelMesh.transform.position = leftWheelPosition;
				leftWheel.wheelMesh.transform.rotation = leftWheelRotation;
			}

			// Update the position and rotation of each right wheel mesh to match the corresponding right wheel collider
			foreach (var rightWheel in rightWheels)
			{
				// Get the Right Wheel collider's world pose values and
				// use them to set the right wheel model's position and rotation
				rightWheel.wheelCollider.GetWorldPose(out rightWheelPosition, out rightWheelRotation);
				rightWheel.wheelMesh.transform.position = rightWheelPosition;
				rightWheel.wheelMesh.transform.rotation = rightWheelRotation;
			}			
		}
	}
}
