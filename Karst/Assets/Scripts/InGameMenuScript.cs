using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class InGameMenuScript : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuCanvas;
    [SerializeField] GameObject rayInteractor;
    [SerializeField] Button continueBtn;
    [SerializeField] Button exitToMenuBtn;
    bool paused;
    bool lastMenuButtonState;

    void Start()
    {
        paused = false;
        lastMenuButtonState = false;
        continueBtn.onClick.AddListener(ContinueButtonHandler);
        exitToMenuBtn.onClick.AddListener(ExitToMenuButtonHandler);
    }

    void Update()
    {
        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).IsPressed(InputHelpers.Button.MenuButton, out bool menuButtonPressed);

        // Check for button press (transition from not pressed to pressed)
        if (menuButtonPressed && !lastMenuButtonState)
        {
            TogglePauseState();
        }

        lastMenuButtonState = menuButtonPressed;
    }

    void TogglePauseState()
    {
        if (!paused)
        {
            PauseGame();
        }
        else
        {
            ContinueButtonHandler();
        }
    }

    void PauseGame()
    {
        gameObject.GetComponent<Draw>().enabled = false;
        rayInteractor.SetActive(true);
        pauseMenuCanvas.SetActive(true);
        paused = true;
        Time.timeScale = 0f;
    }

    public void ContinueButtonHandler()
    {
        pauseMenuCanvas?.SetActive(false);
        rayInteractor.SetActive(false);
        gameObject.GetComponent<Draw>().enabled = true;
        Time.timeScale = 1f;
        paused = false;
    }

    public void ExitToMenuButtonHandler()
    {
        pauseMenuCanvas?.SetActive(false);
        rayInteractor?.SetActive(false);
        Time.timeScale = 1f;
        SceneTransitionManager.singleton.GoToScene(0);
        paused = false;
    }
}