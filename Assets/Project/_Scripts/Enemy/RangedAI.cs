﻿using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RangedAI : EnemyAI {

    [Header("Caster Variables")]
    public GameObject myProjectile;
    public float castingRange;
    bool inCastingRange;
    public float castingDelay;
    public float castingTimer;

	protected override void Start () {

        base.Start();

        maxSpeed = UnityEngine.Random.Range(maxSpeed - 1, maxSpeed + 1);
        distanceToLook = UnityEngine.Random.Range(distanceToLook - 2, distanceToLook + 2);
        meleeDelay = UnityEngine.Random.Range(meleeDelay - 0.9f, meleeDelay + 0.9f);
        castingDelay = UnityEngine.Random.Range(meleeDelay - 0.9f, meleeDelay + 0.9f);

        projectileLaunchPoint = child.transform.Find("ProjectileLaunchPoint");
    }

    protected override void Update () {

        DisplayHealth();

        if (pausingMovement)
        {
            if (isDead)
            {
                anim.SetBool("Stunned", false);
            }
            timer = meleeDelay;
            return;
        }

        timer -= Time.deltaTime;
        castingTimer -= Time.deltaTime;


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
        CheckCastingRange();

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
                anim.SetTrigger("hit1");
                timer = meleeDelay;
                castingTimer = castingDelay;
            }

            else if (inCastingRange && timer < 0)
            {
                anim.SetTrigger("Shoot");
                castingTimer = castingDelay;
                timer = meleeDelay;
            }
        }

    }

    public override void DetermineAggro(Vector3 pos)
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

        RaycastHit2D[] hits = Physics2D.RaycastAll(tr.position, (pos - transform.position), distanceToLook, finalMask);

        var drawdirection = (pos - transform.position).normalized;
        Debug.DrawRay(transform.position, (pos - transform.position), Color.cyan);
        Debug.DrawLine(transform.position, (drawdirection * distanceToLook) + transform.position, Color.black, 1f);

        // if the ray hits something
        if (hit.transform != null)
        {
            // is it the player?
            if (hit.transform.name == "Player")
            {
                // if you've hit the player but don't have aggro
                if (!haveAggro)
                {
                    //Debug.Log("I see you!");
                    setter.targetASTAR = playerObj;
                    haveAggro = true;
                }
            }

            // if you have aggro but what you're first Line of sight object is NOT the player
            else if (haveAggro && hit.transform.name != "Player")
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].transform.name == "Player")
                    {
                        Debug.Log("Player is still in range");
                        return;
                    }
                }
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

    public override void CheckMeleeRange()
    {
        base.CheckMeleeRange();
    }

    public override void OnEnemyCastComplete()
    {
        if (setter.targetASTAR != null)
        {
            Vector2 direction = new Vector2(setter.targetASTAR.transform.position.x - transform.position.x, setter.targetASTAR.transform.position.y - transform.position.y);
            direction.Normalize();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GameObject projectile = PooledProjectilesController.instance.GetEnemyProjectile(gameObject.name, myProjectile);

            var projectileScript = projectile.GetComponent<enemy_Projectile>();
            projectileScript.MakeProjectileReady();

            projectile.transform.position = projectileLaunchPoint.transform.position;
            projectile.transform.rotation = Quaternion.identity;

            projectile.transform.Rotate(0, 0, angle, Space.World);

            projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
        }

    }

    public virtual void CheckCastingRange()
    {
        //create layer masks for the player and the obstacles ending a finalmask combining both
        int playerLayer = 10;
        int obstacleLayer = 13;
        var playerlayerMask = 1 << playerLayer;
        var obstacleLayerMask = 1 << obstacleLayer;
        var finalMask = playerlayerMask | obstacleLayerMask;

        if (setter.targetASTAR != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, (playerObj.position - transform.position), castingRange, finalMask);
            var drawdirection = (playerObj.position - transform.position).normalized;

            if (hit.collider != null)
            {
                //Debug.Log(hit.collider.name);

                if (hit.collider.name == "Player")
                {
                    Debug.DrawRay(transform.position, (playerObj.position - transform.position), Color.green);
                    // Debug.Log("reach target, in melee range");
                    inCastingRange = true;
                    return;
                }
                else
                {
                    inCastingRange = false;
                }
            }

            inCastingRange = false;
        }
    }

    public override void AggroFromDistance(int newLookDistance)
    {
        distanceToLook = newLookDistance;
        setter.ai.destination = PlayerController.instance.gameObject.transform.position;
    }

}
