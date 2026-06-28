/*
 * UnityTank: TankControl.cs
 * Version: Unity 6+ (New Input)
 * Edits By: DeathwatchGaming
 * License: MIT
 * Description: This script controls the movement and behavior of the tank, including handling player input for acceleration, braking, and steering. It uses Unity's WheelCollider components to simulate the physics of the tank's tracks and wheels, allowing for realistic movement and interaction with the terrain. The script also updates the visual representation of the wheels to match their physical state, ensuring that the tank's appearance reflects its movement and interactions with the environment.
 */

// Note: Make sure to set up the wheel colliders and meshes correctly in the Unity Editor, and adjust the properties based on your tank's design and performance requirements for optimal results.
// Note: This script is designed for use with Unity's new input system. If you are using the old Input System, you will need to modify the input handling code accordingly.
// Note: For each wheel collider you need to edit the "Transform" for the "Position Y" and add "+0.15" to the value of what the wheel mesh value is. This is to ensure that the wheel mesh is visually aligned with the wheel collider for proper visuals during movement. Adjust this value as needed based on your specific tank model and wheel setup.

// Import necessary namespaces
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Define the namespace for the tank control script
namespace UnityTank.Scripts
{
	public class TankControl : MonoBehaviour
	{
        [Serializable]
        public struct Track
        {            
            [Tooltip("The mesh renderer.")]
            // Note: The mesh renderer should be assigned in the Unity Editor to the corresponding track mesh for proper texture scrolling and visual representation of the tank's tracks.
            public MeshRenderer meshRenderer;
            // Note: The left and right booleans should be set in the Unity Editor to indicate which track is the left track and which is the right track for proper texture scrolling and visual representation of the tank's tracks.
            [Tooltip("The bool to mark as left track.")]
            public bool left;
            [Tooltip("The bool to mark as right track.")]
            public bool right;
        }

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

        [Header("Tank Tracks")]
        [Tooltip("The track elements.")]
        // Create a List of Tracks ie: Tracks and start with 2 track elements ie: [0,1]
        [SerializeField] private List<Track> tracks = new List<Track>(new Track[2]);

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

        [Header("Turret Properies")]
        [Tooltip("Transform of the turret to rotate.")]
        // Reference to the turret's transform for rotation
        [SerializeField] private Transform turretTransform;
        [Tooltip("Speed at which the turret rotates.")]
        // Speed at which the turret rotates based on mouse input
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Barrel Properties")]
        [Tooltip("Transform of the barrel to lift.")]
        // Reference to the barrel's transform for lifting up and down
        [SerializeField] private Transform barrelTransform;
        [Tooltip("Speed at which the barrel lifts up and down.")]
        // Speed at which the barrel lifts based on mouse input
        [SerializeField] private float liftSpeed = 5f;
        [Tooltip("Maximum angle the barrel can lift up (in degrees).")]
        // Maximum angle the barrel can lift up from its initial position
        [SerializeField] private float maxLiftAngle = 1.5f;
        [Tooltip("Minimum angle the barrel can lift down (in degrees).")]
        // Minimum angle the barrel can lift down from its initial position
        [SerializeField] private float minLiftAngle = -10f;
        [Tooltip("Invert the vertical mouse input for lifting the barrel.")]
        // Option to invert the vertical mouse input for lifting the barrel
        [SerializeField] private bool invertMouseY = false;

        [Header("Track Properties")]
        [Tooltip("Set if using shader graph.")]
        // Bool for if using shader graph
        [SerializeField] private bool usingShaderGraph = true;
        [Tooltip("Set the main texture for shader graph.")]
        // String for setting the main texture for shader graph
        [SerializeField] private string setMainTexture = "_Albedo_Map";
        [Tooltip("The scroll speed.")]
        // Scroll the main texture based on the tank's forward speed and this scroll speed multiplier to create the illusion of the tracks moving as the tank moves. Adjusting this value will affect how fast the track textures scroll, which can help match the visual effect to the actual movement of the tank.
        [SerializeField] private float scrollSpeed = 0.05f;

        // Private variables for internal use
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

        // Store the current and target angles for the barrel
        private float currentAngle = 0f;
        private float targetAngle = 0f;

        // Store mouse input for rotation and lifting
        private Vector2 mouseInputVector;

        // Store rotation and lift input values
        private float rotationInput;
        private float liftInput;

