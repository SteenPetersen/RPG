using System;
using System.Collections;
using UnityEngine;

public class Destructable : MonoBehaviour {

    public bool shaking;
    public bool obstructVision;

    [Tooltip("Does this Destructable have anything inside")]
    [SerializeField] bool objInside;
    [SerializeField] bool canDropItems;
    [Tooltip("Place the items that can only drop once as the first item in the list")]
    [SerializeField] GameObject[] droppableItems;

    [SerializeField] int chanceToDrop;
    [SerializeField] float chanceForRareItem;
    [SerializeField] float chanceForUncommonItem;

    [SerializeField] Material damagedMat;

    [SerializeField] float shakeTime;
    [SerializeField] float decreaseShakeTime;
    [SerializeField] int MaxHealth;
    [SerializeField] int health;
    [SerializeField] ParticleSystem impact;
    [SerializeField] ParticleSystem destroy;
    [SerializeField] string impactSound;
    [SerializeField] string destroySound;

    [Tooltip("The Object(s) inside this destructable that will spawn when destroyed")]
    [SerializeField] GameObject[] obj;

    [SerializeField] bool isDestroyed;
    [SerializeField] bool cannotMove;

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
        ShakeManager.instance.shakeGameObject(gameObject, shakeTime, decreaseShakeTime, true, this);

        health -= 50;
        impact.Play();
        SoundManager.instance.PlayEnvironmentSound(impactSound);

        if (health < (MaxHealth / 2) && damagedMat != null)
        {
            GetComponent<MeshRenderer>().material = damagedMat;
        }

        if (health < 0 && !isDestroyed)
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

            isDestroyed = true;
        }
    }

    public void Impact(bool move)
    {
        if (move)
        {
            if (!cannotMove)
            {
                FindDirectionAndMove();
                return;
            }

            ShakeManager.instance.shakeGameObject(gameObject, shakeTime, decreaseShakeTime, true, this);

            impact.Play();
            SoundManager.instance.PlayEnvironmentSound(impactSound);

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
            rand = UnityEngine.Random.Range(0, 100);

            if (rand < chanceForRareItem)
            {
                Instantiate(droppableItems[1], transform.position, Quaternion.identity);
                return;
            }

            else if (rand > chanceForRareItem && rand < chanceForUncommonItem)
            {
                rand = UnityEngine.Random.Range(2, droppableItems.Length);

                if (DebugControl.debugOn)
                {
                    Debug.Log("inside uncommon" + rand + "   " + droppableItems.Length);
                }

                if (droppableItems[rand].tag == "Town Portal Book")
                {
                    DungeonManager dungeon = DungeonManager.instance;

                    if (!dungeon.townPortalDropped)
                    {
                        Instantiate(droppableItems[rand], transform.position, Quaternion.identity);
                        dungeon.townPortalDropped = true;
                        return;
                    } 
                }

                Instantiate(droppableItems[rand], transform.position, Quaternion.identity);

                return;

            }

            Instantiate(droppableItems[0], transform.position, Quaternion.identity);
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

    void FindDirectionAndMove()
    {
        int obstacleLayer = 13;
        int des = 19;
        var obstacleLayerMask = 1 << obstacleLayer;
        var desmask = 1 << des;

        var final = obstacleLayerMask | desmask;

        float range = 0.5f;

        RaycastHit2D hit1 = Physics2D.Raycast((Vector2)transform.position + Vector2.up, Vector2.up, range, final);
        RaycastHit2D hit2 = Physics2D.Raycast((Vector2)transform.position + Vector2.down, Vector2.down, range, final);
        RaycastHit2D hit3 = Physics2D.Raycast((Vector2)transform.position + Vector2.right, Vector2.right, range, final);
        RaycastHit2D hit4 = Physics2D.Raycast((Vector2)transform.position + Vector2.left, Vector2.left, range, final);

        if (hit1.collider == null)
        {
            transform.position = transform.position + Vector3.up;
            Debug.Log("Moving a destructable (" + gameObject.name + ") up");
            return;
        }

        if (hit2.collider == null)
        {
            transform.position = transform.position + Vector3.down;
            Debug.Log("Moving a destructable (" + gameObject.name + ") down");
            return;
        }

        if (hit3.collider == null)
        {
            transform.position = transform.position + Vector3.right;
            Debug.Log("Moving a destructable (" + gameObject.name + ") right");
            return;
        }

        if (hit4.collider == null)
        {
            transform.position = transform.position + Vector3.left;
            Debug.Log("Moving a destructable (" + gameObject.name + ") left");
            return;
        }

        if (DebugControl.debugDungeon)
        {
            Debug.Log("Destroying a destructable (" + gameObject.name + ") because it had nowhere to go");
        }

        Destroy(gameObject);

    }

}
