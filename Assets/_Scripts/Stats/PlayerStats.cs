using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : CharacterStats {

    public Animator anim;
    public PlayerController playerControl;

    public float exp;

    void Start () {
        EquipmentManager.instance.onEquipmentChanged += OnEquipmentChanged;
        anim = GetComponent<Animator>();
        playerControl = PlayerController.instance;
	}
    private void Update()
    {
        if (currentHealth == maxHealth)
        {
            RemoveHealthBar();
        }

        if (playerControl == null)
        {
            playerControl = PlayerController.instance;
        }
    }

    private void OnEquipmentChanged(Equipment newItem, Equipment oldItem)
    {
        if(newItem != null)
        {
            armor.AddModifier(newItem.armorModifier);
            damage.AddModifier(newItem.damageModifier);
        }

        if (oldItem != null)
        {
            armor.RemoveModifier(oldItem.armorModifier);
            damage.RemoveModifier(oldItem.damageModifier);
        }
    }

    public override void Die()
    {
        base.Die();
        // kill the player in some way
        playerControl.speed = 0;
        playerControl.isDead = true;
        anim.SetTrigger("Dead");
        PlayerManager.instance.KillPlayer();
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (playerControl.healthGroup.alpha == 0)
        {
            playerControl.healthGroup.alpha = 1f;
        }

        playerControl.healthbar.value = playerControl.CalculateHealth(currentHealth, maxHealth);
    }

    void RemoveHealthBar()
    {
        playerControl.healthGroup.alpha -= 0.02f;
    }

    private void OnDisable()
    {
        EquipmentManager.instance.onEquipmentChanged -= OnEquipmentChanged;
    }
}
