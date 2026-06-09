using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    private Camera mainCamera;
    public LayerMask groundLayer; // Layer mask to specify which layers constitute the ground
    public float maxRaycastDistance = 100f; // Maximum distance for the raycast


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 newPosition = mainCamera.transform.position;

            // Calculate the direction towards the camera's forward on the horizontal plane (Y-axis)
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0; // Zero out the Y component to ensure no vertical rotation
            cameraForward.Normalize(); // Normalize to make it a unit vector

            // Set the object's forward direction to the horizontal direction
            transform.forward = cameraForward;

            // Perform a sphere cast to determine the ground height
            RaycastHit hit;
            float sphereRadius = 0.5f; // Set this to the desired radius of the sphere

            if (Physics.SphereCast(newPosition, sphereRadius, Vector3.down, out hit, maxRaycastDistance, groundLayer))
            {
                newPosition.y = hit.point.y; // Set the y position to the ground height
            }
            else
            {
                Debug.LogWarning("Ground not found within sphere cast distance.");
                // Handle case where ground is not found, e.g., set to a default height or keep current height
                // newPosition.y = someDefaultHeight;
            }

            transform.position = newPosition + Vector3.up*1.2f;
        }
    }
}
