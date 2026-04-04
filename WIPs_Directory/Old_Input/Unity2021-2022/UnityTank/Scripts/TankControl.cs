/*
 * UnityTank: TankControl.cs
 * Version: Unity 2021-2022 (Old Input)
 * Edits By: DeathwatchGaming
 * License: MIT
 */

using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityTank.Scripts
{
	public class TankControl : MonoBehaviour
	{
		[Serializable]
		public struct LeftWheel
		{
			[Tooltip("The wheel collider component for the left wheel.")]
			// Note: The wheel collider should be set up with the correct radius and suspension settings for proper behavior
			public WheelCollider wheelCollider;
			[Tooltip("The transform component for the left wheel mesh.")]
			// Note: The wheel mesh should be a child of the tank and properly aligned with the wheel collider for correct visuals
			public Transform wheelMesh;
			[Tooltip("Whether this left wheel is motorized and receives torque.")]
			// Note: Only motorized wheels will receive torque from the motor input, allowing for different drive configurations (e.g., front-wheel drive, rear-wheel drive, or all-wheel drive)
			public bool motorized;
		}

		[Serializable]
		public struct RightWheel
		{
			[Tooltip("The wheel collider component for the right wheel.")]
			// Note: The wheel collider should be set up with the correct radius and suspension settings for proper behavior
			public WheelCollider wheelCollider;
			[Tooltip("The transform component for the right wheel mesh.")]
			// Note: The wheel mesh should be a child of the tank and properly aligned with the wheel collider for correct visuals
			public Transform wheelMesh;
			[Tooltip("Whether this right wheel is motorized and receives torque.")]
			// Note: Only motorized wheels will receive torque from the motor input, allowing for different drive configurations (e.g., front-wheel drive, rear-wheel drive, or all-wheel drive)
			public bool motorized;
		}

		[Header("Track Wheels")]
		[Tooltip("The left wheels.")]
		// Note: The left wheels should be in the same order as the right wheels for proper steering and torque application
		[SerializeField] private List<LeftWheel> leftWheels = new List<LeftWheel>(new LeftWheel[9]);
		[Tooltip("The right wheels.")]
		// Note: The right wheels should be in the same order as the left wheels for proper steering and torque application
		[SerializeField] private List<RightWheel> rightWheels = new List<RightWheel>(new RightWheel[9]);

		[Header("Tank Properties")]
		[Tooltip("The rigidbody mass amount.")]
		// Note: The mass of the rigidbody affects the tank's acceleration, handling, and interaction with physics. A heavier mass will result in slower acceleration but better stability, while a lighter mass will allow for quicker acceleration but may be more prone to tipping over. Adjust this value based on the desired feel and performance of the tank.
		[SerializeField] private float rigidBodyMass = 9000f;
		[Tooltip("The center of gravity offset amount.")]
		// Note: Adjusting the center of gravity can help improve the tank's stability and prevent it from rolling over during sharp turns or when traversing uneven terrain. A negative offset will lower the center of gravity, while a positive offset will raise it. Experiment with different values to find the optimal balance for your tank's design and performance.
		[SerializeField] private float centerOfGravityOffset = -1f;
		[Tooltip("The motor torque amount.")]
		// Note: The motor torque determines how much force is applied to the wheels when accelerating. A higher motor torque will result in faster acceleration, while a lower motor torque will provide more gradual acceleration. Adjust this value based on the desired performance and handling characteristics of the tank.
		[SerializeField] private float motorTorque = 2000f;
		[Tooltip("The brake torque amount.")]
		// Note: The brake torque determines how much force is applied to the wheels when braking. A higher brake torque will result in stronger braking, while a lower brake torque will provide more gradual deceleration. Adjust this value based on the desired braking performance and handling characteristics of the tank.
		[SerializeField] private float brakeTorque = 2000f;
		[Tooltip("The maximum speed amount.")]
		// Note: The maximum speed limits how fast the tank can go. A lower maximum speed will result in a slower top speed, while a higher maximum speed will allow the tank to reach faster speeds. Adjust this value based on the desired performance and handling characteristics of the tank, as well as the scale of your game world.
		[SerializeField] private float maxSpeed = 10f;

		// Private variables for internal use
	    
		private Rigidbody rigidBody;
		private Vector3 centerOfMass;
		private Vector3 leftWheelPosition;
		private Vector3 rightWheelPosition;
		private Quaternion leftWheelRotation;
		private Quaternion rightWheelRotation;

		private float motorInput;
		private float steerInput;
		private float forwardSpeed;
		private float speedFactor;
		private float currentMotorTorque;
		private bool isAccelerating;

		// Start is called before the first frame update
		private void Start()
		{
			// Get the rigidbody
			rigidBody = GetComponent<Rigidbody>();

			// Set the rigidbody mass
			rigidBody.mass = rigidBodyMass;

			// Adjust center of mass to improve stability and prevent rolling
			centerOfMass = rigidBody.centerOfMass;
			centerOfMass.y += centerOfGravityOffset;
			rigidBody.centerOfMass = centerOfMass;
		}

		// Update is called every frame
		private void Update()
		{
			UpdateWheels();
		}

		// FixedUpdate is called at a fixed time interval
		private void FixedUpdate()
		{
			// Get player input for acceleration and steering
			motorInput = Input.GetAxis("Vertical"); // Forward / backward input
			steerInput = Input.GetAxis("Horizontal"); // Steering input

			// Calculate current speed along the tank's forward axis
			forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);
			speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed)); // Normalized speed factor

			// Reduce motor torque at high speeds for better handling
			currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

			// Determine if the player is accelerating or trying to reverse
			isAccelerating = Mathf.Sign(motorInput) == Mathf.Sign(forwardSpeed);

			// Apply motor torque and steering to the left wheels
			foreach (var leftWheel in leftWheels)
			{
				if (isAccelerating)
				{
					// Apply torque to motorized leftwheels
					if (leftWheel.motorized)
					{
						leftWheel.wheelCollider.motorTorque = motorInput * currentMotorTorque;
						leftWheel.wheelCollider.motorTorque += motorTorque * steerInput; // Apply steering torque (positive for left, negative for right)
					}

					// Apply brakes when brake key is applied
					if (Input.GetKey(KeyCode.Space))
					{
						// Apply brakes
						leftWheel.wheelCollider.motorTorque = 0f;
						leftWheel.wheelCollider.brakeTorque = Mathf.Abs(motorInput) * brakeTorque;
					}

					else
					{
						// Release brakes when accelerating
						leftWheel.wheelCollider.brakeTorque = 0f;
					}					
 				}

 				else 
 				{
					// Apply brakes when reversing direction
					leftWheel.wheelCollider.motorTorque = 0f;
					leftWheel.wheelCollider.brakeTorque = Mathf.Abs(motorInput) * brakeTorque;
 				}
 			}

			// Apply motor torque and steering to the right wheels
			foreach (var rightWheel in rightWheels)
			{
				if (isAccelerating)
				{
					// Apply torque to motorized rightwheels
					if (rightWheel.motorized)
					{
						rightWheel.wheelCollider.motorTorque = motorInput * currentMotorTorque;
						rightWheel.wheelCollider.motorTorque -= motorTorque * steerInput; // Apply steering torque (negative for right, positive for left)
					}

					// Apply brakes when brake key is applied
					if (Input.GetKey(KeyCode.Space))
					{
						// Apply brakes
						rightWheel.wheelCollider.motorTorque = 0f;
						rightWheel.wheelCollider.brakeTorque = Mathf.Abs(motorInput) * brakeTorque;
					}

					else
					{
						// Release brakes when accelerating
						rightWheel.wheelCollider.brakeTorque = 0f;
					}					
 				}

 				else 
 				{
					// Apply brakes when reversing direction
					rightWheel.wheelCollider.motorTorque = 0f;
					rightWheel.wheelCollider.brakeTorque = Mathf.Abs(motorInput) * brakeTorque;
 				}
 			}
		}

		// Update the wheel visuals
		private void UpdateWheels()
		{
			foreach (var leftWheel in leftWheels)
			{
				// Get the Left Wheel collider's world pose values and
				// use them to set the left wheel model's position and rotation
				leftWheel.wheelCollider.GetWorldPose(out leftWheelPosition, out leftWheelRotation);
				leftWheel.wheelMesh.transform.position = leftWheelPosition;
				leftWheel.wheelMesh.transform.rotation = leftWheelRotation;
			}

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
