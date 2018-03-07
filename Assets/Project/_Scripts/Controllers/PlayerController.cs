﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class PlayerController : Interactable
{
    // plane needed for hitray during projectiles
    // located on Gamemanager since monsters also need access to it.
    Plane m_Plane;
    public Collider2D[] enemies;
    bool enemiesInRange;

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

    public CoordinateDirection dir;

    GameObject skeleton;
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
    //Camera cam;
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
    public bool ranged, dialogue;   // is the player using ranged or in a dialogue?

    EquipmentManager equip;
    bool weaponsGone = false;       // has the player unequipped his/her weapons

    //blockState
    bool hasAddedModifer;           // To make sure armor modifer only get added once when block is clicked
    int modifierToRemove;           // actual amount of extra damage that is absorbed when blocking
    #endregion

    void Start()
    {
        Initialize();
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
        if (isDead || gameDetails.paused || dialogue)
            return;

        CheckIfFacingCorrectDirection(horizontal);
        HandleMovement(horizontal, vertical);

    }

    private void LateUpdate()
    {
        prevPosition = transform.position;
    }

    private void HandleAggro()
    {
        int layerId = 11;
        int layerMask = 1 << layerId;

        enemies = Physics2D.OverlapCircleAll(transform.position, 20, layerMask);

        if (!enemiesInRange && enemies.Length > 0)
        {
            StartCoroutine(LineOfSight());
            enemiesInRange = true;
        }
    }

    private void HandleAnimation(float horizontal, float vertical)
    {
        if (eventSys.IsPointerOverGameObject())
            return;

        anim.SetFloat("VelocityX", Mathf.Abs(horizontal));
        anim.SetFloat("VelocityY", Mathf.Abs(vertical));

        if (melee)
        {
            if (Input.GetMouseButtonDown(0))
            {
                bool canHit = CheckIfPlayerMayHit();

                if (!canHit)
                    return;

                var startPos = meleeStartPoint.position;

                // create a direction vector from Hit position of the mouse and the projectiles original position
                direction = new Vector2(mousePosition.x - startPos.x, mousePosition.y - startPos.y);
                direction.Normalize();

                float distanceFromFrontToMouse, distanceFromBackToMouse;
                CheckDistancesToMouse(out distanceFromFrontToMouse, out distanceFromBackToMouse);

                if (distanceFromBackToMouse < distanceFromFrontToMouse)
                    return;

                if (dir == CoordinateDirection.NE || dir == CoordinateDirection.NW)
                {
                    anim.SetTrigger("HitMeleeUp");
                }
                else if (dir == CoordinateDirection.SE || dir == CoordinateDirection.SW)
                {
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
        else if (ranged)
        {
            if (Input.GetMouseButton(0))
            {
                Interactable inter = ClickFeedback();
                if (inter != null)
                    return;

                anim.SetLayerWeight(1, 1);

                if (weaponsGone)
                    return;
                anim.SetTrigger("ShootRanged");
            }
        }

    }

    private bool CheckIfPlayerMayHit()
    {
        Interactable inter = ClickFeedback();
        if (inter != null)
            return false;

        //anim.SetLayerWeight(1, 1);
        if (weaponsGone)
            return false;


        return true;
    }

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

        // spawn a copy of player current skeleton with all gear etc
        Vector2 spawnPoint = new Vector2(transform.position.x, transform.position.y + 0.452f);
        var image = Instantiate(skeleton, spawnPoint, Quaternion.identity);
        var col = image.GetComponent<CircleCollider2D>();
        col.enabled = true;

        // if player is not facing right flip the image
        if (!facingRight)
        {
            image.transform.localScale = new Vector2(image.transform.localScale.x * -1, image.transform.localScale.y);
        }

        var imageRigid = image.GetComponent<Rigidbody2D>();
        imageRigid.bodyType = RigidbodyType2D.Dynamic;
        imageRigid.AddForce(direction * thrustSpeed / 1.5f);
        image.AddComponent<Fade>();
    }

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
            //||
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
                Debug.Log(interactable.name + " hit it!");
                SetFocus(interactable);
                SetMousePosition();
                return interactable;
            }
            else
            {
                Debug.Log("something wrong with the clickFeedback");
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

        SoundManager.instance.PlaySound("bow");

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

        if (equip.visibleGear[3].sprite == null)
            return;

        SoundManager.instance.PlaySound("bladeSwing");

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
    private void CheckDistancesToMouse(out float distanceFromFrontToMouse, out float distanceFromBackToMouse)
    {
        mousePosition = SetMousePosition();
        distanceFromFrontToMouse = Vector3.Distance(projectilePoint.transform.position, mousePosition);
        distanceFromBackToMouse = Vector3.Distance(back.position, mousePosition);
    }

    //return the vector2 position of the mouse
    private Vector2 SetMousePosition()
    {
        //Create a ray from the Mouse click position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Initialise the enter variable
        float enter = 0.0f;

        if (m_Plane.Raycast(ray, out enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = ray.GetPoint(enter);
            mousePosition = hitPoint;
        }

        return mousePosition;
    }

    // initializes the Gameobject with all necessary references
    private void Initialize()
    {
        cameraControl = CameraController.instance;
        cameraControl.lookAt = this.gameObject;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        playerStat = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
        facingRight = true;


        particles = GetComponentsInChildren<ParticleSystem>();
        skeleton = transform.Find("Skeleton").gameObject;


        gameDetails = GameDetails.instance;
        equip = EquipmentManager.instance;
        m_Plane = GameDetails.instance.m_Plane;
        pooledArrows = PooledProjectilesController.instance;

        if (cameraControl.lookAt != this.gameObject)
        {
            cameraControl.lookAt = this.gameObject;
        }
    }

    private ParticleSystem GetSystem(string systemName)
    {
        foreach (ParticleSystem system in particles)
        {
            if (system.name == systemName)
            {
                return system;
            }
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10);
    }

    IEnumerator LineOfSight()
    {
        while (enemies.Length > 0)
        {
            Debug.Log("calling Coroutine");

            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyAI script = enemies[i].transform.parent.GetComponent<EnemyAI>();
                if (script != null)
                {
                    Debug.Log(enemies[i].transform.GetInstanceID());
                    script.DetermineAggro(transform.position);
                }

            }
            yield return new WaitForSeconds(0.5f);


        }

        enemiesInRange = false;
    }

    public void SetMouseQuadrant(CoordinateDirection newDir)
    {
        dir = newDir;
    }
}

