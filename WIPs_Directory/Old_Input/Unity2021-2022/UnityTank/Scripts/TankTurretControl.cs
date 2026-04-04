/*
 * UnityTank: TankTurretControl.cs
 * Version: Unity 2021-2022+ (Old Input)
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

        // Store rotation and lift input values
        private float rotationInput;
        private float liftInput;

        // Store the mouse input as a Vector2 for easier handling
        private Vector2 moveInput;

        // Update is called once per frame
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
                if (Input.GetMouseButtonDown(0)) // Left mouse button to lock the cursor
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
    }
}
