using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class DeathScene : MonoBehaviour {

    public Text foesVanquished, 
        dungeonFloorsExplored, 
        riposte, 
        block, 
        hits, 
        fullChargeHits, 
        arrowsFired, 
        randomizedLootDropped;

    private void OnEnable()
    {
        // subscribe to notice is a scene is loaded.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UiManager.instance.HideToolTip();

        foesVanquished.text = "Foes Vanquished: " + GameDetails.enemiesKilled;
        dungeonFloorsExplored.text = "Dungeon floors explored: " + GameDetails.dungeonFloorsExplored;
        riposte.text = "Ripostes executed: " + GameDetails.ripostes;
        block.text = "Blocks performed: " + GameDetails.blocks;
        hits.text = "Successful hits: " + GameDetails.hits;
        fullChargeHits.text = "Charged Hits: " + GameDetails.fullChargeHits;
        arrowsFired.text = "Arrows Fired: " + GameDetails.arrowsFired;
        randomizedLootDropped.text = "Random loot created: " + GameDetails.randomizedItemsDropped;
    }

    // Use this for initialization
    void Start () {

        var allCanvas = GameDetails.instance.gameObject.GetComponentsInChildren<Canvas>();

        foreach (Canvas i in allCanvas)
        {
            i.enabled = false;
        }

	}
	
    private void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Continue()
    {
        CameraController.instance.SetHomeRotation();
        GameDetails.instance.Load(true);
    }
}
