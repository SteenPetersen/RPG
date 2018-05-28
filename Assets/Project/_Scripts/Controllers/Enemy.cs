using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class Enemy : Interactable {



    [HideInInspector]
    public CharacterStats myStats;
    [HideInInspector]
    public Animator anim;

	void Awake () {
        myStats = GetComponent<CharacterStats>();
    }

    public override void Interact()
    {
        base.Interact();
    }


}
