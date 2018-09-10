using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System;
using EZCameraShake;

public class PlayerController : MonoBehaviour
{
    // plane needed for hitray during projectiles
    // located on Gamemanager since monsters also need access to it.
    Plane m_Plane;
    [SerializeField] bool ripostePossible, enemiesInRange, lineOfSightRoutineActivated, largeStrike, largeStrikeAnimationReady, 
                          heldStrikeCoroutinePlaying, chargedShot, autoFiring, portal;
    [SerializeField] float riposteTime, chargeTime, blockTime = 0;

    public bool maxChargedHit;

    /// <summary>
    /// Used to stop Player from shooting and hitting when mousing over a vendor or items on the ground
    /// </summary>
    public bool mouseOverInteractable;
    public bool mouseOverItem;

    Vector3 prevPosition;
    Vector3 move;
    [SerializeField] Light pointLight;

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
    }
    #endregion

    Vector2 mousePosition;
    EventSystem eventSys;

    Rigidbody2D rigid;
    CameraController cameraControl;
    PlayerStats playerStat;
    ParticleSystem[] particles;
    [SerializeField] ParticleSystem sprintTrail;

    #region Public Variables

    /// <summary>
    /// Used for Particle effect to spawn on player and avoid the flipping
    /// </summary>
    public Transform avoidFlip;


    /// <summary>
    /// Accessed by the grass to turn it off when player leaves the sand and enters Grass
    /// </summary>
    public ParticleSystem footPrints;

    /// <summary>
    /// Accessed by the grass to turn it off when player leaves the sand and enters Grass
    /// </summary>
    public ParticleSystem sandTrail;


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
    public float speed;

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

    /// <summary>
    /// Used for when talents increase the level of 
    /// projectile and therefore slightly increases speed
    /// </summary>
    public float MyProjectileSpeed
    {
        get
        {
            return projectileSpeed;
        }
        set
        {
            projectileSpeed = value;
        }
    }

    /// <summary>
    /// Used by the Portals to stop the player from moving when he enters them.
    /// </summary>
    public bool Portal
    {
        get
        {
            return portal;
        }

        set
        {
            portal = value;
        }
    }
    #endregion

    #region State/etc
    public Text stateText;          // to give player feedback of what state they are in
    public bool melee = true;       // is the player in melee?
    public bool ranged, dialogue;   // is the player using ranged or in a dialogue? Player can still animate during pause due to unscaled time on his animator

    EquipmentManager equip;
    DungeonManager dungeon;

    //blockState
    bool hasBlockingModifier;           // To make sure armor modifer only get added once when block is clicked
    int modifierToRemove;           // actual amount of extra damage that is absorbed when blocking
    public bool blocking;
    #endregion

    [SerializeField] ParticleSystem weaponCharged;
    [SerializeField] ParticleSystem bowCharged;
    bool weaponChargeReady;
    float percentageCostOfChargingBow;
    float determineChargeCost;

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
        if (isDead || GameDetails.instance.paused || dialogue || Portal)
            return;

        HandleCombatState();
        HandleActionBarInput();
        HandleAnimation();

        CheckIfFacingCorrectDirection();
        HandleMovement();

        if (staminaBar.fillAmount != playerStat.CalculateStamina(playerStat.MyCurrentStamina, playerStat.MyMaxStamina))
        {
            playerStat.LerpStaminaBar();
        }

        if (Time.frameCount % 3 == 0)
        {
            HandleAggro();
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

        if (chargedShot)
        {
            ChargeBowShot();

            if (EquipmentManager.instance.weaponGlowSlot.color.a > 0.98 && !weaponChargeReady)
            {
                weaponChargeReady = true;
                bowCharged.Play();
            }
        }
     
    }

    /// <summary>
    /// Determines the speed at which the players weapon charges a power hit
    /// </summary>
    private void ChargeHit()
    {
        if (melee)
        {
            if (EquipmentManager.instance.weaponGlowSlot.color.a < 0.98)
            {
                Color tmp = EquipmentManager.instance.weaponGlowSlot.color;
                tmp.a += 0.009f;
                EquipmentManager.instance.weaponGlowSlot.color = tmp;
            }
        }
    }

    /// <summary>
    /// Controls all the logic for the charing of the bow
    /// </summary>
    private void ChargeBowShot()
    {
        if (!playerStat.cantRegen)
        {
            playerStat.cantRegen = true;
        }

        /// if you have enough stamina you can start charging
        if (playerStat.chargeHeld)
        {
            if (EquipmentManager.instance.weaponGlowSlot.color.a < 0.98)
            {
                /// Reset percent cost determiner
                if (EquipmentManager.instance.weaponGlowSlot.color.a < 0.02)
                {
                    determineChargeCost = 0;
                }

                /// CHange the sprite of the item
                Color tmp = EquipmentManager.instance.weaponGlowSlot.color;
                tmp.a += 0.009f;
                EquipmentManager.instance.weaponGlowSlot.color = tmp;

                /// Charge the player stamina per frame
                playerStat.MyCurrentStamina -= 0.3f;

                /// Determine what that number becomes
                determineChargeCost += 0.3f;

                /// If its higher than normal value then set this value to the new value
                if (determineChargeCost > percentageCostOfChargingBow)
                {
                    percentageCostOfChargingBow = determineChargeCost;
                }
            }
        }

        // if you run out of stamina you lose your charge
        if (playerStat.MyCurrentStamina <= 0)
        {
            Color tmp = EquipmentManager.instance.weaponGlowSlot.color;
            tmp.a = 0;
            EquipmentManager.instance.weaponGlowSlot.color = tmp;
            weaponChargeReady = false;
            bowCharged.Stop();
        }

        if (equip.currentEquipment[3] != null)
        {
            if ((int)equip.currentEquipment[3].equipType != 1)
            {
                StopRangedChargeShotState();
                chargedShot = false;
                return;
            }
        }

        if (equip.currentEquipment[3] == null)
        {
            StopRangedChargeShotState();
            chargedShot = false;
            return;
        }

    }

    /// <summary>
    /// Initiates the aggro by making a circle around the player
    /// within which the mobs will check their aggro scripts
    /// </summary>
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
                if (!enemies.Contains(enemy.transform.parent.gameObject) && !enemy.transform.parent.GetComponent<EnemyAI>().isDead)
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
    }

    /// <summary>
    /// Handles animation of the player, 
    /// primarily hitting and shooting
    /// </summary>
    private void HandleAnimation()
    {

        // set the animation parameters of X and Y velocities to be equal to the absolute values of the parameters horizontal and vertical
        anim.SetFloat("VelocityX", Mathf.Abs(rigid.velocity.x));
        anim.SetFloat("VelocityY", Mathf.Abs(rigid.velocity.y));

        /// Ensure that if player's mouse is over the UI that the player doesnt start performing animations
        if (eventSys.IsPointerOverGameObject())
        {
            return;
        }

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
                            float distanceFromInteractable = Vector2.Distance(inter.gameObject.transform.position, transform.position);

                            if (distanceFromInteractable < inter.MyRadius)
                            {
                                inter.Interact();
                            }

                            return;
                        }

                        if (vendor != null)
                        {
                            float distanceFromInteractable = Vector2.Distance(vendor.gameObject.transform.position, transform.position);

                            if (distanceFromInteractable < vendor.MyRadius)
                            {
                                vendor.Interact();
                            }

                            return;
                        }
                    }

                    // make sure maxChargedHit is set to false for every attempt at hitting 
                    // only set it to true if its true for this particular hit
                    maxChargedHit = false;

                    ///If you have a weapon Equipped
                    if (equip.currentEquipment[3] != null)
                    {
                        // check if player was rtying to to do a charged hit
                        if (largeStrikeAnimationReady)
                        {
                            anim.SetBool("StrikeHold", false);
                            largeStrikeAnimationReady = false;

                            if (EquipmentManager.instance.weaponGlowSlot.color.a > 0.98)
                            {
                                //Debug.Log("Maximum Hit");
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
            }

            if (Input.GetMouseButton(1))
            {
                if (equip.currentEquipment[4] != null)
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
                if (blocking)
                {
                    anim.SetBool("Block", false);
                    if (hasBlockingModifier)
                    {
                        blocking = false;
                        playerStat.armor.RemoveModifier(modifierToRemove);
                        hasBlockingModifier = false;
                    }
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
            if (Input.GetMouseButtonDown(0))
            {
                bool canHit = CheckIfPlayerMayHit();

                if (!canHit)
                {
                    Vendor vendor = CheckForVendor();
                    Interactable inter = CheckForInteractable();

                    if (inter != null)
                    {
                        float distanceFromInteractable = Vector2.Distance(inter.gameObject.transform.position, transform.position);

                        if (distanceFromInteractable < inter.MyRadius)
                        {
                            inter.Interact();
                        }

                        return;
                    }

                    if (vendor != null)
                    {
                        float distanceFromInteractable = Vector2.Distance(vendor.gameObject.transform.position, transform.position);

                        if (distanceFromInteractable < vendor.MyRadius)
                        {
                            vendor.Interact();
                        }

                        return;
                    }
                }
            }

            if (Input.GetMouseButton(0))
            {
                if (!mouseOverInteractable && !mouseOverItem)
                {
                    if (equip.currentEquipment[3] != null)
                    {
                        maxChargedHit = false;

                        if (!autoFiring)
                        {
                            StartCoroutine(AutoFire());
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                autoFiring = false;
            }

            // if the player hits the left mouse button
            if (Input.GetMouseButtonDown(1))
            {
                bool canHit = CheckIfPlayerMayHit();

                // if player is not allowed to hit the stop the code from running further
                if (!canHit)
                {
                    Vendor vendor = CheckForVendor();
                    Interactable inter = CheckForInteractable();

                    if (inter != null)
                    {
                        float distanceFromInteractable = Vector2.Distance(inter.gameObject.transform.position, transform.position);

                        if (distanceFromInteractable < inter.MyRadius)
                        {
                            inter.Interact();
                        }

                        return;
                    }

                    if (vendor != null)
                    {
                        float distanceFromInteractable = Vector2.Distance(vendor.gameObject.transform.position, transform.position);

                        if (distanceFromInteractable < vendor.MyRadius)
                        {
                            vendor.Interact();
                        }

                        return;
                    }
                }

                // if player is allowed to hit then animate the shooting

            }

            if (Input.GetMouseButtonUp(1))
            {
                if (playerStat.chargeHeld)
                {
                    playerStat.chargeHeld = false;
                }

                if (playerStat.cantRegen)
                {
                    playerStat.cantRegen = false;
                }

                autoFiring = false;
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

                else if (canHit)
                {
                    largeStrike = false;

                    // make sure maxChargedHit is set to false for every attempt at hitting 
                    // only set it to true if its true for this particular hit
                    maxChargedHit = false;

                    // check if player was trying to to do a charged hit
                    if (chargedShot)
                    {
                        StopRangedChargeShotState();

                        return;
                    }
                }

            }

            if (Input.GetMouseButton(1))
            {
                // start checking if player wants to hold button in
                if (!heldStrikeCoroutinePlaying && !chargedShot && !autoFiring && !VendorWindow.IsOpen)
                {
                    StartCoroutine(CheckForHeldStrike());
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && !UiManager.instance.toolTip.activeInHierarchy)
        {
            mouseOverItem = false;
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


        // Start sprinting
        if (Input.GetKey(KeyCode.Space))
        {
            if (playerStat.MyCurrentStamina > 0)
            {
                if (!playerStat.sprinting)
                {
                    speed *= 2;
                    sprintTrail.Play();
                    anim.SetBool("Sprint", true);
                    playerStat.sprinting = true;
                }
            }
            else
            {
                if (playerStat.sprinting)
                {
                    speed *= 0.5f;
                    sprintTrail.Stop();
                    anim.SetBool("Sprint", false);
                    playerStat.sprinting = false;
                }
            }

            playerStat.MyCurrentStamina -= 0.22f;

        }

        // Stop sprinting
        if (!Input.GetKey(KeyCode.Space))
        {
            if (playerStat.sprinting)
            {
                speed *= 0.5f;
                sprintTrail.Stop();
                anim.SetBool("Sprint", false);
                playerStat.sprinting = false;
            }
        }
    }

    /// <summary>
    /// Handles what happens when the action buttons are clicked
    /// </summary>
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

    /// <summary>
    /// Handles changing the state of the player between
    /// Ranged state and Melee State
    /// </summary>
    private void HandleCombatState()
    {
        if (Input.GetKeyDown(KeybindManager.instance.ActionBinds["ACTION1"])) // ACTION1 is trying to use Melee
        {
            if (!melee)
            {
                StopRangedChargeShotState(true);

                bool sword = EquipFirstMatchingItemInBag(0, 3);
                EquipFirstMatchingItemInBag(2, 4);

                if (sword)
                {
                    melee = true;
                    ranged = false;
                    playerStat.cantRegen = false;
                }

            }

        }

        if (Input.GetKeyDown(KeybindManager.instance.ActionBinds["ACTION2"])) // ACTION2 is trying to use Ranged
        {
            if (blocking)
            {
                anim.SetBool("Block", false);
            }
            if (!ranged)
            {
                StopMeleeChargedHitState(true);

                bool rangedWeap = EquipFirstMatchingItemInBag(1, 3);

                if (rangedWeap)
                {
                    ranged = true;
                    melee = false;
                    playerStat.cantRegen = false;
                }
            }
        }
    }

    /// <summary>
    /// Stops the state of charging the bow
    /// </summary>
    /// <param name="changingState">In case we wish to change the state due to coming from the 
    /// melee state and not because the player has fired his arrows</param>
    private void StopRangedChargeShotState(bool changingState = false)
    {
        if (!changingState)
        {
            anim.SetBool("ChargedShot", false);
        }
        else if (changingState)
        {
            anim.SetTrigger("abandonCharge");
            anim.SetBool("ChargedShot", false);
        }

        chargedShot = false;

        if (EquipmentManager.instance.weaponGlowSlot.color.a > 0.98 && !changingState)
        {
            maxChargedHit = true;
        }

        Color tmp = EquipmentManager.instance.weaponGlowSlot.color;
        tmp.a = 0;
        EquipmentManager.instance.weaponGlowSlot.color = tmp;
        weaponChargeReady = false;
        bowCharged.Stop();


        if (playerStat.chargeHeld)
        {
            playerStat.chargeHeld = false;
        }
    }

    /// <summary>
    /// Stops the state of charging a large hit
    /// </summary>
    /// <param name="changingState">In case we wish to change the state 
    /// due to coming from the ranged state </param>
    private void StopMeleeChargedHitState(bool changeingState = false)
    {
        maxChargedHit = false;
        largeStrike = false;

        // check if player was trying to to do a charged hit
        if (largeStrikeAnimationReady)
        {
            anim.SetBool("StrikeHold", false);
            largeStrikeAnimationReady = false;

            Color tmp = EquipmentManager.instance.weaponGlowSlot.color;
            tmp.a = 0;
            EquipmentManager.instance.weaponGlowSlot.color = tmp;
            weaponChargeReady = false;
            weaponCharged.Stop();

            return;
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
    }

    private Vendor CheckForVendor()
    {
        Vector3 screenNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        Vector3 screenFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);

        Vector3 nearPoint = Camera.main.ScreenToWorldPoint(screenNear);
        Vector3 farPoint = Camera.main.ScreenToWorldPoint(screenFar);

        RaycastHit hit;

        if (Physics.Raycast(nearPoint, farPoint - nearPoint, out hit))
        {
            Vendor vendor = hit.collider.GetComponentInParent<Vendor>();

            if (vendor == null)
            {
                return null;
            }

            if (vendor != null)
            {
                return vendor;
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
            return null;
        }


    }

    /// <summary>
    /// Checks to see if the player hit an interactable witrh a mouse or not
    /// if it did it retuirns the interactable
    /// </summary>
    /// <returns></returns>
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
                return null;
            }

            else if (interactable != null)
            {
                return interactable;
            }


            Debug.LogWarning("something wrong with the clickFeedback");
            return null;
        }

        SetMousePosition();
        return null;
    }

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

                    break;
                }

            }

        }

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

        Vector3 avoidPos = avoidFlip.position;

        avoidFlip.SetParent(null);


        ////////////////////////////////////
        //////////Flip the objects//////////
        ////////////////////////////////////


        Vector3 theScale = transform.localScale;

        theScale.x *= -1;

        facingRight = !facingRight;

        transform.localScale = theScale;

        ////////////////////////////////////
        //////////Replace Objects///////////
        ////////////////////////////////////

        avoidFlip.SetParent(transform);
        avoidFlip.position = avoidPos;
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

        // Determine the correct angle to turn the projectile
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject projectile = pooledArrows.GetPooledArrow();

        var projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.PrepareProjectileState();

        projectile.transform.position = projectilePoint.transform.position;
        projectile.transform.rotation = Quaternion.identity;
        projectile.transform.localScale = new Vector3(1, 1, 1);


        projectile.transform.Rotate(0, 0, angle, Space.World);

        if (maxChargedHit)
        {
            //----------------------------------------------------------------

            GameObject projectile1 = pooledArrows.GetPooledArrow();

            var projectileScript1 = projectile1.GetComponent<Projectile>();

            projectileScript1.PrepareProjectileState();
            PositionAndTurnProjectile(angle, projectile1);

            //----------------------------------------------------------------

            GameObject projectile2 = pooledArrows.GetPooledArrow();

            var projectileScript2 = projectile2.GetComponent<Projectile>();
            projectileScript2.PrepareProjectileState();
            PositionAndTurnProjectile(angle, projectile2);

            // addforce force to the projectiles rigidbody in that direction.
            projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
            projectile1.GetComponent<Rigidbody2D>().AddForce(direction.Rotate(5f) * projectileSpeed);
            projectile2.GetComponent<Rigidbody2D>().AddForce(direction.Rotate(355f) * projectileSpeed);

            Instantiate(ParticleSystemHolder.instance.ChargedBowShot, projectile.transform);
            Instantiate(ParticleSystemHolder.instance.ChargedBowShot, projectile1.transform);
            Instantiate(ParticleSystemHolder.instance.ChargedBowShot, projectile2.transform);

            GameDetails.arrowsFired++;

            return;
        }

        // addforce force to the projectiles rigidbody in that direction.
        projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);

        GameDetails.arrowsFired++;
    }

    private void PositionAndTurnProjectile(float angle, GameObject projectile)
    {
        projectile.transform.position = projectilePoint.transform.position;
        projectile.transform.rotation = Quaternion.identity;
        projectile.transform.localScale = new Vector3(1, 1, 1);

        projectile.transform.Rotate(0, 0, angle, Space.World);
    }

    public void OnStrikeComplete()
    {
        if (equip.visibleGear[3].sprite == null)
            return;

        SoundManager.instance.PlayCombatSound("bladeSwing");

        //create layer masks for the player
        int enemyLayer = 11;
        var enemyLayerMask = 1 << enemyLayer;

        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(meleeStartPoint.position, 0.7f, enemyLayerMask);

        int destructableLayer = 19;
        var destructableLayerMask = 1 << destructableLayer;

        Collider2D[] destructableColliders = Physics2D.OverlapCircleAll(meleeStartPoint.position, 0.7f, destructableLayerMask);

        if (enemyColliders.Length > 0)
        {
            foreach (Collider2D target in enemyColliders)
            {
                if (target.gameObject.transform.parent.GetComponent<EnemyStats>() != null)
                {
                    if (!target.gameObject.transform.parent.GetComponent<EnemyAI>().isDead)
                    {
                        // play the impact particles that belongs to this enemy
                        ParticleSystemHolder.instance.PlayImpactEffect(target.transform.parent.name + "_impact", target.transform.position);
                        target.gameObject.transform.parent.GetComponent<EnemyAI>().Knockback(target.transform.position - transform.position);

                        if (maxChargedHit)
                        {
                            target.gameObject.transform.parent.GetComponent<EnemyStats>().TakeDamage(playerStat.damage.GetValue() * 2);
                            GameDetails.fullChargeHits += 1;
                            return;
                        }

                        target.gameObject.transform.parent.GetComponent<EnemyStats>().TakeDamage(playerStat.damage.GetValue());
                        GameDetails.hits += 1;
                    }
                }
            }
        }

        if (destructableColliders.Length > 0)
        {
            foreach (Collider2D target in destructableColliders)
            {
                if (target.gameObject.GetComponent<Destructable>() != null)
                {
                    target.gameObject.GetComponent<Destructable>().Impact();
                }
            }
        }
    }

    private void CheckDistancesToMouse(out float distanceFromFrontToMouse, out float distanceFromBackToMouse)
    {
        // get the Vector2 Position fof the mouse
        mousePosition = SetMousePosition();

        // check the distance from the "front" gameobject on the player to the mouse
        distanceFromFrontToMouse = Vector3.Distance(back.parent.transform.position, mousePosition);

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
    /// Ensures all references are set for the player
    /// </summary>
    private void InitializePlayerGameObject()
    {
        // Set all necessary references
        rigid = gameObject.GetComponent<Rigidbody2D>();
        playerStat = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
        equip = EquipmentManager.instance;
        
        m_Plane = GameDetails.instance.m_Plane;
        pooledArrows = PooledProjectilesController.instance;
        cameraControl = CameraController.instance;

        staminaBar = GameDetails.instance.transform.Find("UiCanvas/VisibleUi/StaminaBar/Stamina_Bar/Stamina_Fill").GetComponent<Image>();
        healthBar = GameDetails.instance.transform.Find("UiCanvas/VisibleUi/HealthBar/Health_Bar/Health_Fill").GetComponent<Image>();

        // set the lookat on the camera controller to be the playerObject.
        cameraControl.lookAt = this.gameObject.transform;

        // fetch all particleSystems that are children of this gameObject and place them in an array of particleSystems
        particles = GetComponentsInChildren<ParticleSystem>();

        // Find the object called "EventSystem" and make a reference to it
        eventSys = GameObject.Find("EventSystem").GetComponent<EventSystem>();
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

    bool spawnInRunning;
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (portal && !spawnInRunning)
        {
            dungeon = DungeonManager.instance;

            Item tmp = InventoryScript.instance.FindItemInInventory("Boss Room Key");

            if (tmp != null)
            {
                tmp.Remove();
            }

            dungeon.townPortalDropped = false;
            dungeon.dungeonReady = false;

            StartCoroutine(SpawnIn());
        }

        if (!scene.name.EndsWith("_indoor"))
        {
            pointLight.gameObject.SetActive(false);
            DungeonManager.dungeonLevel = 0;
        }

        else if (scene.name.EndsWith("_indoor"))
        {
            if (dungeon != null)
            {
                dungeon._PlayerHasBossKey = false;
                dungeon.bossKeyHasDropped = false;
                //dungeon.bossRoomAvailable = false;
            }
            Camera.main.transform.Find("daylight").GetComponent<Light>().intensity = 0.43f;
            pointLight.gameObject.SetActive(true);
        }

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

    public IEnumerator SpawnIn() // Made public for debugging only
    {
        spawnInRunning = true;

        SoundManager.instance.PlayEnvironmentSound("port_in");
        ParticleSystemHolder.instance.PlaySpellEffect(transform.position, "Spawn in");

        yield return new WaitForSeconds(2.5f);

        CameraShaker.Instance.ShakeOnce(6f, 6f, 0.3f, 0.9f);
        transform.Find("Skeleton").gameObject.SetActive(true);
        GameDetails.MyInstance.Save(SceneManager.GetActiveScene());
        portal = false;
        spawnInRunning = false;
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
        if (equip.currentEquipment[3] != null && !eventSys.IsPointerOverGameObject())
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
                if (melee)
                {
                    // then set the animation state to true
                    largeStrikeAnimationReady = true;
                    // set the animation of the weapon above their head
                    anim.SetBool("StrikeHold", true);
                }
                else if (ranged)
                {
                    if (playerStat.MyCurrentStamina > percentageCostOfChargingBow)
                    {
                        chargedShot = true;

                        playerStat.chargeHeld = true;

                        anim.SetBool("ChargedShot", true);
                    }


                }

            }

            else
            {
                if (melee)
                {

                }

                else if (ranged)
                {
                    anim.SetTrigger("ShootRanged");
                }
            }

            // let the script know the routine is done running
            heldStrikeCoroutinePlaying = false;

        }
    }

    /// <summary>
    /// Determines if the enemies in range of the player can see
    /// the player or not
    /// </summary>
    /// <returns></returns>
    IEnumerator LineOfSight()
    {
        // while you have enemies in the area
        while (enemiesInRange)
        {
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


    /// <summary>
    /// Ensures the player only fires 1 arrow if button is only pressed once
    /// but allows autofiring to also feel seamless, QoL effect that feel more
    /// natural
    /// </summary>
    /// <returns></returns>
    IEnumerator AutoFire()
    {
        autoFiring = true;

        anim.SetTrigger("ShootRanged");

        yield return new WaitForSeconds(0.5f);

        autoFiring = false;
    }


}

