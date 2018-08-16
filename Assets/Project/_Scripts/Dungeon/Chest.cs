using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable {

    [Tooltip("How far away from the chest does the the player need to be before it closes again")]
    public float closeRadius;

    [Tooltip("How big is the circle around this Chest in which the items can spawn")]
    public float lootSpawnRadius;

    [SerializeField] bool locked = true;

    [Tooltip("Tier of loot to provide")]
    public int tier;  // tier of loot to provide

    [Tooltip("Has this chest been Opened before")]
    public bool hasBeenOpened;

    [Tooltip("Sprite for when the chest is closed")]
    public Sprite closed;
    [Tooltip("Sprite for when the chest is open")]
    public Sprite open;
    SpriteRenderer rend;

    public bool chestClosed = true;
    [Tooltip("A list that gets populated by the lootcontroller script in the start method. Determines what object(s) are actually inside this chest")]
    public List<GameObject> loot = new List<GameObject>();  // what is actually inside the chest

    BoxCollider myCollider;

    [Tooltip("How High should loot jump?")]
    [SerializeField] float heightOfLootJump;

    [SerializeField] float distanceFromChest;

    GameObject goToCircumventrotationOnChest;

    bool showingEffect;
    [SerializeField] GameObject front;
    [SerializeField] ParticleSystem effect;

    

    void Start () {
        // set reference to spriteRenderer
        rend = GetComponent<SpriteRenderer>();

        player = PlayerController.instance.gameObject.transform;

        myCollider = GetComponent<BoxCollider>();

        goToCircumventrotationOnChest = new GameObject();
        goToCircumventrotationOnChest.transform.rotation = CameraController.instance.transform.rotation;
        goToCircumventrotationOnChest.transform.parent = null;
        goToCircumventrotationOnChest.transform.position = transform.position;

    }


    private void Update()
    {
        if (!chestClosed)
        {
            CloseIfPlayerLeaves();
        }


        if (Input.GetKey(KeyCode.T))
        {
            FindLocation(goToCircumventrotationOnChest);
        }

        if (!showingEffect && !chestClosed)
        {
            front.SetActive(true);
            effect.Play();
            showingEffect = true;
        }

        if (showingEffect && chestClosed)
        {
            front.SetActive(false);
            effect.Stop(true);
            effect.Clear();
            showingEffect = false;
        }

    }

    /// <summary>
    /// Determines what loot there is inside the chest by passing its teir to the Loot Manager
    /// </summary>
    private void DetermineLoot()
    {
        // generate a random number
        int rnd = UnityEngine.Random.Range(0, 4);

        // iterate for as many times as the random number determined
        for (int i = 0; i < rnd; i++)
        {
            // add loot to the chest for each iteration
            loot.Add(EquipmentGenerator._instance.CreateDroppable(tier));
        }
    }

    public override void Interact()
    {
        base.Interact();

        if (chestClosed)
        {
            if (locked)
            {
                // create a distance from the player to the chest
                float distance = Vector3.Distance(PlayerController.instance.gameObject.transform.position, transform.position);

                if (distance < lootSpawnRadius)
                {
                    Key k = InventoryScript.instance.CheckForCorrectKey(tier);

                    if (k != null)
                    {
                        k.Use();
                        OpenChest();
                        return;
                    }

                    var text = CombatTextManager.instance.FetchText(transform.position);
                    var textScript = text.GetComponent<CombatText>();
                    textScript.White("Locked!" + "\n" + "No key!", transform.position);
                    text.transform.position = transform.position;
                    text.SetActive(true);
                    textScript.FadeOut();

                    return;
                }
            }

            OpenChest();

        }
    }

    /// <summary>
    /// Opens the chest and spawns the loot inside
    /// </summary>
    private void OpenChest()
    {
        // if player has this item in focus and has not interacted with it
        if (!hasInteracted)
        {
            //player a sound for opening the chest
            SoundManager.instance.PlayUiSound("chestopen");

            // change the sprite to look like the chest is open
            rend.sprite = open;

            // if this chest has not been opened before
            if (!hasBeenOpened)
            {
                // spawn the loot
                SpawnLoot();
            }
            else if (hasBeenOpened)
            {
                var text = CombatTextManager.instance.FetchText(transform.position);
                var textScript = text.GetComponent<CombatText>();
                textScript.White("Looted!", transform.position);
                text.transform.position = transform.position;
                text.SetActive(true);
                textScript.FadeOut();
            }

            // disactivate 3d collider so it doesnt get in the way of looting
            myCollider.enabled = false;

            // establish that the player has already interacted with the chest
            hasInteracted = true;

            // establish that the chest is no longer closed
            chestClosed = false;

            locked = false;
        }
    }

    private void CloseIfPlayerLeaves()
    {
        Vector2 direction = new Vector2(PlayerController.instance.gameObject.transform.position.x - transform.position.x, PlayerController.instance.gameObject.transform.position.y - transform.position.y);
        direction.Normalize();

        int playerLayer = 10;
        var playerlayerMask = 1 << playerLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, closeRadius, playerlayerMask);

        Debug.DrawRay(transform.position, direction, Color.green, 1);
        //Debug.DrawLine(transform.position, PlayerController.instance.gameObject.transform.position - transform.position, Color.yellow);

        if (hit.transform != null)
        {
            if (hit.collider.name == "Player")
            {
                //Debug.Log("Player is nearby!");
                return;
            }
        }

        CloseChest();

    }

    private void CloseChest()
    {
        if (!chestClosed)
        {
            // change the sprite to look closed
            rend.sprite = closed;

            //player a sound for closing the chest
            SoundManager.instance.PlayUiSound("chestclose");

            // establish that the chest is closed for the scripts sake
            chestClosed = true;

            // reactivate 3d collider so it player can open Chest again
            myCollider.enabled = true;


            hasInteracted = false;
        }
    }

    private void SpawnLoot()
    {
        DetermineLoot();

        if (loot.Count != 0)
        {
             StartCoroutine(SpawnLootItems(loot, loot.Count));
        }

        else if (loot.Count == 0)
        {
            var text = CombatTextManager.instance.FetchText(transform.position);
            var textScript = text.GetComponent<CombatText>();
            textScript.White("Empty!", transform.position);
            text.transform.position = transform.position;
            text.SetActive(true);
        }


        // make sure you cannot open this chest multiple times
        hasBeenOpened = true;
    }

    /// <summary>
    /// Finds a random position amongst 24 evenly distributed points which fill the
    /// criteria of not being inside or on the otherside of a wall or innaccessable to the player.
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    Vector2 FindLocation(GameObject origin)
    {
        int obstacleLayer = 13;
        int destructableLayer = 19;
        var obstacleLayerMask = 1 << obstacleLayer;
        var destructableLayerMask = 1 << destructableLayer;

        var finalMask = obstacleLayerMask | destructableLayerMask;

        // A list of possible locations for the loot to land at
        List<Vector2> positions = new List<Vector2>();

        //Start on ground
        Vector3 beginRayCast = new Vector3(origin.transform.position.x, origin.transform.position.y, 0);

        // initial direction is from obj and up
        Vector2 direction = origin.transform.position + Vector3.up;

        for (int i = 1; i <= 24; i++)
        {
            Vector2 dir = direction.Rotate(15f * i);

            RaycastHit2D hit = Physics2D.Raycast(beginRayCast, dir, distanceFromChest, finalMask);

            if (hit.collider == null)
            {
                if (drawGizmos)
                {
                    if (i == 1)
                    {
                        Debug.DrawRay(beginRayCast, dir.normalized * distanceFromChest, Color.blue, 1f);
                    }
                    else
                    {
                        Debug.DrawRay(beginRayCast, dir.normalized * distanceFromChest, Color.red, 1f);
                    }
                }

                bool canSpawn = CheckAreaForWalls((Vector2)beginRayCast + (dir.normalized * distanceFromChest));

                if (canSpawn)
                {
                    Vector2 toAdd = (Vector2)beginRayCast + (dir.normalized * distanceFromChest);
                    positions.Add(toAdd);
                }
            }
        }

        if (positions.Count != 0)
        {
            int rand = UnityEngine.Random.Range(0, positions.Count);
            return positions[rand];
        }

        return transform.position;

    }

    /// <summary>
    /// Checks a specific area for wall colliders
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool CheckAreaForWalls(Vector3 pos)
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

    /// <summary>
    /// A temporary change of ths sorting order so the item look like they are coming from inside
    /// the Chest and not from behind it.
    /// </summary>
    /// <returns></returns>
    IEnumerator TemporarySortingOrderCHange(GameObject tmp)
    {
        SpriteRenderer sr = tmp.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 11;

        yield return new WaitForSeconds(0.5f);

        sr.sortingOrder = 10;
    }

    IEnumerator SpawnLootItems(List<GameObject> items, int count)
    {
        var text = CombatTextManager.instance.FetchText(transform.position);
        var textScript = text.GetComponent<CombatText>();
        textScript.White("Click!", transform.position);
        text.transform.position = transform.position;
        text.SetActive(true);

        ShakeManager.instance.shakeGameObject(gameObject, 1f, 0.2f, true);

        yield return new WaitForSeconds(0.2f);

        foreach (var item in items)
        {
            LootParabola movement = item.GetComponent<LootParabola>();

            // TODO maybe activate the lootParabola here instead of having it on the default item? if it gives trouble elsewhere 
            // then this would be the place to fix it

            GameObject goToCircumventrotationOnChest = new GameObject();
            goToCircumventrotationOnChest.transform.rotation = CameraController.instance.transform.rotation;
            goToCircumventrotationOnChest.transform.parent = null;
            goToCircumventrotationOnChest.transform.position = transform.position;
            item.transform.parent = goToCircumventrotationOnChest.transform;
            item.transform.position = transform.position;

            Vector2 endPosition = FindLocation(goToCircumventrotationOnChest);

            movement._EndPosition = endPosition;
            movement._StartPosition = transform.position;
            movement._Height = heightOfLootJump;

            Debug.DrawLine(goToCircumventrotationOnChest.transform.position, endPosition, Color.cyan, 5f);

            movement.Go = true;

            SoundManager.instance.PlayUiSound("lootAppearsChest");


            yield return new WaitForSeconds(0.5f);
        }

    }
}

