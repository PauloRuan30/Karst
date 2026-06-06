using System;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.IO;

public class Draw : MonoBehaviour
{
    [Header("Pen Properties")]
    [SerializeField] private GameObject tip;
    [SerializeField] private Material drawingMaterial;
    [SerializeField] private Material whiteMaterial;

    [Range(0.01f, 0.1f)]
    public float penWidth = 0.03f;
    [SerializeField][Range(0.01f, 0.5f)] private float DrawOffset = 0.1f;
    [SerializeField] private Camera cam;
    [SerializeField] float movementThreshold = 0.5f; // Threshold for player movement
    [SerializeField] GameObject playerModel;
    private LineRenderer currentDrawing = null;
    private CastSystem castSystem;
    private Vector3 startDrawingPosition;

    public static System.Threading.SynchronizationContext syncContext;
    // for Habib ************************************************
    //private InputAction triggerAction;
    //private bool triggerButtonPressed;
    //private void OnEnable()
    //{
    //    // Setup the input action for the trigger button
    //    triggerAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{RightHand}/trigger");
    //    triggerAction.performed += ctx => triggerButtonPressed = true;
    //    triggerAction.canceled += ctx => triggerButtonPressed = false;
    //    triggerAction.Enable();
    //}

    //private void OnDisable()
    //{
    //    triggerAction.Disable();
    //    triggerAction.Dispose();
    //}
    //// Update for vr simulater
    //void Update()
    //{
    //    if (triggerButtonPressed)
    //    {
    //        draw();
    //    }
    //    else
    //    {
    //        if (currentDrawing != null)
    //        {
    //            currentDrawing.material = whiteMaterial;
    //            TakePicture.SavePicture(cam, currentDrawing);
    //            pythonConnector.SetDataToSend(TakePicture.GetLastPictureAsData());
    //            Destroy(currentDrawing.gameObject);
    //            currentDrawing = null;
    //        }
    //    }
    //}
    // ******************************************************************


    private void Start()
    {
        syncContext = System.Threading.SynchronizationContext.Current;
        castSystem = GetComponent<CastSystem>();
        pythonConnector.OnDataReceived += castSystem.PrepareSkill;
    }

    private void OnDestroy()
    {
        pythonConnector.OnDataReceived -= castSystem.PrepareSkill;
    }

    // Update is called once per frame
    void Update()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).IsPressed(InputHelpers.Button.TriggerButton, out bool rightJoystickButtonPressed);

        if (rightJoystickButtonPressed)
        {
            if (currentDrawing == null)
            {
                startDrawingPosition = playerModel.transform.position; // Store the starting position
            }

            // Check if player has moved beyond the threshold
            if (Vector3.Distance(playerModel.transform.position, startDrawingPosition) >= movementThreshold)
            {
                // Remove the current drawing
                if (currentDrawing != null)
                {
                    Destroy(currentDrawing.gameObject);
                    currentDrawing = null;
                }
                startDrawingPosition = playerModel.transform.position; // Reset the starting position
            }
            else
            {
                draw();
            }
        }
        else if (!rightJoystickButtonPressed)
        {
            if (currentDrawing != null)
            {
                currentDrawing.material = whiteMaterial;
                TakePicture.SavePicture(cam, currentDrawing);
                pythonConnector.SetDataToSend(TakePicture.GetLastPictureAsData());
                Destroy(currentDrawing.gameObject);
                currentDrawing = null;
            }
        }
    }

    void draw()
    {
        if (currentDrawing == null)
        {
            currentDrawing = new GameObject().AddComponent<LineRenderer>();
            currentDrawing.material = drawingMaterial;
            currentDrawing.startColor = currentDrawing.endColor = Color.black;
            currentDrawing.startWidth = currentDrawing.endWidth = penWidth;
            currentDrawing.positionCount = 1;
            currentDrawing.numCornerVertices = 20;
            currentDrawing.numCapVertices = 20;
            currentDrawing.SetPosition(0, tip.transform.position);
            currentDrawing.gameObject.layer = LayerMask.NameToLayer("Projection");
        }
        else
        {
            var currentPos = currentDrawing.GetPosition(currentDrawing.positionCount - 1);
            if (Vector3.Distance(currentPos, tip.transform.position) > DrawOffset)
            {
                currentDrawing.positionCount++;
                currentDrawing.SetPosition(currentDrawing.positionCount - 1, tip.transform.position);
            }
        }
    }

}
