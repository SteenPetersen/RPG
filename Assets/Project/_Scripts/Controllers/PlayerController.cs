using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    // plane needed for hitray during projectiles
    // located on Gamemanager since monsters also need access to it.
    Plane m_Plane;
    [SerializeField] bool ripostePossible, enemiesInRange, lineOfSightRoutineActivated, largeStrike, largeStrikeAnimationReady, 
                          heldStrikeCoroutinePlaying, maxChargedHit;
    [SerializeField] float riposteTime, chargeTime, blockTime = 0;

    Vector3 prevPosition;
    Vector3 move;

    #region Singleton

    public static PlayerController instance;

    private void Awake()
    {

        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        //cam = Camera.main;

    }
    #endregion

    GameObject skeleton;
    Vector2 mousePosition;
    EventSystem eventSys;
    float thrustSpeed = 0.5f;                  

    Rigidbody2D rigid;
    CameraController cameraControl;
    PlayerStats playerStat;
    GameObject cameraHolder;
    ParticleSystem[] particles;

    #region Public Variables

    /// <summary>
    /// playerstats needs reference to check if player can riposte or not
    /// </summary>
    public bool timedBlock;

    /// <summary>
    /// Needed everyTime an outside scripts wishes to stun the player or knockback
    /// </summary>
    [HideInInspector] public bool interruptMovement;

    /// <summary>
    /// Activated when player is dead
    /// Gamedetails needs reference to it to start the death sequence
    /// Enemies needs reference to it to stop attacking when player is dead
    /// </summary>
    [HideInInspector] public bool isDead;

    /// <summary>
    /// List of enemies neaby
    /// Gamedetails needs reference to remove enemies from list when they die or on loading scenes
    /// </summary>
    [HideInInspector] public List<GameObject> enemies = new List<GameObject>();

    /// <summary>
    /// Speed of Character
    /// Needed for saving and Loading incase we make it possible for player to adjust this.
    /// </summary>
    [HideInInspector] public float speed;

    /// <summary>
    /// needed on gamedetails for setting death animation etc
    /// </summary>
    [HideInInspector] public Animator anim;

    /// <summary>
    /// needed for correcting position during dialogue and inventory
    /// </summary>
    [HideInInspector] public bool facingRight = true;

    #endregion Public Variables

    public Transform back;
    public Image healthBar;
    public Image staminaBar;
    float currentSta;

    #region projectiles
    PooledProjectilesController pooledArrows;
    [SerializeField] GameObject projectilePoint;
    [SerializeField] float projectileSpeed;
    [SerializeField] Transform meleeStartPoint;
    Vector2 direction;
    #endregion

    #region State/etc
    public Text stateText;          // to give player feedback of what state they are in
    public bool melee = true;       // is the player in melee?
    public bool ranged, dialogue;   // is the player using ranged or in a dialogue? Player can still animate during pause due to unscaled time on his animator

    EquipmentManager equip;
    //bool weaponsGone = false;       // has the player unequipped his/her weapons

    //blockState
    bool hasBlockingModifier;           // To make sure armor modifer only get added once when block is clicked
    int modifierToRemove;           // actual amount of extra damage that is absorbed when blocking
    public bool blocking;
    #endregion

    //[SerializeField] private bool mouseOverInteractableAndPlayerInRange;
    [SerializeField] ParticleSystem weaponCharged;
    bool weaponChargeReady;

    private void OnEnable()
    {
        // subscribe to notice is a scene is loaded.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        InitializePlayerGameObject();
    }

    private void Update()
    {
        if (isDead || GameDetails._instance.paused || dialogue)
            return;

        HandleAggro();
        HandleCombatState();
        HandleActionBarInput();
        HandleAnimation();

        CheckIfFacingCorrectDirection();
        HandleMovement();

        if (staminaBar.fillAmount != playerStat.CalculateHealth(playerStat.MyCurrentStamina, playerStat.MyMaxStamina))
        {
            LerpStaminaBar();
        }
    }

    private void FixedUpdate()
    {
        //Must happen in fixed update so all computers charge weapon at same speed
        if (largeStrikeAnimationReady)
        {
            ChargeHit();

            if (EquipmentManager.instance.weaponGlowSlot.color.a > 0.98 && !weaponChargeReady)
            {
                weaponChargeReady = true;
                weaponCharged.Play();
            }
        }
    }

    private void HandleAggro()
    {
        // make a layerMask
        int layerId = 11;
        int layerMask = 1 << layerId;

        // make a temporary array to hold the overlapping enemies on the layer in question
        Collider2D[] currentEnemiesInRange = Physics2D.OverlapCircleAll(transform.position, 25, layerMask);

        // if there are objects in the list
        if (currentEnemiesInRange.Length > 0)
        {
            // then there are enemies nearby
            enemiesInRange = true;

            // run through all of those enemies
            foreach (var enemy in currentEnemiesInRange)
            {
                // if the enemy is not already in the list and the enemy is not dead
                if (!enemies.Contains(enemy.transform.parent.gameObject) && !enemy.transform.parent.gameObject.GetComponent<EnemyAI>().isDead)
                {
                    // add the enemy to the list / enemy gets removed from this list when dying or when out of range
                    enemies.Add(enemy.gameObject.transform.parent.gameObject);
                }
            }
        }

        // if there are no enemies in the list
        else if (currentEnemiesInRange.Length == 0)
        {
            // then there are no enemies nearby
            enemiesInRange = false;
            // so clear the list of enemies so we dont iterate through enemies that we outran
            enemies.Clear();
        }

        // there are enemies nearby and you are not already checking line of sight
        if (enemiesInRange && !lineOfSightRoutineActivated)
        {
            // start the coroutine to check if they can see you
            StartCoroutine(LineOfSight());

            // let the script know that you have activated the corutine so you dont keep calling it
            lineOfSightRoutineActivated = true;
        }
    } // commented

    IEnumerator LineOfSight()
    {
        // while you have enemies in the area
        while (enemiesInRange)
        {
            //Debug.Log("calling Coroutine");

            // run through all the enemies in the list
            for (int i = 0; i < enemies.Count; i++)
            {
                // if the enemy has an EnemyAI script
                if (enemies[i].GetComponent<EnemyAI>() != null)
                {
                    // call the enemies DetermineAggro Method and have it check if it can see you or not.
                    enemies[i].GetComponent<EnemyAI>().DetermineAggro(transform.position);
                }
            }

            // when all enemies have been checked but there are still enemies in range wait for a moment
            yield return new WaitForSeconds(0.5f);
        }

        // if there are no enemies in range then let the HandleAggro function know that the coroutine is not running and it can be restarted.
        lineOfSightRoutineActivated = false;
    } // commented

    private void HandleAnimation()
    {
        // make sure that is players mouse is over the UI that the player doesnt start performing animations
        if (eventSys.IsPointerOverGameObject())
            return;

        // set the animation parameters of X and Y velocities to be equal to the absolute values of the parameters horizontal and vertical
        anim.SetFloat("VelocityX", Mathf.Abs(rigid.velocity.x));
        anim.SetFloat("VelocityY", Mathf.Abs(rigid.velocity.y));

        // if the current state of the player is melee
        if (melee)
        {
            if (!ripostePossible)
            {
                // if the player presses the left mouse button
                if (Input.GetMouseButtonUp(0))
                {
                    // stop the strike
                    StopCoroutine(CheckForHeldStrike());
                    largeStrike = false;

                    // check if player interacts with anything else in the environment
                    // if he does then run the appropriate functions and disallow hit animations
                    bool canHit = CheckIfPlayerMayHit();

                    // if you cannot hit then interact with the thing that is in your way
                    if (!canHit)
                    {
                        Vendor vendor = CheckForVendor();
                        Interactable inter = CheckForInteractable();

                        if (inter != null)
                        {
                            Debug.Log("Player controller trying to interact with: " + inter.gameObject.name);
                            inter.Interact();
                            return;
                        }

                        if (vendor != null)
                        {
                            vendor.Interact();
                            return;
                        }
                    }

                    // make sure maxChargedHit is set to false for every attempt at hitting 
                    // only set it to true if its true for this particular hit
                    maxChargedHit = false;

                    // check if player was rtying to to do a charged hit
                    if (largeStrikeAnimationReady)
                    {
                        anim.SetBool("StrikeHold", false);
                        largeStrikeAnimationReady = false;

                        if (EquipmentManager.instance.weaponGlowSlot.color.a > 0.98)
                        {
                            Debug.Log("Maximum Hit");
                            maxChargedHit = true;
                        }

                        Color tmp = EquipmentManager.instance.weaponGlowSlot.color;
                        tmp.a = 0;
                        EquipmentManager.instance.weaponGlowSlot.color = tmp;
                        weaponChargeReady = false;
                        weaponCharged.Stop();

                        return;
                    }

                    int rnd = UnityEngine.Random.Range(0, 4);

                    // run a switch case on rnd and play a random hit animation
                    switch (rnd)
                    {
                        case 0:
                            anim.SetTrigger("Strike1");
                            break;

                        case 1:
                            anim.SetTrigger("Strike2");
                            break;

                        case 2:
                            anim.SetTrigger("Strike3");
                            break;
                    }
                }
            }

            if (Input.GetMouseButton(1))
            {
                if (EquipmentManager.instance.currentEquipment[4] != null)
                {
                    anim.SetBool("Block", true);

                    StartCoroutine(TimedBlockInitialize());

                    if (!hasBlockingModifier)
                    {
                        blocking = true;
                        modifierToRemove = (int)playerStat.armor.GetValue();
                        playerStat.armor.AddModifier((int)playerStat.armor.GetValue());
                        hasBlockingModifier = true;
                    }
                }

            }

            if (!Input.GetMouseButton(1))
            {
                anim.SetBool("Block", false);
                if (hasBlockingModifier)
                {
                    blocking = false;
                    playerStat.armor.RemoveModifier(modifierToRemove);
                    hasBlockingModifier = false;
                }
            }

            if (Input.GetMouseButton(0))
            {
                // start checking if player wants to hold button in
                if (!heldStrikeCoroutinePlaying)
                {
                    StartCoroutine(CheckForHeldStrike());
                }
            }

            if (ripostePossible)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    anim.SetTrigger("Riposte");
                    ripostePossible = false;
                }
            }
        }


        // if the player is in a ranged state
        else if (ranged)
        {
            // if the player hits the left mouse button
            if (Input.GetMouseButton(0))
            {
                bool canHit = CheckIfPlayerMayHit();

                // if player is not allowed to hit the stop the code from running further
                if (!canHit)
                {
                    Vendor vendor = CheckForVendor();
                    Interactable inter = CheckForInteractable();

                    if (inter != null)
                    {
                        Debug.Log("Player controller trying to interact with: " + inter.gameObject.name);
                        inter.Interact();
                        return;
                    }

                    if (vendor != null)
                    {
                        vendor.Interact();
                        return;
                    }
                }

                // if player is allowed to hit then animate the shooting
                anim.SetTrigger("ShootRanged");
            }
        }

    } // commented

    /// <summary>
    /// Determines the speed at which the players weapon charges a power hit
    /// </summary>
    private void ChargeHit()
    {
        if (EquipmentManager.instance.weaponGlowSlot.color.a < 0.98)
        {
            Color tmp = EquipmentManager.instance.weaponGlowSlot.color;
            tmp.a += 0.009f;
            EquipmentManager.instance.weaponGlowSlot.color = tmp;
        }
    }

    private bool CheckIfPlayerMayHit()
    {
        // check if player hit something with the mouse when clicking that interacable
        Interactable inter = CheckForInteractable();
        Vendor vendor = CheckForVendor();

        // if the player did hit something interacable with the mouse then do not allow the player to hit
        if (inter != null || vendor != null)
            return false;

        // if the player does not have weapons in his hands
        if (EquipmentManager.instance.currentEquipment[3] == null)
            return false;

        // otherwise let him hit
        return true;
    } // commented

    private Vendor CheckForVendor()
    {
        Vector3 screenNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        Vector3 screenFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);

        Vector3 farPoint = Camera.main.ScreenToWorldPoint(screenFar);
        Vector3 nearPoint = Camera.main.ScreenToWorldPoint(screenNear);

        RaycastHit hit;

        if (Physics.Raycast(nearPoint, farPoint - nearPoint, out hit))
        {
            Vendor vendor = hit.collider.GetComponentInParent<Vendor>();

            if (vendor == null)
            {
                //mouseOverInteractableAndPlayerInRange = false;
                return null;
            }

            if (vendor != null)
            {
                // check if player is close enough to interact with the interactable
                float distanceFromInteractable = Vector2.Distance(hit.collider.gameObject.transform.position, transform.position);

                if (distanceFromInteractable < vendor.radius)
                {
                    //mouseOverInteractableAndPlayerInRange = true;
                    return vendor;
                }

                return null;
            }
            else
            {
                Debug.LogWarning("something wrong with the clickFeedback");
                return null;
            }
        }
        else
        {
            //RemoveFocus();
            SetMousePosition();
            return null;
        }


    }

    private Interactable CheckForInteractable()
    {
        Vector3 screenNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        Vector3 screenFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);

        Vector3 farPoint = Camera.main.ScreenToWorldPoint(screenFar);
        Vector3 nearPoint = Camera.main.ScreenToWorldPoint(screenNear);

        RaycastHit hit;

        if (Physics.Raycast(nearPoint, farPoint - nearPoint, out hit))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();

            if (interactable == null)
            {
                //mouseOverInteractableAndPlayerInRange = false;
                return null;
            }

            if (interactable != null)
            {
                // check if player is close enough to interact with the interactable
                float distanceFromInteractable = Vector2.Distance(hit.collider.gameObject.transform.position, transform.position);

                //Debug.Log(distanceFromInteractable + "   " + interactable.radius);

                if (distanceFromInteractable < interactable.MyRadius)
                {
                    //mouseOverInteractableAndPlayerInRange = true;
                    return interactable;
                }

                //mouseOverInteractableAndPlayerInRange = false;
                return null;
            }
            else
            {
                Debug.LogWarning("something wrong with the clickFeedback");
                return null;
            }
        }
        else
        {
            SetMousePosition();
            //mouseOverInteractableAndPlayerInRange = false;
            return null;
        }


    }

    /// <summary>
    /// Handles the movement of the Player with a normalized vector2
    /// </summary>
    private void HandleMovement()
    {
        if (interruptMovement)
            return;

        // instantiate a vector2 direction and set it to zero
        Vector2 directionOfMovement = Vector2.zero;

        // for every button the player presses add that vector to the previously created vector2
        if (Input.GetKey(KeybindManager.instance.Keybinds["RIGHT"]))
        {
            directionOfMovement += Vector2.right;
        }
        if (Input.GetKey(KeybindManager.instance.Keybinds["LEFT"]))
        {
            directionOfMovement += Vector2.left;
        }
        if (Input.GetKey(KeybindManager.instance.Keybinds["DOWN"]))
        {
            directionOfMovement += Vector2.down;
        }
        if (Input.GetKey(KeybindManager.instance.Keybinds["UP"]))
        {
            directionOfMovement += Vector2.up;
        }

        // once all buttons have been pressed normalize the vector2 and add force to it multiplied by speed
        rigid.AddRelativeForce(directionOfMovement.normalized * speed * Time.deltaTime);


        // jump forward
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playerStat.MyCurrentStamina >= 50)
            {
                playerStat.MyCurrentStamina -= 50;

                Debug.Log("calling ghost spawn");
                var trail = GetSystem("thrustTrail");
                trail.Play();
                anim.SetTrigger("Thrust");

                SpawnGhostImage(directionOfMovement);

                // if you're standing still move towards mnouse instead
                if (directionOfMovement == Vector2.zero)
                {
                    Vector3 directionMouse = new Vector3(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y, 0f);
                    directionMouse.Normalize();
                    rigid.AddRelativeForce(directionMouse * thrustSpeed);
                    return;
                }

                // add a relative local force to the player rigidbody in the given direction.
                rigid.AddRelativeForce(directionOfMovement * thrustSpeed);
            }

        }
    }

    /// <summary>
    /// Lerps the stamina Bar between its before cost and after values
    /// </summary>
    private void LerpStaminaBar()
    {
        staminaBar.fillAmount = Mathf.Lerp(staminaBar.fillAmount, 
            playerStat.CalculateHealth(playerStat.MyCurrentStamina, playerStat.MyMaxStamina), 
            Time.deltaTime * 8);
    }

    private void HandleActionBarInput()
    {
        if (Input.GetKeyDown(KeybindManager.instance.ActionBinds["ACTION3"]))
        {
            UiManager.instance.ClickActionButton("ACTION1");
        }
        else if (Input.GetKeyDown(KeybindManager.instance.ActionBinds["ACTION4"]))
        {
            UiManager.instance.ClickActionButton("ACTION2");
        }
        else if (Input.GetKeyDown(KeybindManager.instance.ActionBinds["ACTION5"]))
        {
            UiManager.instance.ClickActionButton("ACTION3");
        }

        //foreach (string action in KeybindManager.instance.ActionBinds.Keys)
        //{
        //    Debug.Log("searching " + action);
        //    if (Input.GetKeyDown(KeybindManager.instance.ActionBinds[action]))
        //    {
        //        Debug.Log("Pressing: " + KeybindManager.instance.ActionBinds[action].ToString());
        //        UiManager.instance.ClickActionButton(action);
        //    }
        //}
    }

    private void HandleCombatState()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("calling loadscene");
            SceneManager.LoadSceneAsync(0);
            transform.position = new Vector2(-12f, 4);
        }

        if (Input.GetKeyDown(KeybindManager.instance.ActionBinds["ACTION1"])) // ACTION1 is trying to use Melee
        {
            EquipFirstMatchingItemInBag(0,3);
            EquipFirstMatchingItemInBag(2,4);
        }

        if (Input.GetKeyDown(KeybindManager.instance.ActionBinds["ACTION2"])) // ACTION2 is trying to use Ranged
        {
            EquipFirstMatchingItemInBag(1,3);
        }
    }

    private void SpawnGhostImage(Vector2 direction)
    {
        // set a vector2 Position that we can use to spawn the ghost at
        Vector2 spawnPoint = new Vector2(transform.position.x, transform.position.y + 0.452f);

        // spawn the ghost at the aforementioned spawnpoint
        var image = Instantiate(skeleton, spawnPoint, Quaternion.identity);

        // enable the collider on the skeleton
        image.GetComponent<CircleCollider2D>().enabled = true;

        // if player is not facing right flip the image
        if (!facingRight)
        {
            image.transform.localScale = new Vector2(image.transform.localScale.x * -1, image.transform.localScale.y);
        }

        // make a ref to the rigidbody on the ghost
        var imageRigid = image.GetComponent<Rigidbody2D>();

        // set the rigidbody to be dynamic
        imageRigid.bodyType = RigidbodyType2D.Dynamic;

        // add a force to the ghost in the direction the player is moving
        imageRigid.AddForce(direction * thrustSpeed / 1.5f);

        // make sure the ghost as well as all its children fade away
        image.AddComponent<Fade>();
    } // commented

    /// <summary>
    /// Looks through Inventory and checks for an item 
    /// that matches the Parameters passed to this function
    /// </summary>
    /// <param name="type">Type of item to look for "EquipmentType" Enum located on Equipment script</param>
    /// <param name="slotIndex">Slot that the item fits "EquipmentSlot" Enum located on Equipment script</param>
    /// <returns></returns>
    private static bool EquipFirstMatchingItemInBag(int type, int slotIndex)
    {
        //for all items in your bag
        foreach (SlotScript slot in InventoryScript.instance.GetAllSlots())
        {
            if (slot.MyItem is Equipment)
            {
                //Debug.Log("This Slots item is " + slot.MyItem.name + " and its type of equipment is " + slot.MyItem.typeOfEquipment);
                Equipment tmp = slot.MyItem as Equipment;

                if ((int)tmp.equipType == type && (int)tmp.equipSlot == slotIndex)
                {
                    slot.MyItem.Use();
                }
            }
        }
            //{
                //// check to see if you have equipment in your bag
                //if (InventoryScript.instance.sl is Equipment)
                //{
                //    // set tmp as the equipment found
                //    Equipment tmp = (Equipment)Inventory.instance.itemsInBag[i];

                //    // is tmp a Matching item?
                //    if ((int)tmp.equipSlot == slot)
                //    {
                //        // equip the first Matching item you find
                //        if ((int)tmp.equipType == type)
                //        {
                //            Inventory.instance.itemsInBag[i].Use();
                //            return true;
                //        }
                //    }
                //}
            //}
        return false;
    }

    private void CheckIfFacingCorrectDirection()
    {
        float distanceFromFrontToMouse, distanceFromBackToMouse;
        CheckDistancesToMouse(out distanceFromFrontToMouse, out distanceFromBackToMouse);

        if (distanceFromFrontToMouse > distanceFromBackToMouse + 0.05f )
        {
            FlipPlayer();
        }

    }

    public void FlipPlayer()
    {
        if (isDead)
            return;
        ////////////////////////////////////
        //Gather obects not to be flipped///
        ////////////////////////////////////

        //CanvasGroup healthImage = healthGroup;

        //Vector3 healthPos = healthImage.gameObject.transform.position;

        //healthImage.transform.SetParent(null);

        ////////////////////////////////////
        //////////Flip the objects//////////
        ////////////////////////////////////
        Vector3 theScale = transform.localScale;

        theScale.x *= -1;

        facingRight = !facingRight;

        transform.localScale = theScale;

        //healthImage.transform.SetParent(transform);
        //healthImage.transform.position = healthPos;
    }

    public void KnockBack(float amount)
    {
        interruptMovement = true;

        if (facingRight)
        {
            rigid.AddRelativeForce(Vector2.left * amount);
            return;
        }

        rigid.AddRelativeForce(Vector2.right * amount);
    }

    public void OnCastComplete()
    {
        float distanceFromFrontToMouse, distanceFromBackToMouse;
        CheckDistancesToMouse(out distanceFromFrontToMouse, out distanceFromBackToMouse);

        SoundManager.instance.PlayCombatSound("bow");

        var startPos = projectilePoint.transform.position;

        // create a direction vector from Hit position of the mouse and the projectiles original position
        Vector2 direction = new Vector2(mousePosition.x - startPos.x, mousePosition.y - startPos.y);
        direction.Normalize();

        // Determine the correct angle to turn to the projectile
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject projectile = pooledArrows.GetPooledArrow();

        var projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.MakeProjectileReady();

        projectile.transform.position = projectilePoint.transform.position;
        projectile.transform.rotation = Quaternion.identity;
        projectile.transform.localScale = new Vector3(1, 1, 1);


        projectile.transform.Rotate(0, 0, angle, Space.World);

        // addforce force to the projectiles rigidbody in that direction.
        projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);

        GameDetails.arrowsFired++;
    }

    public void OnStrikeComplete()
    {
        if (equip.visibleGear[3].sprite == null)
            return;

        SoundManager.instance.PlayCombatSound("bladeSwing");

        //create layer masks for the player
        int enemyLayer = 11;
        var enemyLayerMask = 1 << enemyLayer;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(meleeStartPoint.position, 0.7f, enemyLayerMask);

        if (colliders.Length != 0)
        {
            foreach (Collider2D enemy in colliders)
            {
                if (enemy.gameObject.transform.parent.GetComponent<EnemyStats>() != null)
                {
                    if (!enemy.gameObject.transform.parent.GetComponent<EnemyAI>().isDead)
                    {
                        // play the impact particles that belongs to this enemy
                        ParticleSystemHolder.instance.PlayImpactEffect(enemy.transform.parent.name + "_impact", enemy.transform.position);

                        if (maxChargedHit)
                        {
                            enemy.gameObject.transform.parent.GetComponent<EnemyStats>().TakeDamage(playerStat.damage.GetValue() * 2);
                            GameDetails.fullChargeHits += 1;
                            return;
                        }
                        enemy.gameObject.transform.parent.GetComponent<EnemyStats>().TakeDamage(playerStat.damage.GetValue());
                        GameDetails.hits += 1;
                    }
                }
            }
        }

    }

    private void CheckDistancesToMouse(out float distanceFromFrontToMouse, out float distanceFromBackToMouse)
    {
        // get the Vector2 Position fof the mouse
        mousePosition = SetMousePosition();

        // check the distance from the "front" gameobject on the player to the mouse
        distanceFromFrontToMouse = Vector3.Distance(projectilePoint.transform.position, mousePosition);

        // check the distance from the "back" gameobject on the player to the mouse
        distanceFromBackToMouse = Vector3.Distance(back.position, mousePosition);
    }

    private Vector2 SetMousePosition()
    {
        //Create a ray from the Mouse click position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Initialise the enter variable
        float enter = 0.0f;

        // shoot the ray at the plane and return the enter float
        if (m_Plane.Raycast(ray, out enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = ray.GetPoint(enter);

            // set mouseposition to the position hit.
            mousePosition = hitPoint;
        }

        // return the place hit
        return mousePosition;
    } // commented

    /// <summary>
    /// Make sures all references are set for the player
    /// </summary>
    private void InitializePlayerGameObject()
    {
        // Set all necessary references
        rigid = gameObject.GetComponent<Rigidbody2D>();
        playerStat = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
        equip = EquipmentManager.instance;
        m_Plane = GameDetails._instance.m_Plane;
        pooledArrows = PooledProjectilesController.instance;
        cameraControl = CameraController.instance;

        // set the lookat on the camera controller to be the playerObject.
        cameraControl.lookAt = this.gameObject;

        // fetch all particleSystems that are children of this gameObject and place them in an array of particleSystems
        particles = GetComponentsInChildren<ParticleSystem>();

        // Find the child GameObject called "Skeleton" and make a reference to it
        skeleton = transform.Find("Skeleton").gameObject;

        // Find the object called "EventSystem" and make a reference to it
        eventSys = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        // make sure that the camera is always looking at the player
        if (cameraControl.lookAt != this.gameObject)
        {
            cameraControl.lookAt = this.gameObject;
        }
    }

    private ParticleSystem GetSystem(string systemName)
    {
        // run through all the particle systems in the list of particle systems.
        foreach (ParticleSystem system in particles)
        {
            // if its the one that matches the parameter that has been passed in
            if (system.name == systemName)
            {
                // the return the system
                return system;
            }
        }

        // if nothing matches the parameter then return nothing
        return null;
    }  // commented

    private void OnDrawGizmosSelected()
    {
        // set the color of the gizmo
        Gizmos.color = Color.yellow;
        // set the size of the gizmo ( this one relates to the range at which the player will consider enemies to be "in range")
        Gizmos.DrawWireSphere(transform.position, 25);

        Gizmos.DrawWireSphere(meleeStartPoint.position, 0.5f);
    } // commented

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    private void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnRiposte()
    {

        int enemyLayer = 11;
        var enemyLayerMask = 1 << enemyLayer;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(meleeStartPoint.position, 0.7f, enemyLayerMask);

        if (colliders.Length != 0)
        {
            foreach (Collider2D enemy in colliders)
            {
                if (enemy.gameObject.transform.parent.GetComponent<EnemyAI>() != null)
                {
                    // create a direction vector from Hit position of the mouse and the projectiles original position
                    Vector2 direction = new Vector2(enemy.transform.position.x - transform.position.x, enemy.transform.position.y - transform.position.y);
                    direction.Normalize();


                    if (!enemy.gameObject.transform.parent.GetComponent<EnemyAI>().isDead)
                    {
                        SoundManager.instance.PlayCombatSound("shieldriposte");
                        GameDetails.ripostes += 1;
                        enemy.gameObject.transform.parent.GetComponent<EnemyAI>().Knockback(direction);
                        enemy.gameObject.transform.parent.GetComponent<EnemyAI>().Stunned();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Temporarily turns on the ability to riposte an attack
    /// </summary>
    /// <returns></returns>
    public IEnumerator Riposte()
    {
        ripostePossible = true;

        // TODO warn Player that riposte is available
        var text = CombatTextManager.instance.FetchText(transform.position);
        var textScript = text.GetComponent<CombatText>();
        textScript.White("Riposte!", transform.position);

        yield return new WaitForSeconds(riposteTime);

        ripostePossible = false;
    }

    /// <summary>
    /// Temporarily opens a window called "timedBlock" for use in combat system
    /// </summary>
    /// <returns></returns>
    IEnumerator TimedBlockInitialize()
    {
        timedBlock = true;

        yield return new WaitForSeconds(blockTime);

        timedBlock = false;
    }

    /// <summary>
    /// Checks to see if player is holding in the hit key
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckForHeldStrike()
    {
        // coroutine is running
        heldStrikeCoroutinePlaying = true;

        // set a bool to true at the start of the routine
        largeStrike = true;

        // wait for 0.3 seconds
        yield return new WaitForSeconds(0.3f);

        // if the boolean is still true (gets set to false if the button is released)
        if (largeStrike)
        {
            // then set the animation state to true
            largeStrikeAnimationReady = true;
            // set the animation of the weapon above their head
            anim.SetBool("StrikeHold", true);
        }

        // let the script know the routine is done running
        heldStrikeCoroutinePlaying = false;
    }

}

