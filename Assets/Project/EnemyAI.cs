using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class EnemyAI : AIPath
{

    public float force;
    #region Health
    [SerializeField]
    public CanvasGroup healthGroup;
    public Slider healthbar;

    #endregion

    public AIDestinationSetter setter;

    public float distanceToLook,  meleeRange;
    public bool isDead, inRange, haveAggro, pausingMovement, displayingHealth, alert, inMeleeRange;
    public bool facingRight = true;
    public Transform playerObj;
    public Animator anim;

    public bool moving;

    public GameObject child;  // used to check if collison detected is self or not
    public CircleCollider2D myCollider;
    public CharacterStats myStats;

    public GameObject strikeGraphic;
    public float projectileSpeed;
    public Transform effectPoint;

    public List<GameObject> arrows = new List<GameObject>();
    public Transform ArrowHolder;

    public float meleeDelay;
    public float timer;


    protected override void Start()
    {

        base.Start();

        maxSpeed = UnityEngine.Random.Range(maxSpeed - 1, maxSpeed + 1);
        distanceToLook = UnityEngine.Random.Range(distanceToLook - 2, distanceToLook + 2);
        meleeDelay = UnityEngine.Random.Range(meleeDelay - 0.9f, meleeDelay + 0.9f);

        setter = GetComponent<AIDestinationSetter>();
        playerObj = PlayerController.instance.gameObject.transform;
    }

    protected override void Update()
    {
        timer -= Time.deltaTime;

        DisplayHealth();


        if (isDead)
        {
            setter.ai.canMove = false;
            GetComponent<Rigidbody2D>().isKinematic = true;
            setter.ai.destination = tr.position;
            setter.targetASTAR = null;
            setter.enabled = false;
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

    public void WalkToShooterPosition(Vector2 pos)
    {
        setter.ai.destination = pos;
    }

    public virtual void DetermineAggro(Vector3 pos)
    {
        if (inRange || isDead || pausingMovement)
            return;

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
                    setter.targetASTAR = playerObj;
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

    public virtual void HyperAlert()
    {
        if (!alert)
        {
            distanceToLook = distanceToLook * 3;
            alert = true;
        }
    }

    public virtual void FaceTarget()
    {
        float distanceFromObjectToTarget = Vector3.Distance(CameraController.instance.measurementTransform.position, playerObj.position);
        float distanceFromObjectToMe = Vector3.Distance(CameraController.instance.measurementTransform.position, transform.position);

        if (distanceFromObjectToTarget > distanceFromObjectToMe && !facingRight || distanceFromObjectToTarget < distanceFromObjectToMe && facingRight)
        {
            Flip();
        }
    }

    public virtual void Flip()
    {
        if (isDead)
            return;

        // Transform tmp = logic.transform;

        //Vector3 pos = tmp.position;

        //tmp.SetParent(null);

        Vector3 theScale = child.gameObject.transform.localScale;

        theScale.x *= -1;

        facingRight = !facingRight;

        child.gameObject.transform.localScale = theScale;

        //tmp.SetParent(transform);
        //tmp.position = pos;
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

    //IEnumerator MeleeHitTheTarget()
    //{
    //    anim.SetTrigger("Hit1");
    //}

    public virtual void OnStrikeComplete()
    {
        if (!PlayerController.instance.isDead)
        {
            Vector2 direction = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
            direction.Normalize();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            SoundManager.instance.PlaySound(gameObject.name + "_swing");

            //GameObject projectile = pooledProjectiles.GetEnemyProjectile(gameObject.name);
            //var projectileScript = projectile.GetComponent<enemy_Projectile>();
            //projectileScript.MakeProjectileReady();

            GameObject strike = Instantiate(strikeGraphic, null, true);

            strike.transform.position = effectPoint.position;
            strike.transform.rotation = Quaternion.identity;

            strike.transform.Rotate(0, 0, angle, Space.World);

            strike.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
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

    public void Knockback(Vector3 dir)
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
}
