using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemHolder : MonoBehaviour {

    public static ParticleSystemHolder instance;

    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public ParticleSystem impBlood;
    public ParticleSystem redBlood;
    public ParticleSystem bombExplosion;
    public ParticleSystem bossOneFireShield;

    [SerializeField] GameObject bombExplosionObj;

    [SerializeField] GameObject ashAway;

    [SerializeField] GameObject townPortal;
    [SerializeField] GameObject townPortalEnter;
    [SerializeField] GameObject bossPortal;
    [SerializeField] GameObject bossPortalEnter;
    [SerializeField] GameObject spawnIn;

    [SerializeField] GameObject levelUpEffect;

    public GameObject ChargedBowShot;

    public GameObject[] stunWords;
    public GameObject[] critWords;

    /// <summary>
    /// Plays an impact effect of a certain name at a certain location
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="pos"></param>
    public void PlayImpactEffect(string clip, Vector2 pos)
    {
        if (clip.StartsWith("Imp") && clip.EndsWith("_impact"))
        {
            Instantiate(impBlood, pos, Quaternion.identity);
        }

        else if (clip == "player")
        {
            Instantiate(redBlood, pos, Quaternion.identity);
        }
    }

    /// <summary>
    /// Plays spells effects based on an item name or effect name at a 
    /// given location
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="itemOrEffectName"></param>
    /// <returns></returns>
    public GameObject PlaySpellEffect(Vector2 pos, string itemOrEffectName)
    {
        if (itemOrEffectName.EndsWith("(Clone)"))
        {
            itemOrEffectName = itemOrEffectName.Remove(itemOrEffectName.Length - 7);
        }

        switch (itemOrEffectName)
        {
            case "Town Portal Book":
                return Instantiate(townPortal, pos, Quaternion.identity);

            case "Town Portal":
                return Instantiate(townPortalEnter, pos, Quaternion.identity);

            case "boss_portal":
                return Instantiate(bossPortal, pos, Quaternion.identity);

            case "Boss Portal":
                return Instantiate(bossPortalEnter, pos, Quaternion.identity);

            case "Spawn in":
                return Instantiate(spawnIn, pos, Quaternion.identity);

            case "level up":
                return Instantiate(levelUpEffect, pos, Quaternion.identity);

            case "imp die burning":
                return Instantiate(ashAway, pos, Quaternion.identity);

            case "explode":
                return Instantiate(bombExplosionObj, pos, Quaternion.identity);

        }

        return null;
    }

    public GameObject PlayerStunnedEffect()
    {
        int rnd = UnityEngine.Random.Range(0, stunWords.Length);

        switch (rnd)
        {
            case 0:
                return stunWords[0];

            case 1:
                return stunWords[1];

            case 2:
                return stunWords[2];
        }

        return stunWords[0];

    }

    public GameObject CritWord()
    {
        int rnd = UnityEngine.Random.Range(0, stunWords.Length);

        switch (rnd)
        {
            case 0:
                return critWords[0];

            case 1:
                return critWords[1];

            case 2:
                return critWords[2];
        }

        return null;

    }

    /// <summary>
    /// Spawns a port - this Method finds a location based on the players position
    /// and on the type of Ports range
    /// </summary>
    /// <param name="player"></param>
    /// <param name="name"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool SpawnPort(Vector2 player, string name, float range)
    {
        Vector2 dir = new Vector2();
        dir = Vector2.up;

        RaycastHit2D hit = new RaycastHit2D();

        return FindLocation(hit, dir, name, player, range);
    }

    /// <summary>
    /// Simply spawns a port at a given Position
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="name"></param>
    public void SpawnPort(Vector2 pos, string name)
    {
        PlaySpellEffect(pos, name);
        SoundManager.instance.PlayEnvironmentSound("portal_appears");
    }

    /// <summary>
    /// Finds all possible positions around the player and selects a random one.
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="dir"></param>
    /// <param name="player"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    bool FindLocation(RaycastHit2D hit, Vector2 dir, string obj, Vector2 player, float range)
    {
        int obstacleLayer = 13;
        int destructableLayer = 19;
        var obstacleLayerMask = 1 << obstacleLayer;
        var destructableLayerMask = 1 << destructableLayer;
        var finalMask = obstacleLayerMask | destructableLayerMask;


        List<Vector2> positions = new List<Vector2>();

        for (int i = 1; i <= 12; i++)
        {
            Vector2 direction = dir.Rotate(30f * i);
            hit = Physics2D.Raycast(player, direction, range, finalMask);

            if (hit.collider == null)
            {
                if (i == 1)
                {
                    Debug.DrawRay(player, direction, Color.blue, 5f);
                }
                else
                {
                    Debug.DrawRay(player, direction, Color.red, 5f);
                    Debug.DrawRay(player, dir, Color.yellow, 5f);
                }

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

            PlaySpellEffect(positions[rand], obj);
            SoundManager.instance.PlayEnvironmentSound("portal_appears");

            return true;
        }

        return false;

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
