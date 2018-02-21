using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingLogic : MonoBehaviour {

    [SerializeField]
    private int scene;
    [SerializeField]
    private Text loadingText;

    private void Start()
    {
        StartCoroutine(LoadNewScene(scene));
    }


    private void Update()
    {
        loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
    }

    // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
    IEnumerator LoadNewScene(int scene)
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
        AsyncOperation async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            Debug.Log(async.progress);
            yield return null;
        }

        async.allowSceneActivation = true;


    }
}
