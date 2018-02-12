using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    /* handles interactions with enemies */

[RequireComponent(typeof(CharacterStats))]
public class Enemy : Interactable {



    [HideInInspector]
    public CharacterStats myStats;
    [HideInInspector]
    public Animator anim;

	void Awake () {
        myStats = GetComponent<CharacterStats>();
        playerManager = PlayerManager.instance;

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
