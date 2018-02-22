using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


/* handles interactions with enemies */

public class EnemyController : Enemy {

    #region Aggro
    public float lookRadiusNonAggro = 5f;
    public float lookRadiusAggro = 7.5f;
    public float pullRadiusAggro = 30f;
    public float currentLookRadius;
    bool haveAggro;
    #endregion

    //ranged
    public GameObject enemyProjectile;
    public float rangedDelay;
    public float projectileSpeed;
    float timer;



    public bool isDead = false;
    bool displayingHealth = false;
    public float speed;
    float velocity;

    [HideInInspector]
    public bool facingRight = true;
    PolyNavAgent nav;
    Transform target;

    [HideInInspector]
    public CharacterCombat combat;

    PooledProjectilesController pooledProjectiles;

    // a game object to hold items that we do not wish to flip
    GameObject logic;
    //[HideInInspector]
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

        Follow();
        DisplayHealth();
        RangedAttack();

        //check if focused and interact with
        //if (isFocus && !hasInteracted)
        //{
        //    float distance = Vector3.Distance(target.position, transform.position);

        //    if (distance <= radius)
        //    {
        //        Interact();
        //    }
        //    hasInteracted = true;
        //}
    }

    private void DisplayHealth()
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

            foreach (GameObject arrow in arrows)
            {
                arrow.transform.SetParent(null);
                arrow.gameObject.SetActive(false);
                arrows.Remove(arrow);
            }
            return;
        }


        float distance = Vector3.Distance(target.position, transform.position);
        if (distance <= currentLookRadius && distance > radius)
        {
            //nav.centerOffset = new Vector2(center.position.x, center.position.y);
            haveAggro = true;
            currentLookRadius = lookRadiusAggro;
            anim.SetBool("Walk", true);
            nav.SetDestination(target.position);
            FaceTarget();
        }

        else if (distance < radius)
        {
            //nav.centerOffset = Vector2.zero;
            anim.SetBool("Walk", false);
            nav.SetDestination(transform.position);
            CharacterStats targetStats = target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                // if you're close enough then go to CharacterCombat and perform Attack();
                combat.Attack(targetStats, anim);
            }
        }
        else
        {
            if (haveAggro)
            {
                haveAggro = false;
                // monster lost aggro
            }
            nav.SetDestination(nav.nextPoint);
            currentLookRadius = lookRadiusNonAggro;
            if ((Vector2)transform.position == nav.nextPoint)
            {
                anim.SetBool("Walk", false);
            }
        }
    }

    private void RangedAttack()
    {
        if (isDead || PlayerController.instance.isDead)
            return;

        float distance = Vector3.Distance(target.position, transform.position);
        if (haveAggro && distance > radius)
        {


            if (timer < 0)
            {
                //do something
                anim.SetTrigger("Shoot");

                timer = rangedDelay;
            }
        }

    }

    private void OnThrowCOmplete()
    {
        Vector2 direction = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);
        direction.Normalize();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject projectile = pooledProjectiles.GetEnemyProjectile(gameObject.name);
        var projectileScript = projectile.GetComponent<enemy_Projectile>();
        projectileScript.MakeProjectileReady();

        projectile.transform.position = transform.position;
        projectile.transform.rotation = Quaternion.identity;

        projectile.transform.Rotate(0, 0, angle, Space.World);

        projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
    }

    void FaceTarget()
    {
        float distanceFromObjectToTarget = Vector3.Distance(CameraController.instance.measurementTransform.position, target.position);
        float distanceFromObjectToMe = Vector3.Distance(CameraController.instance.measurementTransform.position, transform.position);

        if (distanceFromObjectToTarget > distanceFromObjectToMe && !facingRight || distanceFromObjectToTarget < distanceFromObjectToMe && facingRight)
        {
            Flip();
        }
    }

    void Flip()
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,currentLookRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
