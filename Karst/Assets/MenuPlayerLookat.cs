using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPlayerLookat : MonoBehaviour
{
    public Transform playerCamera;
    public float distanceFromPlayer = 4f; // Distance to place the menu from the player
    public float minDistanceToReposition = 50f; // Minimum distance to trigger a reposition
    private Vector3 previousCameraPosition;
    private Quaternion previousCameraRotation;

    private void Start()
    {
        // Initialize the previous camera position and rotation
        previousCameraPosition = playerCamera.position;
        previousCameraRotation = playerCamera.rotation;
    }

    private void Update()
    {
        // Check if the camera has moved significantly
        if (Vector3.Distance(playerCamera.position, previousCameraPosition) > minDistanceToReposition ||
            Quaternion.Angle(playerCamera.rotation, previousCameraRotation) > minDistanceToReposition)
        {
            PositionMenu();

            // Update the previous camera position and rotation
            previousCameraPosition = playerCamera.position;
            previousCameraRotation = playerCamera.rotation;
        }
    }

    private void PositionMenu()
    {
        // Set the position of the canvas in front of the player
        Vector3 newPosition = playerCamera.position + (playerCamera.forward * distanceFromPlayer);
        newPosition.y = transform.position.y;
        transform.position = newPosition;

        // Rotate the canvas to face the player
        transform.LookAt(playerCamera);
        transform.forward = -transform.forward;
        //transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0); // Optional: only rotate around the Y-axis
    }
}
