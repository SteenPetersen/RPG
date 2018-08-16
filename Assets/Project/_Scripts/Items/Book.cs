using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Book", menuName = "Items/Books", order = 1)]
public class Book : Item, IUseable
{
    [Tooltip("first line of the description")]
    [SerializeField] string description;
    [Tooltip("Second line of the description")]
    [SerializeField] string description2;
    [SerializeField] ParticleSystem clickParticles;
    [SerializeField] bool drawGizmos;

    Vector2 player;
    GameObject tmp;

    float range = 4f;

    public override void Use()
    {
        player = PlayerController.instance.transform.position;

        bool spawn = ParticleSystemHolder.instance.SpawnPort(player, name, range);

        if (spawn)
        {
            Remove();
        }
    }

    public override string GetDescription(bool showSaleValue = true)
    {
        return base.GetDescription() + string.Format(description + "\n" + description2);
    }

}
