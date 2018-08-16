using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Key", menuName = "Items/Keys", order = 1)]
public class Key : Item, IUseable
{
    [Tooltip("first line of the description")]
    [SerializeField] string description;
    [Tooltip("Second line of the description")]
    [SerializeField] string description2;
    [SerializeField] bool drawGizmos;

    [SerializeField] int amount;
    [SerializeField] int tier;
    [SerializeField] bool bossKey;

    float range = 4f;

    public int _Amount
    {
        get
        {
            return amount;
        }

        set
        {
            amount = value;
        }
    }

    public bool _BossKey
    {
        get
        {
            return bossKey;
        }

        set
        {
            bossKey = value;
        }
    }

    public int _Tier
    {
        get
        {
            return tier;
        }

        set
        {
            tier = value;
        }
    }

    public override void Use()
    {
        Remove();
    }

    public override string GetDescription(bool showSaleValue = true)
    {
        return base.GetDescription() + string.Format(description + "\n" + description2);
    }

}
