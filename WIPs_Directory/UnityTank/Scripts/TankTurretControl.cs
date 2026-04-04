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
            // Ensure the turret and barrel transforms are assigned
            if (turretTransform == null || barrelTransform == null)
            {
                Debug.LogWarning("Turret or Barrel Transform is not assigned.");
                return;
            }

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

        // FixedUpdate is called at a fixed interval and is independent of frame rate
        private void FixedUpdate()
        {
            // Get mouse input for rotation and lifting
            mouseInputVector = tankControls.Tank.TurretMovement.ReadValue<Vector2>();

            // Rotate the turret and lift the barrel based on mouse input
            RotateTurret();
            LiftBarrel();
        }

        // Method to rotate the turret based on mouse input
        private void RotateTurret()
        {
            // Get the horizontal mouse input for rotating the turret
            rotationInput = mouseInputVector.x;

            // Rotate the turret based on mouse input
            turretTransform.Rotate(0, rotationInput * rotationSpeed * Time.fixedDeltaTime, 0);
        }

        // Method to lift the barrel based on mouse input
        private void LiftBarrel()
        {
            // Get the vertical mouse input for lifting the barrel
            liftInput = mouseInputVector.y;

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
