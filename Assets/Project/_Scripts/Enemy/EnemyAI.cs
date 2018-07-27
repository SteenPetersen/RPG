using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyAI : AIPath
{

    #region Health
    [HideInInspector] public CanvasGroup healthGroup;
    [HideInInspector] public Slider healthbar;

    #endregion

    [HideInInspector] public AIDestinationSetter setter;

    [HideInInspector] public bool isDead, inRange, haveAggro, pausingMovement, displayingHealth, alert, inMeleeRange;
    [HideInInspector] public bool facingRight = true;
    [HideInInspector] public Transform playerObj;

    [Tooltip("Tier of this enemy, used to determine the loot it drops")]
    public int tier;
    
    [HideInInspector] public bool moving;

    /// <summary>
    /// Needs to be accessed in order to call animation from outside
    /// </summary>
    [HideInInspector] public Animator anim;

    /// <summary>
    /// Must be fed in as the name of this object can vary from monster to monster
    /// Used to check if collison detected is self or not
    /// </summary>
    public GameObject child;


    /// <summary>
    /// Some monsters require access to this collider
    /// </summary>
    [HideInInspector] public Collider2D myCollider;

    [HideInInspector] public EnemyStats myStats;

    [HideInInspector] public Transform projectileLaunchPoint;

    [Tooltip("The sprite holders that will become populated when this enemy is hit by a projectile")]
    public GameObject[] woundGraphics;

    [Header("Unique Variables")]
    public float distanceToLook; 
    public float meleeRange, force, projectileSpeed, experienceGain, meleeDelay, timer, meleeHitRange;

    [HideInInspector] public Transform skeleton;

    protected override void Start()
    {
        base.Start();

        InitializeEnemy();

        int percentageToAdd = ExperienceManager.instance.level * 20;

        myStats.maxHealth = myStats.maxHealth + percentageToAdd;
        myStats.currentHealth = myStats.maxHealth;
        maxSpeed = UnityEngine.Random.Range(maxSpeed - 1, maxSpeed + 1);
        distanceToLook = UnityEngine.Random.Range(distanceToLook - 2, distanceToLook + 2);
        meleeDelay = UnityEngine.Random.Range(meleeDelay - 0.9f, meleeDelay + 0.9f);
        

        setter = GetComponent<AIDestinationSetter>();
        playerObj = PlayerController.instance.gameObject.transform;
    }

    protected override void Update()
    {
        DisplayHealth();

        if (pausingMovement)
        {
            if (isDead)
            {
                anim.SetBool("Stunned", false);
            }
            return;
        }

        timer -= Time.deltaTime;

        if (PlayerController.instance.isDead)
            return;


        if (isDead)
        {
            setter.ai.canMove = false;
            GetComponent<Rigidbody2D>().isKinematic = true;
            setter.ai.destination = tr.position;
            setter.targetASTAR = null;
            setter.enabled = false;


            CheckIfEnemyIsOnAList();

            return;
        }

        base.Update();

        CheckMeleeRange();

        if (Mathf.Approximately(velocity.x, 0) && Mathf.Approximately(velocity.y, 0) && moving)
        {
            //Debug.Log("stopping");
            anim.SetBool("Walk", false);
            moving = false;
        }
        else if (velocity.x > 0.03 || velocity.y > 0.03 && !moving)
        {
           // Debug.Log("moving");
            anim.SetBool("Walk", true);
            moving = true;
        }

        if (haveAggro)
        {
            FaceTarget();

            if (inMeleeRange && timer < 0)
            {
                anim.SetTrigger("Hit1");
                timer = meleeDelay;
            }
        }

    }

    public virtual void CheckIfEnemyIsOnAList()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "bossroom1")
        {
            EnemyHolder.instance.enemies.Remove(gameObject);
        }
    }

    public virtual void DetermineAggro(Vector3 pos)
    {
        if (inRange || isDead || pausingMovement)
            return;

        if (Vector3.Distance(playerObj.transform.position, gameObject.transform.position) < distanceToLook)
        {
            //Debug.Log("calling DetermineAggro");

            //create layer masks for the player and the obstacles ending a finalmask combining both
            int playerLayer = 10;
            int obstacleLayer = 13;
            var playerlayerMask = 1 << playerLayer;
            var obstacleLayerMask = 1 << obstacleLayer;
            var finalMask = playerlayerMask | obstacleLayerMask;

            // shoot a ray from the enemy in the direction of the player, the distance of the enemy from the player on the layer masks that we created above
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (pos - transform.position), distanceToLook, finalMask);

            var drawdirection = (pos - transform.position).normalized;
            Debug.DrawRay(transform.position, (pos - transform.position), Color.cyan);
            Debug.DrawLine(transform.position, (drawdirection * distanceToLook) + transform.position, Color.black, 1f);

            // if the ray hits the player then aggro him
            if (hit.transform != null)
            {
                if (hit.transform.name == "Player")
                {
                    if (!haveAggro)
                    {
                        //Debug.Log("I see you!");
                        Debug.Log(setter);
                        Debug.Log(setter.targetASTAR);
                        Debug.Log(PlayerController.instance.transform);

                        setter.targetASTAR = PlayerController.instance.transform;
                        haveAggro = true;
                    }
                }
                else if (haveAggro && hit.transform.name != "Player")
                {
                    haveAggro = false;
                    setter.targetASTAR = null;
                    //Debug.LogWarning("Lost Line of sight to the player");
                    return;
                }
            }
            else if (hit.transform == null)
            {
                haveAggro = false;
                setter.targetASTAR = null;
                //Debug.LogWarning("Ran out of my distance");
                return;
            }
        }


    }

    /// <summary>
    /// used to make enemies attack at a large distance
    /// </summary>
    public virtual void HyperAlert()
    {
        if (!alert)
        {
            distanceToLook = distanceToLook * 6;
            alert = true;
        }
    }

    /// <summary>
    /// used to guarantee that the enemy will always face the target
    /// </summary>
    public virtual void FaceTarget()
    {
        float distanceFromObjectToTarget = Vector3.Distance(CameraController.instance.measurementTransform.position, playerObj.position);
        float distanceFromObjectToMe = Vector3.Distance(CameraController.instance.measurementTransform.position, transform.position);

        if (distanceFromObjectToTarget > distanceFromObjectToMe && !facingRight || distanceFromObjectToTarget < distanceFromObjectToMe && facingRight)
        {
            Flip();
        }
    }

    /// <summary>
    /// Flips the enemy
    /// </summary>
    public virtual void Flip()
    {
        if (isDead)
            return;

        Transform tmp = healthGroup.transform;

        Vector3 pos = tmp.position;

        tmp.SetParent(null);

        Vector3 theScale = child.gameObject.transform.localScale;

        theScale.x *= -1;

        facingRight = !facingRight;

        child.gameObject.transform.localScale = theScale;

        tmp.SetParent(child.transform);

        if (facingRight)
        {
            tmp.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (!facingRight)
        {
            tmp.transform.localScale = new Vector3(-1, 1, 1);
        }

        tmp.position = pos;
    }

    public virtual void CheckMeleeRange()
    {
        //create layer masks for the player and the obstacles ending a finalmask combining both
        int playerLayer = 10;
        var playerlayerMask = 1 << playerLayer;

        if (setter.targetASTAR != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (playerObj.position - transform.position), meleeRange, playerlayerMask);
            var drawdirection = (playerObj.position - transform.position).normalized;

            if (hit.collider != null)
            {
                //Debug.Log(hit.collider.name);

                if (hit.collider.name == "Player")
                {
                    Debug.DrawRay(transform.position, (playerObj.position - transform.position), Color.green);
                   // Debug.Log("reach target, in melee range");
                    inMeleeRange = true;
                    return;
                }
                else
                {
                    inMeleeRange = false;
                }
            }

            inMeleeRange = false;
        }
    }

    public virtual void OnStrikeComplete()
    {
        if (!PlayerController.instance.isDead)
        {
            if (!pausingMovement)
            {
                SoundManager.instance.PlayCombatSound(gameObject.name + "_swing");

                //create layer masks for the player
                int playerLayer = 10;
                var playerlayerMask = 1 << playerLayer;

                RaycastHit2D hit = Physics2D.Raycast(transform.position, playerObj.transform.position - transform.position, meleeHitRange, playerlayerMask);

                if (hit.transform != null)
                {
                    if (hit.collider.name == "Player")
                    {
                        Debug.Log("Hit the player");
                        if (hit.collider.transform.root.GetComponent<PlayerStats>() != null)
                        {
                            var statScript = hit.collider.transform.root.GetComponent<PlayerStats>();
                            statScript.TakeDamage(myStats.damage.GetValue());
                        }
                    }
                }
            }
        }
    }

    public float CalculateHealth(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth;
    }

    public virtual void DisplayHealth()
    {
        if (myStats.currentHealth < myStats.maxHealth && myStats.currentHealth > 0 && !displayingHealth)
        {
            healthGroup.alpha = 1;
            displayingHealth = true;
        }
        else if (displayingHealth && myStats.currentHealth <= 0)
        {
            healthGroup.alpha = 0;
            displayingHealth = false;
        }
    }

    public virtual void Knockback(Vector3 dir)
    {
        int obstacleLayer = 13;
        var obstacleLayerMask = 1 << obstacleLayer;

        // shoot a ray from the enemy in the direction of the player, the distance of the enemy from the player on the layer masks that we created above
        RaycastHit2D hit = Physics2D.Raycast(tr.position, dir, 1.2f, obstacleLayerMask);

        RaycastHit2D hit1 = Physics2D.Raycast(tr.position, Vector2.up, 1.2f, obstacleLayerMask);
        RaycastHit2D hit2 = Physics2D.Raycast(tr.position, Vector2.down, 1.2f, obstacleLayerMask);
        RaycastHit2D hit3 = Physics2D.Raycast(tr.position, Vector2.right, 1.2f, obstacleLayerMask);
        RaycastHit2D hit4 = Physics2D.Raycast(tr.position, Vector2.left, 1.2f, obstacleLayerMask);

        Debug.DrawRay(transform.position, dir, Color.cyan, 1);
        Debug.DrawRay(transform.position, Vector2.up, Color.cyan, 1);
        Debug.DrawRay(transform.position, Vector2.down, Color.cyan, 1);
        Debug.DrawRay(transform.position, Vector2.right, Color.cyan, 1);
        Debug.DrawRay(transform.position, Vector2.left, Color.cyan, 1);


        if (hit.collider == null && hit1.collider == null && hit2.collider == null && hit3.collider == null && hit4.collider == null)
        {
            GetComponent<Rigidbody2D>().AddForce(dir * force * 10);
        }
    }

    public virtual void OnEnemyCastComplete()
    {
        // meant to be overriden
    }

    public virtual void CastFireCircle()
    {
        // meant to be overriden
    }

    public virtual void OnExplode()
    {
        // meant to be overriden
    }

    public virtual void AggroFromDistance(int newLookDistance)
    {
        distanceToLook = newLookDistance;
    }

    public virtual void ProjectileWoundGraphics(ArrowType arrowType)
    {
        Sprite woundArrow = null;

        for (int i = 0; i < ProjectileList.instance.arrowHitGraphics.Count; i++)
        {
            if (ProjectileList.instance.arrowHitGraphics[i].name == arrowType + "_hit")
            {
                woundArrow = ProjectileList.instance.arrowHitGraphics[i];
            }
        }

        foreach (var wound in woundGraphics)
        {
            if (wound.GetComponent<SpriteRenderer>().sprite == null)
            {
                wound.GetComponent<SpriteRenderer>().sprite = woundArrow;
                return;
            }
        }
    }

    /// <summary>
    /// Sets all necessary references so that the editor can be cleaner
    /// </summary>
    public virtual void InitializeEnemy()
    {
        myCollider = child.GetComponent<BoxCollider2D>();
        myStats = GetComponent<EnemyStats>();
        anim = child.GetComponent<Animator>();
        healthGroup = child.transform.Find("HealthCanvas").GetComponent<CanvasGroup>();
        healthbar = child.transform.Find("HealthCanvas").transform.Find("Slider").GetComponent<Slider>();
        skeleton = child.transform.Find("Skeleton");
    }

    /// <summary>
    /// Temporarily stuns the enemy
    /// </summary>
    public virtual void Stunned()
    {
        // stop the enemy from moving temporarily
        pausingMovement = true;
        canMove = false;

        // set the animation of stunned
        anim.SetBool("Stunned", true);


        // play the particle effect of being stunned
        Quaternion rot = new Quaternion(0, 0, 0, 0);
        var system = ParticleSystemHolder.instance.PlayerStunnedEffect();
        var go = Instantiate(system, tr.position, rot, skeleton);
        go.transform.localPosition = Vector3.zero;


        // unstun the monster after a given time
        StartCoroutine(UnStun(go));
    }

    /// <summary>
    /// unstuns the enemy after a predetermined amount of time
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public virtual IEnumerator UnStun(GameObject go)
    {
        yield return new WaitForSeconds(2f);

        anim.SetBool("Stunned", false);
        pausingMovement = false;
        canMove = true;
        Destroy(go);
    }
}
