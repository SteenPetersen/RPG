﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDoor : MonoBehaviour {

    [SerializeField] bool open;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (DungeonManager.instance.playerHasBossKey)
            {
                if (!open)
                {
                    GetComponentInParent<Animator>().SetTrigger("open");
                    SoundManager.instance.PlayEnvironmentSound("stone_wall_open");
                    open = true;
                }
            }
            else
            {
                StoryManager.instance.NotifyPlayer("You do not have the Key!");
            }

        }

    }

    public void DisableCube()
    {
        gameObject.SetActive(false);
    }
}
