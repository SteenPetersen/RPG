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

    GameObject tmp;

    float range = 4f;
    Vector2 player;

    public override void Use()
    {
        player = PlayerController.instance.transform.position;
        bool spawn = SpawnPort(player, name);

        if (spawn)
        {
            Remove();
        }
    }

    bool SpawnPort(Vector2 player, string name)
    {
        Vector2 dir = new Vector2();
        dir = Vector2.up;

        RaycastHit2D hit = new RaycastHit2D();

        return FindLocation(hit, dir);

    }

    bool FindLocation(RaycastHit2D hit, Vector2 dir)
    {
        int obstacleLayer = 13;
        int obstacleLayerMask = 1 << obstacleLayer;
        List<Vector2> positions = new List<Vector2>();

        for (int i = 1; i <= 12; i++)
        {
            Vector2 direction = dir.Rotate(30f * i);
            hit = Physics2D.Raycast(player, direction, range, obstacleLayerMask);

            if (hit.collider == null)
            {
                bool canSpawn = CheckArea(player + (direction * range));

                if (canSpawn)
                {
                    Vector2 toAdd = player + (direction * range);
                    positions.Add(toAdd);
                }
            }
        }

        if (positions.Count != 0)
        {
            int rand = UnityEngine.Random.Range(0, positions.Count);

            ParticleSystemHolder.instance.PlaySpellEffect(positions[rand], name);
            SoundManager.instance.PlayEnvironmentSound("portal_appears");

            return true;
        }

        return false;

    }

    public override string GetDescription(bool showSaleValue = true)
    {
        return base.GetDescription() + string.Format("\n" + description + "\n" + description2);
    }



    /// <summary>
    /// Checks the area around a given Position for walls
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool CheckArea(Vector3 pos)
    {
        int obstaclesLayer = 13;
        var obstacleLayerMask = 1 << obstaclesLayer;

        Collider2D[] wallColliders = Physics2D.OverlapCircleAll(pos, 1f, obstacleLayerMask);

        if (wallColliders.Length != 0)
        {
            foreach (Collider2D col in wallColliders)
            {
                if (col.transform.name.Contains("Wall"))
                {
                    return false;
                }
            }
        }

        return true;
    }

}
