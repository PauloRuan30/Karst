using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationArrowController : MonoBehaviour
{
    public GameObject target;
    public float arrowDistance = 0.3f;
    public float rotationSpeed = 5f;
    public float colorChangeSpeed = 2f;

    private Transform cameraTransform;
    private Renderer arrowRenderer;
    private Material arrowMaterial;

    [Header("Debug")]
    public bool showDebugInfo = true;
    [SerializeField] private float currentAngle;
    [SerializeField] private Color currentColor;

    void Start()
    {
        if (GetComponent<Renderer>() == null)
        {
            Debug.LogError("This script should be attached to an object with a Renderer component.");
            return;
        }

        // Use the main camera as the VR camera
        cameraTransform = Camera.main.transform;
        arrowRenderer = GetComponent<Renderer>();

        arrowMaterial = new Material(arrowRenderer.material);
        arrowRenderer.material = arrowMaterial;
    }

    void Update()
    {
        if (target == null || cameraTransform == null)
        {
            return;
        }

        // Position the arrow in front of the VR camera
        Vector3 arrowPosition = cameraTransform.position + cameraTransform.forward * arrowDistance + Vector3.up*0.2f;
        transform.position = arrowPosition;

        // Calculate direction to target
        Vector3 directionToTarget = (target.transform.position - cameraTransform.position).normalized;

        // Project both vectors onto the horizontal plane
        Vector3 flatCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 flatDirectionToTarget = Vector3.ProjectOnPlane(directionToTarget, Vector3.up).normalized;

        // Calculate the angle between the camera's forward direction and direction to target
        currentAngle = Vector3.Angle(flatCameraForward, flatDirectionToTarget);

        // Determine if the target is to the left or right of the forward direction
        if (Vector3.Dot(cameraTransform.right, flatDirectionToTarget) < 0)
        {
            currentAngle = 360 - currentAngle;
        }

        // Create a rotation towards the target
        Quaternion targetRotation = Quaternion.LookRotation(flatDirectionToTarget, Vector3.up);

        // Apply an additional 180-degree rotation around the up axis
        targetRotation *= Quaternion.Euler(0, 180, 0);

        // Smoothly rotate the arrow towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Normalize the angle (0 to 360) to a 0 to 1 range for color interpolation
        float t = Mathf.Clamp01(Mathf.Abs(180 - currentAngle) / 180f);

        // Interpolate between green (accurate) and red (inaccurate)
        Color targetColor = Color.Lerp(Color.red, Color.green, t);

        // Smoothly change the color
        arrowMaterial.color = Color.Lerp(arrowMaterial.color, targetColor, colorChangeSpeed * Time.deltaTime);

        // Update debug info
        currentColor = arrowMaterial.color;

        if (showDebugInfo)
        {
            Debug.Log($"Angle: {currentAngle}, Color: {currentColor}");
        }
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    private void OnDestroy()
    {
        if (arrowMaterial != null)
        {
            Destroy(arrowMaterial);
        }
    }

}
