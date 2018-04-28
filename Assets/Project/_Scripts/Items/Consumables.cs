using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumables")]
public class Consumables : Item {

    public PotionType potionType;
    public int valueIncrease;

    PlayerStats stats;

    public override void Use()
    {
        //base.Use();
        //// use the potion

        Consume();
        //remove it from inventory

    }

    /// <summary>
    /// Uses the consumable and applies its effect
    /// </summary>
    void Consume()
    {
        Debug.Log("using potion");
        // Health Potion
        if (potionType == 0)
        {
            bool tryHeal = PlayerController.instance.gameObject.GetComponent<PlayerStats>().Heal(valueIncrease);

            if (tryHeal)
            {
                Remove();
                //RemoveFromInventory();
                SoundManager.instance.PlayInventorySound("gulp");
            }
        }
    }

    public enum PotionType { Health, Mana, Str, Agi, Int, Sta, Armor }
}