        // Variables to track the current offset for left and right tracks
        private float leftTrackOffset = 0f;
        private float rightTrackOffset = 0f;

        // Variable to store the current offset value for track texture scrolling
        private float currentOffset;

        // Variable to store the base scroll value calculated from the tank's forward speed and scroll speed
        private float baseScroll = 0f;

        // Variables to track the turning direction for left and right turns
        private bool turningLeft;
        private bool turningRight;

        // Variables to store the scroll values for left and right tracks based on turning direction
        private float leftScroll;
        private float rightScroll;

        // Variables to track the turning state for left and right turns
        private bool isTurnLeft;
        private bool isTurnRight;

        // Variable to store the offset vector for track texture scrolling
        private Vector2 offsetVector;

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
            // Unlock the cursor and make it visible when the keyboard escape key is pressed
            if (Keyboard.current.escapeKey.wasPressedThisFrame) // equivalent to Input.GetKeyDown(KeyCode.Escape) // Escape is used to unlock the cursor
            {
                // Unlock the cursor and make it visible for UI interaction or exiting
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Lock the cursor again when the left mouse button is pressed while the cursor is unlocked 
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Mouse.current.rightButton.wasPressedThisFrame) // equivalent to Input.GetMouseButtonDown(1) // Right mouse button is used to lock the cursor
                {
                    // Lock the cursor to the center of the screen and hide it for better control
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // Check if the cursor is locked before processing input
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                // If the cursor is locked, get mouse input for controlling the turret and barrel
                GetMouseInput();
            }

            // Check braking button value
            isBrakingKey = tankControls.Tank.Brake.IsPressed();

            // Update the wheel visuals to match the physical state of the wheels
            UpdateWheels();

            // Call the method to handle track scrolling based on turning direction
            DoTrackScrolling();
        }

		// FixedUpdate is called at a fixed time interval
		private void FixedUpdate()
		{
            // Handle movement and wheel control in FixedUpdate for consistent physics behavior
            DoMovement();
            DoWheelControl();

            // Rotate the turret and lift the barrel based on mouse input
            RotateTurret();
            LiftBarrel();
		}

        // Method to get player input for movement
        private void GetMovementInput()
        {
			// Read the Vector2 input from the new Input System
			inputVector = tankControls.Tank.Movement.ReadValue<Vector2>();

			// Get player input for acceleration
			motorInput = inputVector.y; // Forward / backward input
			steerInput = inputVector.x; // Steering input		
        }

