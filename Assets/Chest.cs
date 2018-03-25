using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable {

    [Tooltip("How far away from the chest does the the player need to be before it closes again")]
    public float closeRadius;

    [Tooltip("How big is the circle around this Chest in which the items can spawn")]
    public float lootSpawnRadius;

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

    void Start () {
        // set reference to spriteRenderer
        rend = GetComponent<SpriteRenderer>();

        // establish what loot is inside chest
        DetermineLoot();

        player = PlayerController.instance.gameObject.transform;

        myCollider = GetComponent<BoxCollider>();
	}


    private void Update()
    {
        if (chestClosed)
        {
            OpenChest();
        }
        else if (!chestClosed)
        {
            CloseIfPlayerLeaves();
        }

    }

    private void DetermineLoot()
    {
        // generate a random number
        int rnd = UnityEngine.Random.Range(0, 4);

        // iterate for as many times as the random number determined
        for (int i = 0; i < rnd; i++)
        {
            // add loot to the chest for each iteration
            loot.Add(LootController.instance.DetermineChestLoot(tier));
        }
    }

    private void OpenChest()
    {
        // if player has this item in focus and has not interacted with it
        if (isFocus && !hasInteracted)
        {
            // create a distance from the player to the chest
            float distance = Vector3.Distance(PlayerController.instance.gameObject.transform.position, transform.position);

            // if the distance is smaller than the radius
            if (distance <= radius)
            {
                // interact with the player
                Interact();

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
            }

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
                Debug.Log("Player is nearby!");
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
        }
    }

    private void SpawnLoot()
    {
        if (loot.Count != 0)
        {
            foreach (var item in loot)
            {
                // required to set myposition as a vector 2, because the insideUnitCircle method is ambiguous between vector3 and vector2
                Vector2 myPosition = transform.position;

                // now that we know the vector2 position of this object we can make a position within a random cicle around it
                Vector2 pos = myPosition + UnityEngine.Random.insideUnitCircle * lootSpawnRadius;

                // and instantiate the object at that position
                Instantiate(item, pos, Quaternion.identity);

                // play loot sound
                SoundManager.instance.PlayUiSound("lootdrop");
            }
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
}
