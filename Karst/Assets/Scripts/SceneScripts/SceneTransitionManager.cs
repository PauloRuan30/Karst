using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public FadeScreen fadeScreen;
    public static SceneTransitionManager singleton;
    public static int Index;

    private void Awake()
    {
        if (singleton && singleton != this)
            Destroy(singleton);

        singleton = this;
    }

    public void GoToScene(int sceneIndex)
    {
        StartCoroutine(GoToSceneRoutine(sceneIndex));
    }

    IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();

        yield return new WaitForSeconds(fadeScreen.fadeDuration);

        SceneManager.LoadScene(sceneIndex);
    }

    //public void GoToSceneAsync(int sceneIndex)
    //{
    //    StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    //    Index = sceneIndex;
    //}

    //IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    //{
    //    yield return StartCoroutine(fadeScreen.FadeRoutine(0, 1));
    //    //Launch the new scene
    //    //AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
    //    float timer = 0;
    //    while((timer <= fadeScreen.fadeDuration))
    //    {
    //        timer += Time.deltaTime;
    //        yield return null;
    //    }
    //}
}
