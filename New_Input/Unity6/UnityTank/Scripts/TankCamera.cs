/*
 * UnityTank: TankCamera.cs
 * Version: Unity 6+
 * Edits By: DeathwatchGaming
 * License: MIT
 * Description: This script is responsible for controlling the tank camera. It follows the tank's position and rotates to match the tank's velocity vector, giving a dynamic and immersive camera experience. The camera smoothly transitions between different angles and heights based on the tank's movement, providing a cinematic feel while maintaining visibility of the surroundings.
 */

// Import necessary namespaces
using UnityEngine;

// Define the namespace for the script
namespace UnityTank.Scripts
{
    public class TankCamera : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The tank rigidbody.")]
        // Note: This reference is used to access the tank's velocity, which is essential for calculating the camera's rotation based on the tank's movement.
        [SerializeField] private Rigidbody tankRigidbody;
        [Tooltip("The tank transform.")]
        // Note: This is the parent transform of the camera root, which is used to follow the tank's position.
        [SerializeField] private Transform tankTransform;
        [Tooltip("The tank camera root transform.")]
        // Note: This is the transform that the camera will follow, and it is detached from the tank to allow for smooth movement.
        [SerializeField] private Transform cameraRoot;
        [Tooltip("The tank camera transform.")]
        // Note: This is the actual camera transform that will be moved and rotated to follow the tank.
        [SerializeField] private Transform cameraTransform;

        [Header("Settings")]
        [Tooltip("If tank speed is below this value, then the camera will default to looking forward.")]
        // Note: This threshold prevents the camera from rotating erratically when the tank is stationary or moving very slowly, ensuring a stable viewing experience.
        [SerializeField] private float rotationThreshold = 1.0f;
        [Tooltip("How closely the camera matches the tank's velocity vector. The lower the value, the smoother the camera rotations, but too much results in not being able to see where you're going.")]
        // Note: This setting controls the responsiveness of the camera's rotation to the tank's movement. A higher value will make the camera more responsive to changes in direction, while a lower value will create a smoother, more cinematic effect.
        [SerializeField] private float rotationSpeed = 5.0f;
        [Tooltip("The camera distance determines how closely the camera follows the tank's position.")]
        // Note: This distance is used to calculate the camera's position relative to the tank, allowing for a better view of the surroundings and the tank itself.
        [SerializeField] private float distance = 10.0f;
        [Tooltip("The camera height determines the height of the camera.")]
        // Note: This height is added to the tank's position to determine the vertical position of the camera, allowing for a better view of the surroundings and the tank itself.
        [SerializeField] private float height = 3.0f;
        [Tooltip("The rotation damping.")]
        // Note: This damping factor controls how quickly the camera rotates to match the tank's velocity vector. A higher value results in faster rotation, while a lower value creates a smoother, more gradual rotation.
        [SerializeField] private float rotationDamping = 3.0f;
        [Tooltip("The height damping.")]
        // Note: This damping factor controls how quickly the camera height changes to match the desired height. A higher value results in faster height changes, while a lower value creates a smoother, more gradual height change.
        [SerializeField] private float heightDamping = 2.0f;

        // Private variables for internal use
        // Note: These variables are used to store the calculated rotation, desired position, tank velocity, and height values that are used in the camera's movement and rotation logic.
        private Quaternion lookRotation;
        private Vector3 desiredPosition;
        private Vector3 tankVelocity;
        private float desiredHeight;
        private float currentHeight;
        private float smoothedHeight;

        // Awake is called when a script instance is being loaded
        private void Awake()
        {
            // Initialize references to the tank's rigidbody, transform, camera root, and camera transform. This setup allows the camera to access the necessary components to follow and rotate based on the tank's movement.
            tankRigidbody = tankTransform.GetComponent<Rigidbody>();
            tankTransform = cameraRoot.parent.GetComponent<Transform>();
            cameraRoot = GetComponent<Transform>();
            cameraTransform = Camera.main.GetComponent<Transform>();
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Detach the camera root from the tank to allow for smooth movement and independent rotation. This ensures that the camera can follow the tank's position while still being able to rotate freely based on the tank's velocity.
            cameraRoot.parent = null;
        }

        // FixedUpdate is called at a fixed time interval
        private void FixedUpdate()
        {
            // Moves the camera to match the tank's position
            cameraRoot.position = Vector3.Lerp(cameraRoot.position, tankTransform.position, distance * Time.fixedDeltaTime);

            // Calculate the desired rotation based on the tank's velocity
            tankVelocity = tankRigidbody.linearVelocity;

            // If the tank isn't moving, default to looking forwards
            // Prevents camera from freaking out with a zero velocity getting put into a Quaternion.LookRotation
            if (tankVelocity.magnitude < rotationThreshold)
            {
                lookRotation = Quaternion.LookRotation(tankTransform.forward);
            }

            else
            {
                lookRotation = Quaternion.LookRotation(tankVelocity.normalized);
            }

            // Rotates the camera towards the velocity vector
            // Smoothly rotate the camera towards the desired rotation
            lookRotation = Quaternion.Slerp(cameraRoot.rotation, lookRotation, rotationSpeed * Time.fixedDeltaTime);
            cameraRoot.rotation = lookRotation;

            // Calculate the desired height and position of the camera
            desiredHeight = tankTransform.position.y + height;
            currentHeight = cameraTransform.position.y;
            smoothedHeight = Mathf.Lerp(currentHeight, desiredHeight, heightDamping * Time.fixedDeltaTime);

            desiredPosition = tankTransform.position - cameraRoot.forward * distance;
            desiredPosition.y = smoothedHeight;

            // Smoothly move the camera to the desired position
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, rotationDamping * Time.fixedDeltaTime);
        }
    }
}
