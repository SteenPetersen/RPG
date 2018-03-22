using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System;

public class PlayerController : Interactable
{
    // plane needed for hitray during projectiles
    // located on Gamemanager since monsters also need access to it.
    Plane m_Plane;
    public List<GameObject> enemies = new List<GameObject>();
    bool enemiesInRange, lineOfSightRoutineActivated;

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

    public GameObject miniMap;
    public CoordinateDirection dir;

    public GameObject bodyLight;
    public GameObject skeleton;
    public Vector2 mousePosition;
    public EventSystem eventSys;
    public bool isDead, interruptMovement;
    public Interactable focus;
    [HideInInspector]
    public float normalSpeed = 0.8f;            // used to reset speeds after death etc
    public float speed;                         // actual speed of the character used for calculations
    public float thrustSpeed;                   // speed of the thrust forward

    Rigidbody2D rigid;
    CameraController cameraControl;
    PlayerStats playerStat;
    GameObject cameraHolder;
    public Animator anim;
    ParticleSystem[] particles;

    float horizontal;
    float vertical;

    [HideInInspector]
    public bool facingRight = true;

    #region Logic inside player Gameobject
    public Transform back;
    #endregion

    #region projectiles
    PooledProjectilesController pooledArrows;
    public GameObject projectilePoint;
    public float projectileSpeed;
    public Transform meleeStartPoint;
    Vector2 direction;
    #endregion

    #region State/etc
    public Text stateText;          // to give player feedback of what state they are in
    public bool melee = true;       // is the player in melee?
    public bool ranged, dialogue;   // is the player using ranged or in a dialogue? Player can still animate during pause due to unscaled time on his animator

    EquipmentManager equip;
    bool weaponsGone = false;       // has the player unequipped his/her weapons

    //blockState
    bool hasAddedModifer;           // To make sure armor modifer only get added once when block is clicked
    int modifierToRemove;           // actual amount of extra damage that is absorbed when blocking
    #endregion

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
        move = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (isDead || gameDetails.paused || dialogue)
            return;

