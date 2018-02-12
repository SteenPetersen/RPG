using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlayerController : Interactable
{
    // plane needed for hitray during projectiles
    // located on Gamemanager since monsters also need access to it.
    Plane m_Plane;

    #region Singleton

    public static PlayerController instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    public Vector3 clickPosition;
    public Vector2 mousePosition;

    public bool isDead;
    public Interactable focus;

    Rigidbody2D rigid;
    public float speed;

    CameraController cameraControl;
    PlayerStats playerStat;
    PlayerManager manager;
    GameObject cameraHolder;
    Camera cam;
    public EventSystem eventSys;

    Animator anim;

    [HideInInspector]
    public bool onLand, facingRight = true;

    #region Logic inside player Gameobject
    public Transform back;
    #endregion

    #region projectiles
    PooledProjectilesController pooledArrows;
    public GameObject projectilePoint;
    public float projectileSpeed;
    #endregion

    #region State/etc
    bool changedWaterState;

    public bool melee = true;
    public bool ranged;
    public bool dialogue;

    EquipmentManager equip;
    bool weaponsGone = false;
    [HideInInspector]
    public GameObject shadow;

    //blockState
    bool hasAddedModifer;
    int modifierToRemove;
    #endregion

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        //eventSys = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        cameraControl = CameraController.instance;
        cameraControl.lookAt = this.gameObject;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        playerStat = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
        facingRight = true;
        shadow = this.gameObject.transform.GetChild(0).GetChild(2).gameObject;

        manager = PlayerManager.instance;
        equip = EquipmentManager.instance;
        m_Plane = PlayerManager.instance.m_Plane;
        pooledArrows = PooledProjectilesController.instance;
    }

    private void Update()
    {
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        if (cameraControl.lookAt != this.gameObject)
        {
            cameraControl.lookAt = this.gameObject;
        }

        Flip(horizontal);
        HandleStates();
        HandleMovement(horizontal, vertical);
        HandleAnimation(horizontal, vertical);
    }

    private void HandleAnimation(float horizontal, float vertical)
    {
        if (isDead || dialogue || manager.paused || eventSys.IsPointerOverGameObject())
            return;

        anim.SetFloat("VelocityX", Mathf.Abs(horizontal));
        anim.SetFloat("VelocityY", Mathf.Abs(vertical));

        if (melee)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Interactable inter = ClickFeedback();
                if (inter != null)
                    return;

                anim.SetLayerWeight(1, 1);
                if (weaponsGone)
                    return;
                anim.SetTrigger("HitMelee");
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
            if (Input.GetMouseButtonDown(0))
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

        // TODO not necessary to have 2 bools
        if (!onLand && !changedWaterState)
        {
            Color c = new Color32(43,77,121,50);
            equip.equipmentSlots[2].color = c;
            equip.equipmentSlots[5].color = c;
            equip.equipmentSlots[6].color = c;
            shadow.SetActive(false);
            gameObject.transform.position = new Vector3((float)gameObject.transform.position.x, (float)(gameObject.transform.position.y - 0.225), 0f);
            changedWaterState = true;
        }
        if (onLand && changedWaterState)
        {
            Color c = new Color32(255, 255, 255, 255);
            equip.equipmentSlots[2].color = c;
            equip.equipmentSlots[5].color = c;
            equip.equipmentSlots[6].color = c;
            shadow.SetActive(true);
            gameObject.transform.position = new Vector3((float)gameObject.transform.position.x, (float)(gameObject.transform.position.y + 0.225), 0f);
            changedWaterState = false;
        }
    }

    private void HandleMovement(float horizontal, float vertical)
    {
        if (isDead || dialogue || manager.paused)
            return;

        if (Input.GetKey(KeyCode.D))
        {
            rigid.AddRelativeForce(Vector2.right * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rigid.AddRelativeForce(Vector2.left * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.W))
        {
            rigid.AddRelativeForce(Vector2.up * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rigid.AddRelativeForce(Vector2.down * speed * Time.deltaTime);
        }

        // Sheathe weaponary
        if (Input.GetKeyDown(KeyCode.P))
        {
            SheatheWeaponary();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            UnSheatheWeaponary();
        }

    }

    private void HandleStates()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            Debug.Log("1");
            melee = true;
            ranged = false;
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            Debug.Log("2");
            melee = false;
            ranged = true;
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            Debug.Log("3");
            dialogue = !dialogue;
        }
    }

    private void Flip(float horizontal)
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

    public void OnCastComplete()
    {
        float distanceFromFrontToMouse, distanceFromBackToMouse;
        CheckDistancesToMouse(out distanceFromFrontToMouse, out distanceFromBackToMouse);

        if (distanceFromBackToMouse > distanceFromFrontToMouse)
        {
            // create a direction vector from Hit position of the mouse and the projectiles original position
            Vector2 direction = new Vector2(clickPosition.x - projectilePoint.transform.position.x, clickPosition.y - projectilePoint.transform.position.y);
            direction.Normalize();

            // Determine the correct angle to turn to the projectile
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GameObject projectile = pooledArrows.GetPooledArrow();

            projectile.SetActive(true);
            projectile.GetComponent<CircleCollider2D>().enabled = true;
            transform.parent = null;
            projectile.transform.position = projectilePoint.transform.position;
            projectile.transform.rotation = Quaternion.identity;

            projectile.GetComponent<Rigidbody2D>().isKinematic = false;

            projectile.transform.Rotate(0, 0, angle, Space.Self);

            // addforce force to the projectiles rigidbody in that direction.
            projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);

        }
        else
        {
            Debug.Log("I cannot shoot Backwards!");
        }
    }

    private void SheatheWeaponary()
    {
        Color c = new Color32(255, 255, 255, 0);
        equip.equipmentSlots[3].color = c;
        equip.equipmentSlots[4].color = c;

        weaponsGone = true;
    }

    private void UnSheatheWeaponary()
    {
        Color c = new Color32(255, 255, 255, 255);
        equip.equipmentSlots[3].color = c;
        equip.equipmentSlots[4].color = c;

        weaponsGone = false;
    }

    // checks the distance from the front of the player to the mouse and back of the player to the mouse and returns both values
    private void CheckDistancesToMouse(out float distanceFromFrontToMouse, out float distanceFromBackToMouse)
    {
        mousePosition = SetMousePosition();
        distanceFromFrontToMouse = Vector3.Distance(projectilePoint.transform.position, clickPosition);
        distanceFromBackToMouse = Vector3.Distance(back.position, clickPosition);
        //Debug.Log(distanceFromFrontToMouse + "   " + distanceFromBackToMouse);
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

            clickPosition = hitPoint;
        }
        return clickPosition;
    }

}

