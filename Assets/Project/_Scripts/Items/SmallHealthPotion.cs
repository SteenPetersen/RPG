﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Small Health Potion", menuName ="Items/Potion", order = 1)]
public class SmallHealthPotion : Item, IUseable {

    [SerializeField]
    private int health = 0;

    public override void Use()
    {
        if (PlayerStats.instance.currentHealth < PlayerStats.instance.maxHealth)
        {
            SoundManager.instance.PlayInventorySound("gulp");

            Remove();

            PlayerStats.instance.Heal(health);
        }
    }
}