/*
 * UnityTank: TankControl.cs
 * Version: Unity 6+ (Old Input)
 * Edits By: DeathwatchGaming
 * License: MIT
 * Description: This script controls the movement and behavior of a tank, including motor torque, braking, and wheel visuals. It uses Unity's WheelCollider components for realistic physics-based movement and handling. The script is designed to be flexible and customizable, allowing you to adjust properties such as motor torque, brake torque, maximum speed, and center of gravity to achieve the desired performance and handling characteristics for your tank. The script also includes comments and tooltips to help you understand how to set up and use the various properties and components effectively.
 */

// Note: Make sure to set up the wheel colliders and meshes correctly in the Unity Editor, and adjust the properties based on your tank's design and performance requirements for optimal results.
// Note: This script is designed for use with Unity's old input system. If you are using the new Input System, you will need to modify the input handling code accordingly.
// Note: For each wheel collider you need to edit the "Transform" for the "Position Y" and add "+0.15" to the value of what the wheel mesh value is. This is to ensure that the wheel mesh is visually aligned with the wheel collider for proper visuals during movement. Adjust this value as needed based on your specific tank model and wheel setup.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTank.Scripts
{
    public class TankControl : MonoBehaviour
    {
        [Serializable]
        public struct Track
        {            
            [Tooltip("The mesh renderer.")]
            // Note: Each track should have a mesh renderer with a material that supports texture offset for proper scrolling effect. The track meshes should be set up to visually represent the tank's tracks and should be aligned with the wheel colliders for accurate visuals during movement.
            public MeshRenderer meshRenderer;
            // Note: The left and right track booleans are used to determine which offset to apply for texture scrolling based on the tank's turning direction. Make sure to mark the appropriate track as left or right in the Unity Editor for correct behavior.
            [Tooltip("The bool to mark as left track.")]
            public bool left;
            [Tooltip("The bool to mark as right track.")]
            public bool right;
        }

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

        [Header("Tank Tracks")]
        [Tooltip("The track elements.")]
        // Create a List of Tracks ie: Tracks and start with 2 track elements ie: [0,1]
        [SerializeField] private List<Track> tracks = new List<Track>(new Track[2]);

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

        [Header("Turret Control")]
        [Tooltip("Transform of the turret to rotate.")]
        // Reference to the turret's transform for rotation
        [SerializeField] private Transform turretTransform;
        [Tooltip("Speed at which the turret rotates.")]
        // Speed at which the turret rotates based on mouse input
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Barrel Control")]
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
        // Scroll the main texture based on time
        [SerializeField] private float scrollSpeed = 0.05f;

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

        // Store the current and target angles for the barrel
        private float currentAngle = 0f;
        private float targetAngle = 0f;

        // Store rotation and lift input values
        private float rotationInput;
        private float liftInput;

        // Store the mouse input as a Vector2 for easier handling
        private Vector2 moveInput;

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
            // Check if the Escape key is pressed to unlock the cursor and make it visible
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Unlock the cursor and make it visible when the Escape key is pressed
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Check if the cursor is not locked, and if so, check for mouse input to lock it again
            else if (Cursor.lockState == CursorLockMode.None)
            {
                // If the cursor is not locked, check for mouse input to lock it again
                if (Input.GetMouseButtonDown(1)) // Right mouse button to lock the cursor
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // If the cursor is locked, get mouse input for controlling the turret and barrel
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                // If the cursor is locked, get mouse input for controlling the turret and barrel
                GetMouseInput();
            }

            // Update the wheel visuals in Update for smooth animation, while physics is handled in FixedUpdate
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
            // Get player input for acceleration and steering
            motorInput = Input.GetAxis("Vertical"); // Forward / backward input
            steerInput = Input.GetAxis("Horizontal"); // Steering input			
        }

        // Method to handle movement logic, including speed calculation and motor torque adjustment
        private void DoMovement()
        {
            // Get player input for movement
            GetMovementInput();

            // Calculate current speed along the tank's forward axis
            forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
            speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed)); // Normalized speed factor

            // Reduce motor torque at high speeds for better handling
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
            // Apply motor torque and steering to the left wheels
            foreach (var leftWheel in leftWheels)
            {
                // Apply motor torque and steering based on player input and current speed
                if (isAccelerating)
                {
                    // Apply torque to motorized left wheels
                    if (leftWheel.motorized)
                    {
                        // Apply motor torque based on player input and current speed factor
                        leftWheel.wheelCollider.motorTorque = motorInput * currentMotorTorque;
                        leftWheel.wheelCollider.motorTorque += motorTorque * steerInput; // Apply steering torque (positive for left, negative for right)

                        // Debug logs to check input values and motor torque
                        //Debug.Log($"Motor Input: {motorInput}, Steer Input: {steerInput}, Current Motor Torque: {currentMotorTorque}");
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
                // Apply motor torque and steering based on player input and current speed
                if (isAccelerating)
                {
                    // Apply torque to motorized right wheels
                    if (rightWheel.motorized)
                    {
                        // Apply motor torque based on player input and current speed factor
                        rightWheel.wheelCollider.motorTorque = motorInput * currentMotorTorque;
                        rightWheel.wheelCollider.motorTorque -= motorTorque * steerInput; // Apply steering torque (negative for right, positive for left)

                        // Debug logs to check input values and motor torque
                        //Debug.Log($"Motor Input: {motorInput}, Steer Input: {steerInput}, Current Motor Torque: {currentMotorTorque}"); 
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

        // Method to get mouse input for rotating the turret and lifting the barrel
        private void GetMouseInput()
        {
            // Get mouse input for rotating the turret and lifting the barrel
            moveInput.x = Input.GetAxis("Mouse X");
            moveInput.y = Input.GetAxis("Mouse Y");

            // Get the horizontal mouse input for rotating the turret
            rotationInput = moveInput.x;

            // Get the vertical mouse input for lifting the barrel
            liftInput = moveInput.y;
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
            // Invert the lift input if the option is enabled
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