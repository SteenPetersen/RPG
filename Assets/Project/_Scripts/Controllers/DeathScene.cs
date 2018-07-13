using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class DeathScene : MonoBehaviour {

    public Text foesVanquished, dungeonFloorsExplored;


    private void OnEnable()
    {
        // subscribe to notice is a scene is loaded.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        foesVanquished.text = "Foes Vanquished: " + GameDetails.enemiesKilled;
        dungeonFloorsExplored.text = "Dungeon floors explored: " + GameDetails.dungeonFloorsExplored;

    }

    // Use this for initialization
    void Start () {

        var allCanvas = GameDetails._instance.gameObject.GetComponentsInChildren<Canvas>();

        foreach (Canvas i in allCanvas)
        {
            i.enabled = false;
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Continue()
    {
        GameDetails._instance.Load();
    }
}
