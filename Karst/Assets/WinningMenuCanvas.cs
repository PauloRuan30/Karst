using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinningMenuCanvas : MonoBehaviour
{
    [SerializeField] GameObject winningMenuCanvas;
    [SerializeField] GameObject rayInteractor;
    [SerializeField] Button tourMapBtn;
    [SerializeField] Button restartBtn;
    [SerializeField] Button exitToMenuBtn;
    public static bool isWin;
    bool menuOn;

    void Start()
    {
        menuOn = false;
        isWin = false;
        tourMapBtn.onClick.AddListener(TourMapButtonHandler);
        restartBtn.onClick.AddListener(RestartButtonHandler);
        exitToMenuBtn.onClick.AddListener(ExitToMenuButtonHandler);
    }
    void Update()
    {
        if (isWin && !menuOn)
        {
            gameObject.GetComponent<Draw>().enabled = false;
            rayInteractor.SetActive(true);
            winningMenuCanvas.SetActive(true);
            Time.timeScale = 0f;
            menuOn = true;
        }
    }
    public void TourMapButtonHandler()
    {
        winningMenuCanvas?.SetActive(false);
        rayInteractor.SetActive(false);
        gameObject.GetComponent<Draw>().enabled = true;
        Time.timeScale = 1f;
    }
    public void RestartButtonHandler()
    {
        winningMenuCanvas?.SetActive(false);
        rayInteractor.SetActive(false);
        Time.timeScale = 1f;
        SceneTransitionManager.singleton.GoToScene(1);
    }

    public void ExitToMenuButtonHandler()
    {
        winningMenuCanvas?.SetActive(false);
        rayInteractor?.SetActive(false);
        Time.timeScale = 1f;
        SceneTransitionManager.singleton.GoToScene(0);
    }
}
