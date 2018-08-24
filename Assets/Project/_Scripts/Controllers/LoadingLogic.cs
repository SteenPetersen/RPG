using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LoadingLogic : MonoBehaviour {

    [SerializeField]
    private string scene;

    Canvas[] ui;

    public Canvas LoadingScreen;

    private void Start()
    {
        ui = GameObject.Find("GameDetails").transform.GetComponentsInChildren<Canvas>();

        StartCoroutine(LoadNewScene(scene));

        LoadingScreen.enabled = true;
    }


    private void Update()
    {
        //loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
    }

    // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
    IEnumerator LoadNewScene(string scene)
    {
        //Application.backgroundLoadingPriority = ThreadPriority.Low;
        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
        foreach (Canvas canvas in ui)
        {
            canvas.enabled = false;
        }

        yield return new WaitForSeconds(1);
        AsyncOperation async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        async.allowSceneActivation = false;

        while (async.progress < 0.89f)
        {
            //Debug.Log(async.progress);
            yield return null;
        }

        async.allowSceneActivation = true;


    }
}