        // Method to handle movement logic, including speed calculation and motor torque adjustment
        private void DoMovement()
        {
            // Get player input for movement
            GetMovementInput();

			// Calculate current speed along the tank's forward axis
			forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity); // Speed in the forward direction
			speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed)); // Normalized speed factor

			// Reduce motor torque and steering at high speeds for better handling
			currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

			// Determine if the player is accelerating or trying to reverse
			isAccelerating = Mathf.Sign(motorInput) == Mathf.Sign(forwardSpeed);

            // Determine turning direction based on steer input
            isTurnLeft = steerInput < 0f;
            isTurnRight = steerInput > 0f;
        }

        // Method to apply motor torque and steering to the wheels
        private void DoWheelControl()
        {
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

						// Debug logs to check input values and motor torque
						//Debug.Log($"Motor Input: {motorInput}, Steer Input: {steerInput}, Current Motor Torque: {currentMotorTorque}");
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

                // Apply braking when brake key is pressed and no movement input (neutral)
                if (isBrakingKey && Mathf.Approximately(motorInput, 0f))
                {
                    leftWheel.wheelCollider.motorTorque = 0f;
                    leftWheel.wheelCollider.brakeTorque = brakeTorque;
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

                        // Debug logs to check input values and motor torque
                        //Debug.Log($"Motor Input: {motorInput}, Steer Input: {steerInput}, Current Motor Torque: {currentMotorTorque}");
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
                
                // Apply braking when brake key is pressed and no movement input (neutral)
                if (isBrakingKey && Mathf.Approximately(motorInput, 0f))
                {
                    rightWheel.wheelCollider.motorTorque = 0f;
                    rightWheel.wheelCollider.brakeTorque = brakeTorque;
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

        // Method to get mouse input for rotating the turret and lifting the barrel
        private void GetMouseInput()
        {
            // Read the Vector2 input from the new Input System

            // Get mouse input for rotation and lifting
            mouseInputVector = tankControls.Tank.TurretMovement.ReadValue<Vector2>();

            // Store the horizontal mouse input for rotating the turret
            rotationInput = mouseInputVector.x;

            // Store the vertical mouse input for lifting the barrel
            liftInput = mouseInputVector.y;
        }

        // Method to rotate the turret based on mouse input
        private void RotateTurret()
        {
            // Rotate the turret based on mouse input
            turretTransform.Rotate(0, rotationInput * rotationSpeed * Time.fixedDeltaTime, 0);
        }

        // Method to lift the barrel based on mouse input
        private void LiftBarrel()
        {
            // Invert the input if the option is enabled
            liftInput = invertMouseY ? -liftInput : liftInput; 

            // Calculate the new angle for the barrel
            currentAngle = barrelTransform.localEulerAngles.x;

            // Convert the angle to a range of -180 to 180 for easier clamping
            if (currentAngle > 180)
            {
                currentAngle -= 360; // Convert to -180 to 180 range
            }

            // Calculate the target angle based on input and clamp it within the specified limits
            targetAngle = Mathf.Clamp(currentAngle + liftInput * liftSpeed * Time.fixedDeltaTime, minLiftAngle, maxLiftAngle);

            // Apply the new angle to the barrel
            barrelTransform.localEulerAngles = new Vector3(targetAngle, 0, 0);
        }

        // Method to handle track scrolling based on turning direction and whether using shader graph or not
        private void DoTrackScrolling()
        {
            // Use signed forward speed for natural forward/reverse scrolling
            baseScroll = forwardSpeed * scrollSpeed * Time.deltaTime;

            // Determine turn direction from steerInput (more reliable than key presses)
            turningLeft = steerInput < 0f; // Determine if the player is turning left based on steer input
            turningRight = steerInput > 0f; // Determine if the player is turning right based on steer input

            // Apply inversion for tank-like track behavior during turns
            leftScroll = baseScroll;
            rightScroll = baseScroll;

            // If turning left, the left track should reverse to create the effect of the tank pivoting in place
            if (turningLeft)
            {
                leftScroll = -baseScroll; // Left track reverses
            }

            // If turning right, the right track should reverse to create the effect of the tank pivoting in place
            if (turningRight)
            {
                rightScroll = -baseScroll; // Right track reverses
            }

            // Accumulate offsets
            leftTrackOffset = (leftTrackOffset + leftScroll) % 1f; // Use modulo to wrap the offset within the range of 0 to 1 for seamless texture scrolling
            rightTrackOffset = (rightTrackOffset + rightScroll) % 1f; // Use modulo to wrap the offset within the range of 0 to 1 for seamless texture scrolling

            // Keep offsets positive
            if (leftTrackOffset < 0)
            {
                // If the left track offset is negative, add 1 to wrap it back into the positive range
                leftTrackOffset += 1f;
            }

            // If the right track offset is negative, add 1 to wrap it back into the positive range
            if (rightTrackOffset < 0)
            {
                // If the right track offset is negative, add 1 to wrap it back into the positive range
                rightTrackOffset += 1f;
            }

            // Update the track materials with the calculated offsets
            foreach (var track in tracks)
            {
                // Set the current offset based on whether it's the left or right track
                currentOffset = 0f;

                // Determine which offset to use based on whether the track is marked as left or right
                if (track.left)
                {
                    // If the track is marked as left, use the left track offset
                    currentOffset = leftTrackOffset;
                }

                // If the track is marked as right, use the right track offset    
                else if (track.right)
                {
                    // If the track is marked as right, use the right track offset
                    currentOffset = rightTrackOffset;
                }

                // Create the offset vector for the material based on the current offset value
                offsetVector = new Vector2(0f, currentOffset);

                // Set the texture offset for the track material based on whether using shader graph or not
                if (usingShaderGraph)
                {
                    // If using shader graph, set the texture offset using the specified main texture property name
                    track.meshRenderer.materials[0].SetTextureOffset(setMainTexture, offsetVector);
                }

                // If not using shader graph, set the texture offset using the default main texture property
                else
                {
                    // If not using shader graph, set the texture offset using the default main texture property
                    track.meshRenderer.materials[0].mainTextureOffset = offsetVector;
                }
            }
        }
    }
}
