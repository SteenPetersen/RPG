using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable {

    [Tooltip("How far away from the chest does the the player need to be before it closes again")]
    public float closeRadius;

    [Tooltip("How big is the circle around this Chest in which the items can spawn")]
    public float lootSpawnRadius;

    [SerializeField] int maxAmountOfItems;

    [SerializeField] bool locked = true;

    [Tooltip("Tier of loot to provide")]
    public int tier;  // tier of loot to provide

    [Tooltip("Level of key required to open")]
    [SerializeField] int keyTier;

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

    [Tooltip("All possible position where loot can land depending on this chests position in the dungeon")]
    [SerializeField] List<Vector2> positions = new List<Vector2>();

    [SerializeField] float distanceFromChest;

    bool showingEffect;
    [SerializeField] GameObject front;
    [SerializeField] ParticleSystem effect;
    bool lootTaken;

    [SerializeField] bool specificItemChest;
    [SerializeField] GameObject[] specificItems;

    void Start () {
        // set reference to spriteRenderer
        rend = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (!chestClosed)
        {
            CloseIfPlayerLeaves();
        }

        if (!lootTaken)
        {
            if (!showingEffect && !chestClosed)
            {
                front.SetActive(true);
                effect.Play();
                showingEffect = true;
            }

        }

        if (showingEffect && chestClosed)
        {
            front.SetActive(false);
            effect.Stop(true);
            effect.Clear();
            showingEffect = false;
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
                    Key k = InventoryScript.instance.CheckForCorrectKey(keyTier);

                    if (k != null)
                    {
                        k.UseKeyOnChest();
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
                bool hasLoot = DetermineLoot();

                if (!hasLoot)
                {
                    lootTaken = true;
                }

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

    /// <summary>
    /// Determines what loot there is inside the chest by passing its teir to the Loot Manager
    /// </summary>
    private bool DetermineLoot()
    {
        if (specificItemChest)
        {
            for (int i = 0; i < specificItems.Length; i++)
            {
                loot.Add(Instantiate(specificItems[i]));
            }


            return true;
        }

        // generate a random number
        int rnd = UnityEngine.Random.Range(0, maxAmountOfItems + 1);

        if (DebugControl.debugEnvironment)
        {
            Debug.Log(rnd);
        }

        if (rnd == 0)
        {
            return false;
        }


        // iterate for as many times as the random number determined
        for (int i = 0; i < rnd; i++)
        {
            // add loot to the chest for each iteration
            loot.Add(EquipmentGenerator._instance.CreateDroppable(tier));
        }

        return true;
    }

    private void SpawnLoot()
    {
        if (loot.Count != 0)
        {
            //GameObject shadeChest = new GameObject();
            //shadeChest.transform.rotation = CameraController.instance.transform.rotation;
            //shadeChest.transform.parent = null;
            //shadeChest.transform.position = transform.position;

            //positions = LootController.instance.FindAllEndPositions(shadeChest.transform, distanceFromChest);

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

    IEnumerator SpawnLootItems(List<GameObject> items, int count)
    {
        foreach (var item in items)
        {
            var text = CombatTextManager.instance.FetchText(transform.position);
            var textScript = text.GetComponent<CombatText>();
            textScript.White(item.name, transform.position);
            text.transform.position = transform.position;
            text.SetActive(true);

            ShakeManager.instance.shakeGameObject(gameObject, 0.3f, 0.2f, true);

            LootParabola movement = item.GetComponent<LootParabola>();

            // TODO maybe activate (SetActive) the lootParabola here instead of having it on the default item? if it gives trouble elsewhere 
            // then this would be the place to fix it

            //item.transform.parent = shadeChest;
            item.transform.position = transform.position;

            List<Vector2> positions = new List<Vector2>();

            positions = LootController.instance.FindAllEndPositions(transform, distanceFromChest);

            Debug.Log(positions.Count);

            Vector2 endPosition = LootController.instance.SelectEndPosition(positions);

            movement._EndPosition = endPosition;
            movement._StartPosition = transform.position;
            movement._Height = heightOfLootJump;

            //Debug.DrawLine(shadeChest.position, endPosition, Color.cyan, 5f);

            StartCoroutine(movement.Parabola());
            //movement.Go = true;

            //shadeChest.gameObject.name = item.name + "_Creator";
            SoundManager.instance.PlayUiSound("lootAppearsChest");

            yield return new WaitForSeconds(0.5f);
        }

        lootTaken = true;
        //Destroy(shadeChest.gameObject, 3f);
    }
}

