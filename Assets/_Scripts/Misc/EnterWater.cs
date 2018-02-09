using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterWater : MonoBehaviour {

    PlayerController player;

    private void Start()
    {
        player = PlayerController.instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        player.onLand = true;
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        player.onLand = false;
    }
}
