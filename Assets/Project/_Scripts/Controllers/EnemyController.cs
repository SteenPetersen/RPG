using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


/* handles interactions with enemies */

public class EnemyController : Enemy {

    #region Aggro
    // raycast aggro system new stuff
    [HideInInspector]
    public bool haveAggro, inRange;
    public float distanceToLook;
    public float distanceToStop;
    [HideInInspector]
    public Vector3 lastKnownPlayerPosition;
    #endregion

    //ranged
    public Transform projectilePoint;
    public float rangedDelay;
    public float projectileSpeed;
    [HideInInspector]
    public float timer;

    public bool isDead = false;
    bool displayingHealth = false;

    [HideInInspector]
    public bool facingRight = true;
    [HideInInspector]
    public PolyNavAgent nav;
    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public CharacterCombat combat;
    [HideInInspector]
    public PooledProjectilesController pooledProjectiles;

    // a game object to hold items that we do not wish to flip
    [HideInInspector]
    public GameObject logic;
    [HideInInspector]
    public List<GameObject> arrows = new List<GameObject>();

    void Start() {
        combat = GetComponent<CharacterCombat>();
        anim = GetComponent<Animator>();
        nav = GetComponent<PolyNavAgent>();
        playercontrol = PlayerController.instance;
        player = playercontrol.gameObject.transform;
        target = player;
        logic = transform.Find("Logic").gameObject;

        pooledProjectiles = PooledProjectilesController.instance;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        DisplayHealth();
        DetermineLookRadius();

        if (isDead)
        {
            nav.SetDestination(transform.position);
            return;
        }

        Follow();


        if (haveAggro && !playercontrol.isDead)
        {
            Chase();
            DetermineAggro(target.position);
        }

        if (!haveAggro && (Vector2)transform.position == nav.nextPoint)
        {
            anim.SetBool("Walk", false);
        }
        else if (!haveAggro && (Vector2)transform.position != nav.nextPoint)
        {
            Debug.Log("trying to find a place to stop");
            nav.SetDestination(lastKnownPlayerPosition);
        }

        Attack();
    }

    public virtual void DisplayHealth()
    {
        if (myStats.currentHealth < myStats.maxHealth && myStats.currentHealth > 0 && !displayingHealth)
        {
            healthGroup.alpha = 1;
            displayingHealth = true;
        }
        else if(displayingHealth && myStats.currentHealth <= 0)
        {
            healthGroup.alpha = 0;
            displayingHealth = false;
        }
    }

    public override void Follow()
    {
        if (playercontrol.isDead)
            return;

        if (isDead)
        {
            nav.SetDestination(transform.position);
            logic.SetActive(false);

            for (int i = 0; i < arrows.Count; i++)
            {
                arrows[i].transform.SetParent(null);
                arrows[i].gameObject.SetActive(false);
                arrows.Remove(arrows[i]);
            }

            return;
        }
    }

    public virtual void Attack()
    {
        if (isDead || playercontrol.isDead)
            return;


        if (haveAggro && !inRange)
        {
            if (timer < 0)
            {
                anim.SetTrigger("Shoot");

                timer = rangedDelay;
            }
        }
    }

    public virtual void OnThrowCOmplete()
    {
        //Vector2 direction = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);
        //direction.Normalize();

        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //GameObject projectile = pooledProjectiles.GetEnemyProjectile(gameObject.name, );
        //var projectileScript = projectile.GetComponent<enemy_Projectile>();
        //projectileScript.MakeProjectileReady();

        //projectile.transform.position = projectilePoint.transform.position;
        //projectile.transform.rotation = Quaternion.identity;

        //projectile.transform.Rotate(0, 0, angle, Space.World);

        //projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
    }

    //public virtual void OnStrikeComplete()
    //{
    //    if (!playercontrol.isDead)
    //    {
    //        Vector2 direction = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);
    //        direction.Normalize();

    //        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    //        GameObject projectile = pooledProjectiles.GetEnemyProjectile(gameObject.name);
    //        var projectileScript = projectile.GetComponent<enemy_Projectile>();
    //        projectileScript.MakeProjectileReady();

    //        projectile.transform.position = transform.position;
    //        projectile.transform.rotation = Quaternion.identity;

    //        projectile.transform.Rotate(0, 0, angle, Space.World);

    //        projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
    //    }

    //}

    public virtual void FaceTarget()
    {
        float distanceFromObjectToTarget = Vector3.Distance(CameraController.instance.measurementTransform.position, target.position);
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

        Transform tmp = logic.transform;

        Vector3 pos = tmp.position;

        tmp.SetParent(null);

        Vector3 theScale = transform.localScale;

        theScale.x *= -1;

        facingRight = !facingRight;

        transform.localScale = theScale;

        tmp.SetParent(transform);
        tmp.position = pos;
    }

    public virtual void Chase()
    {
        if (isDead || !haveAggro)
            return;

        int playerLayer = 10;
        var playerlayerMask = 1 << playerLayer;


        if (!inRange && haveAggro)
        {
            anim.SetBool("Walk", true);
            nav.SetDestination(target.position);
            FaceTarget();
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (target.position - transform.position), distanceToStop, playerlayerMask);

        if (hit.transform != null)
        {
            if (hit.transform.name == "Player")
            {
                inRange = true;
                anim.SetBool("Walk", false);
                nav.SetDestination(transform.position);
                anim.SetTrigger("Hit1");
            }
        }

        inRange = false;

    }

    public virtual void DetermineLookRadius()
    {
        if (haveAggro)
        {
            distanceToLook = 10;
        }

        else
        {
            distanceToLook = 5;
        }
    }

    public virtual void DetermineAggro(Vector3 pos)
    {
        if (inRange || isDead)
            return;

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
                lastKnownPlayerPosition = target.position;
                if (!haveAggro)
                {
                    //Debug.Log("I see you!");
                    haveAggro = true;
                }
            }
            else if (haveAggro && hit.transform.name != "Player")
            {
                haveAggro = false;
                //Debug.LogWarning("Lost  Line of sight to the player");
            }
        }
        else if (hit.transform == null)
        {
            haveAggro = false;
            //Debug.LogWarning("Ran out of my distance");
        }

    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position,currentLookRadius);

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, radius);
    }

}
