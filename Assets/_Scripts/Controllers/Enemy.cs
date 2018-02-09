﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    /* handles interactions with enemies */

[RequireComponent(typeof(CharacterStats))]
public class Enemy : Interactable {


    [HideInInspector]
    public PlayerManager playerManager;
    [HideInInspector]
    public CharacterStats myStats;
    [HideInInspector]
    public Animator anim;

	void Awake () {
        playerManager = GameObject.Find("GameManager").GetComponent<PlayerManager>();
        myStats = GetComponent<CharacterStats>();
	}

    public override void Interact()
    {
        base.Interact();

        CharacterCombat playerCombat = playerManager.player.GetComponent<CharacterCombat>();


        if (playerCombat != null)
        {
           playerCombat.Attack(myStats, anim);
        }
    }


}
