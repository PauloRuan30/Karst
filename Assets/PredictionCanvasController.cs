using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class PredictionCanvasController : MonoBehaviour
{

    private bool showCanvasIsPressed;
    private bool isButtonPressedLastFrame;

    public GameObject predictiontext;

    void Start()
    {
        showCanvasIsPressed = false;
        isButtonPressedLastFrame = false;
    }

    void Update()
    {
        var device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (device.IsPressed(InputHelpers.Button.Primary2DAxisClick, out bool rightJoystickButtonPressed))
        {
            // Check if the button was pressed this frame but not in the last frame
            if (rightJoystickButtonPressed && !isButtonPressedLastFrame)
            {
                // Toggle the canvas visibility
                showCanvasIsPressed = !showCanvasIsPressed;
                predictiontext.SetActive(showCanvasIsPressed);
            }

            // Update the flag for the next frame
            isButtonPressedLastFrame = rightJoystickButtonPressed;
        }
        else
        {
            // Ensure flag is updated when the button is not pressed
            isButtonPressedLastFrame = false;
        }
    }
}
