using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void OnEnable()
    {
        // subscribe to notice is a scene is loaded.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }

    public void PlayGame()
    {
        StartCoroutine(GameDetails.instance.FadeOutAndLoadScene("TutorialArea_indoor"));
        Debug.Log("Playing game");
    }

    public void QuitGame()
    {
        Scene s = SceneManager.GetActiveScene();

        if (!s.name.EndsWith("_indoor"))
        {
            GameDetails.instance.Save(true);
        }

        Application.Quit();
    }

    private void OnDisable()
    {
        // UNsubscribe to notice is a scene is loaded.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
