using System;
using System.Collections;
using UnityEngine;

public class Destructable : MonoBehaviour {

    public bool destroyed;
    public bool shaking;
    public bool obstructVision;

    [Tooltip("Does this Destructable have anything inside")]
    [SerializeField] bool objInside;
    [SerializeField] bool canDropItems;
    [Tooltip("Place the items that can only drop once as the first item in the list")]
    [SerializeField] GameObject[] droppableItems;

    [SerializeField] int chanceToDrop;

    [SerializeField] Material damagedMat;

    [SerializeField] float shakeTime;
    [SerializeField] float decreaseShakeTiome;
    [SerializeField] int MaxHealth;
    [SerializeField] int health;
    [SerializeField] ParticleSystem impact;
    [SerializeField] ParticleSystem destroy;
    [SerializeField] string impactSound;
    [SerializeField] string destroySound;

    [Tooltip("The Object(s) inside this destructable that will spawn when destroyed")]
    [SerializeField] GameObject[] obj;

    /// <summary>
    /// Prop needed by Projectiles that may need to disactivate their reenderer when box explodes.
    /// </summary>
    public int MyHealth
    {
        get
        {
            return health;
        }
    }

    /// <summary>
    /// controls the Impact on this destructable and plays the 
    /// necessary particle effects as well as shaking the gameObject
    /// </summary>
    public void Impact()
    {
        ShakeManager.instance.shakeGameObject(gameObject, shakeTime, decreaseShakeTiome, true, this);

        health -= 50;
        impact.Play();
        SoundManager.instance.PlayEnvironmentSound(impactSound);

        if (health < (MaxHealth / 2) && damagedMat != null)
        {
            GetComponent<MeshRenderer>().material = damagedMat;
        }

        if (health < 0)
        {
            GetComponent<Collider2D>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;

            destroy.Play();
            SoundManager.instance.PlayEnvironmentSound(destroySound);

            Destroy(gameObject, 3);

            if (objInside)
            {
                int rand = UnityEngine.Random.Range(0, obj.Length);
                Vector3 vec = new Vector3(transform.position.x, transform.position.y, 0);
                Instantiate(obj[rand], vec, Quaternion.identity);
            }

            if (canDropItems)
            {
                DetermineLoot();
            }
        }
    }

    /// <summary>
    /// Determines what if anything dropped
    /// </summary>
    private void DetermineLoot()
    {
        int rand = UnityEngine.Random.Range(0, 100);


        if (rand < chanceToDrop)
        {
            rand = UnityEngine.Random.Range(0, droppableItems.Length);

            Debug.Log("tag = " + droppableItems[rand].tag);


            if (droppableItems[rand].tag == "TownPortal")
            {
                DungeonManager dungeon = DungeonManager.instance;

                if (!dungeon.townPortalDropped)
                {
                    Instantiate(droppableItems[rand], transform.position, Quaternion.identity);
                    dungeon.townPortalDropped = true;
                    return;
                }

                Instantiate(droppableItems[rand + 1], transform.position, Quaternion.identity);
                return;
            }

            Instantiate(droppableItems[rand], transform.position, Quaternion.identity);

        }
    }

    /// <summary>
    /// Instantly destroys the destructable
    /// </summary>
    public void InstantDestroy()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        destroy.Play();
        SoundManager.instance.PlayEnvironmentSound(destroySound);

        Destroy(gameObject, 3);
    }

}
