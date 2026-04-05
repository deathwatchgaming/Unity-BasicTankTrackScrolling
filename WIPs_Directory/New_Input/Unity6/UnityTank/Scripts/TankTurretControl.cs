/*
 * UnityTank: TankTurretControl.cs
 * Version: Unity 6+ (New Input)
 * Edits By: DeathwatchGaming
 * License: MIT
 * Description: This script controls the rotation of the tank turret and the lifting of the barrel based on mouse input.
 */

// Import necessary namespaces
using UnityEngine;

// Define the namespace for the script
namespace UnityTank.Scripts
{
    public class TankTurretControl : MonoBehaviour
    {
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

        // Store the current and target angles for the barrel
        private float currentAngle = 0f;
        private float targetAngle = 0f;

        private TankInputActions tankControls; // Reference to the new input system

        // Store mouse input for rotation and lifting
        private Vector2 mouseInputVector;

        // Store rotation and lift input values
        private float rotationInput;
        private float liftInput;

        // Awake is called when the script instance is being loaded
        private void Awake()
        {
            // Initialize the new input system
            tankControls = new TankInputActions();           
        }

        // OnEnable is called when the object becomes enabled and active
        private void OnEnable()
        {
            // Enable the new input system when the script is enabled
            tankControls.Enable();
        }

        // OnDisable is called when the behaviour becomes disabled or inactive
        private void OnDisable()
        {
            // Disable the new input system when the script is disabled
            tankControls.Disable();
        }

        // Update is called once per frame
        private void Update()
        {
            // Unlock the cursor and make it visible when the Escape key is pressed
            if (Keyboard.current.escapeKey.wasPressedThisFrame) // equivalent to Input.GetKeyDown(KeyCode.Escape) // Escape key is used to unlock the cursor and make it visible
            {
                // Unlock the cursor and make it visible for UI interaction or exiting
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Lock the cursor again when the right mouse button is pressed while the cursor is unlocked 
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Mouse.current.rightButton.wasPressedThisFrame) // equivalent to Input.GetMouseButtonDown(1) // Right mouse button is used to lock the cursor again
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
        }

        // FixedUpdate is called at a fixed interval and is independent of frame rate
        private void FixedUpdate()
        {
            // Rotate the turret and lift the barrel based on mouse input
            RotateTurret();
            LiftBarrel();
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
    }
}