        HandleAggro();
        HandleCombatState();
        HandleAnimation(horizontal, vertical);
    }

    void FixedUpdate()
    {
        // we do this in fixedupdate to mke sure that camera movement updates happen in the same frame as the actual movement
        if (isDead || gameDetails.paused || dialogue)
            return;

        CheckIfFacingCorrectDirection(horizontal);
        HandleMovement(horizontal, vertical);
    }

    private void LateUpdate()
    {
        // set the previous position to be the current position but do it in the late update so we can draw
        // direction for the Ghost
        prevPosition = transform.position;
    } // commented

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
        while(enemiesInRange)
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

    private void HandleAnimation(float horizontal, float vertical)
    {
        // make sure that is players mouse is over the UI that the player doesnt start performing animations
        if (eventSys.IsPointerOverGameObject())
            return;

        // set the animation parameters of X and Y velocities to be equal to the absolute values of the parameters horizontal and vertical
        anim.SetFloat("VelocityX", Mathf.Abs(horizontal));
        anim.SetFloat("VelocityY", Mathf.Abs(vertical));

        // if the current state of the player is melee
        if (melee)
        {
            // if the player presses the left mouse button
            if (Input.GetMouseButtonDown(0))
            {
                // check if the playaer is allowed to hit or if he hit something interactable with the mouse
                bool canHit = CheckIfPlayerMayHit();

                // if player is not allowed to hit the stop the code from running further
                if (!canHit) {
                    anim.SetTrigger("PickUp");
                    return;
                }

                // set the startPosition of the hit animation
                var startPos = meleeStartPoint.position;

                // create a direction vector from Hit position of the mouse and the projectiles original position
                direction = new Vector2(mousePosition.x - startPos.x, mousePosition.y - startPos.y);

                // normalize the direction which gives it a magnitude of 1
                direction.Normalize();

                // initialize two floats and populate them with the CheckDistancesToMouse Method call
                float distanceFromFrontToMouse, distanceFromBackToMouse;
                CheckDistancesToMouse(out distanceFromFrontToMouse, out distanceFromBackToMouse);

                // if the player is trying to shoot backwards for whatever reason dont do it
                if (distanceFromBackToMouse < distanceFromFrontToMouse)
                    return;

                // if the mouse is in the upper 2 quadrants
                if (dir == CoordinateDirection.NE || dir == CoordinateDirection.NW)
                {
                    // the animate hitting up
                    anim.SetTrigger("HitMeleeUp");
                }

                // if the mouse is in one of the lower two quadrants
                else if (dir == CoordinateDirection.SE || dir == CoordinateDirection.SW)
                {
                    // animate hitting down
                    anim.SetTrigger("HitMeleeDown");
                }

            }
            if (Input.GetMouseButton(1))
            {
                anim.SetBool("Block", true);
                if (!hasAddedModifer)
                {
                    modifierToRemove = (int)playerStat.armor.GetValue();
                    playerStat.armor.AddModifier((int)playerStat.armor.GetValue());
                    hasAddedModifer = true;
                }
            }
            if (!Input.GetMouseButton(1))
            {
                anim.SetBool("Block", false);
                if (hasAddedModifer)
                {
                    playerStat.armor.RemoveModifier(modifierToRemove);
                    hasAddedModifer = false;
                }
            }
        }


        // if the player is in a ranged state
        else if (ranged)
        {
            // if the player hits the left mouse button
            if (Input.GetMouseButton(0))
            {
                // check if the playaer is allowed to hit or if he hit something interactable with the mouse
                bool canHit = CheckIfPlayerMayHit();

                // if player is not allowed to hit the stop the code from running further
                if (!canHit)
                {
                    anim.SetTrigger("PickUp");
                    return;
                }

                // if player is allowed to hit then animate the shooting
                anim.SetTrigger("ShootRanged");
            }
        }

    } // commented

    private bool CheckIfPlayerMayHit()
    {
        // check if player hit something with the mouse when clicking that interacable
        Interactable inter = ClickFeedback();

        // if the player did hit something interacable with the mouse then do not allow the player to hit
        if (inter != null)
            return false;

        // if the player does not have weapons in his hands
        if (EquipmentManager.instance.currentEquipment[3] == null)
            return false;

        // otherwise let him hit
        return true;
    } // commented

    private void HandleMovement(float horizontal, float vertical)
    {
        if (interruptMovement)
            return;

        move = move.normalized * Time.deltaTime * speed;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            rigid.AddRelativeForce(move);
        }


        //// Sheathe weaponary
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    SheatheWeaponary();
        //}
        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    UnSheatheWeaponary();
        //}

    }

    private void HandleCombatState()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            //CombatTextManager.instance.FetchText(transform.position, gameObject.name);
            PooledProjectilesController.instance.impDaggers.Clear();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("calling loadscene");
            SceneManager.LoadSceneAsync(0);
            transform.position = new Vector2(-12f, 4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var tryMainHand = EquipFirstMatchingItemInBag(0, 3);
            EquipFirstMatchingItemInBag(2, 4);
            if (tryMainHand)
            {
                melee = true;
                ranged = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var tryEquip = EquipFirstMatchingItemInBag(1, 3);
            if (tryEquip)
            {
                melee = false;
                ranged = true;
            }

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            dialogue = !dialogue;
        }

        //if (ranged)
        //{
        //    stateText.text = "Ranged!";
        //}
        //else if (melee)
        //{
        //    stateText.text = "Melee!";
        //}

        // jump forward
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var trail = GetSystem("thrustTrail");
            trail.Play();
            anim.SetTrigger("Thrust");

            Vector3 direction = move;
            direction.Normalize();
            SpawnGhostImage(direction);

            // if you're standing still move towards mnouse instead
            if (move == Vector3.zero)
            {
                Vector3 directionMouse = new Vector3(mousePosition.x - transform.position.x, mousePosition.y - transform.position.y, 0f);
                directionMouse.Normalize();
                rigid.AddRelativeForce(directionMouse * thrustSpeed);
                return;
            }

            // add a relative local force to the player rigidbody in the given direction.
            rigid.AddRelativeForce(direction * thrustSpeed);
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

    private static bool EquipFirstMatchingItemInBag(int type, int slot)
    {
        // for all items in your bag
        for (int i = 0; i < Inventory.instance.itemsInBag.Count; i++)
        {
            // check to see if you have equipment in your bag
            if (Inventory.instance.itemsInBag[i] is Equipment)
            {
                // set tmp as the equipment found
                Equipment tmp = (Equipment)Inventory.instance.itemsInBag[i];

                // is tmp a Matching item?
                if ((int)tmp.equipSlot == slot)
                {
                    // equip the first Matching item you find
                    if ((int)tmp.equipType == type)
                    {
                        Inventory.instance.itemsInBag[i].Use();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void CheckIfFacingCorrectDirection(float horizontal)
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
        //Gather obects not to tbe flipped//
        ////////////////////////////////////

        Transform tmp = selector.transform;
        CanvasGroup healthImage = healthGroup;

        Vector3 pos = tmp.position;
        Vector3 healthPos = healthImage.gameObject.transform.position;

        tmp.SetParent(null);
        healthImage.transform.SetParent(null);



        ////////////////////////////////////
        //////////Flip the objects//////////
        ////////////////////////////////////
        Vector3 theScale = transform.localScale;

        theScale.x *= -1;

        facingRight = !facingRight;

        transform.localScale = theScale;

        tmp.SetParent(transform);
        tmp.position = pos;

        healthImage.transform.SetParent(transform);
        healthImage.transform.position = healthPos;
    }

    private Interactable ClickFeedback()
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
                return null;

            if (interactable != null)
            {
                SetFocus(interactable);
                SetMousePosition();
                return interactable;
            }
            else
            {
                Debug.LogWarning("something wrong with the clickFeedback");
                return null;
            }
        }
        else
        {
            RemoveFocus();
            SetMousePosition();
            return null;
        }


    }

    void SetFocus(Interactable newFocus)
    {
        if (newFocus != focus)
        {
            if (focus != null)
            {
                focus.OnDeFocused();
            }
            focus = newFocus;
        }
        newFocus.OnFocused(gameObject.transform);
    }

    void RemoveFocus()
    {
        if (focus != null)
        {
            focus.OnDeFocused();
        }
        focus = null;
    }

    public void KnockBack()
    {
        
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
       

    }

    public void OnStrikeComplete()
    {
        Debug.Log(equip.visibleGear.Length);

        if (equip.visibleGear[3].sprite == null)
            return;

        SoundManager.instance.PlayCombatSound("bladeSwing");

        // Determine the correct angle to turn to the projectile
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject projectile = pooledArrows.GetPooledSword();

        var projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.MakeProjectileReady();

        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.identity;
        projectile.transform.localScale = new Vector3(1, 1, 1);


        projectile.transform.Rotate(0, 0, angle, Space.World);
        // addforce force to the projectiles rigidbody in that direction.
        projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
        
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

    private void InitializePlayerGameObject()
    {
        // Set all necessary references
        rigid = gameObject.GetComponent<Rigidbody2D>();
        playerStat = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
        gameDetails = GameDetails.instance;
        equip = EquipmentManager.instance;
        m_Plane = GameDetails.instance.m_Plane;
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
    } // commented

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
    } // commented

    public void SetMouseQuadrant(CoordinateDirection newDir)
    {
        // set the mouseQuadrant to be the same as the paramter that has been passed in from the
        // HitDirectionDetection Script to let the player know in which of the 4 quadrants the mouse is.
        dir = newDir;
    } // commented

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // if you're not in an indoor zone
        if (!SceneManager.GetActiveScene().name.EndsWith("_indoor"))
        {
            // turn off your bodyLight
            bodyLight.SetActive(false);
        }

        // if you ARE in a indoor zone
        else if (SceneManager.GetActiveScene().name.EndsWith("_indoor"))
        {
            // turn off your bodyLight
            bodyLight.SetActive(true);

        }

        miniMap.SetActive(false);

    }

    private void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //private void SheatheWeaponary()
    //{
    //    Color c = new Color32(255, 255, 255, 0);
    //    equip.visibleGear[3].color = c;
    //    equip.visibleGear[4].color = c;

    //    weaponsGone = true;
    //}

    //private void UnSheatheWeaponary()
    //{
    //    Color c = new Color32(255, 255, 255, 255);
    //    equip.visibleGear[3].color = c;
    //    equip.visibleGear[4].color = c;

    //    weaponsGone = false;
    //}

    // checks the distance from the front of the player to the mouse and back of the player to the mouse and returns both values

}

