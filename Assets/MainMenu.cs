using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Not USed
/// </summary>
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
        Debug.Log("trying to save");
        Scene s = SceneManager.GetActiveScene();

        GameDetails.instance.Save(s);

        Debug.Log("trying to save II");
        Application.Quit();
    }

    private void OnDisable()
    {
        // UNsubscribe to notice is a scene is loaded.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
