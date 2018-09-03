using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpNormal : EnemyAI {

    [Header("Dasher Variables")]
    [SerializeField] float castingRange;
    [SerializeField] float projectileSpeed = 0;

    [Tooltip("The value to which the timer shall be reset after casting")]
    [SerializeField] float castingDelay;
    float originalCastDelay; /// Used to reset the castDelay back to its original number when enemy is moving
    [SerializeField] float castingTimer;


    bool inCastingRange;

    bool stoppedMoving;
    bool playerApproriateLevel;
    float startSpeed;
    float maxDashSpeed;
    bool dashing;
    [SerializeField] ParticleSystem dashEffect;

    protected override void Start()
    {
        base.Start();

        originalCastDelay = castingDelay;

        startSpeed = maxSpeed;
        maxDashSpeed = startSpeed * 2f;

        if (ExperienceManager.MyLevel >= 3)
        {
            playerApproriateLevel = true;
        }
    }

    protected override void Update()
    {
        base.Update();

        CheckCastingRange();

        if (haveAggro)
        {

            castingTimer -= Time.deltaTime;


            if (inCastingRange && castingTimer < 0)
            {
                if (playerApproriateLevel && !dashing)
                {
                    StartCoroutine(Dash());
                    timer = meleeDelay;
                    dashing = true;
                }
            }
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

    /// <summary>
    /// Dashes towards its target at an incresing speed
    /// </summary>
    /// <returns></returns>
    IEnumerator Dash()
    {
        repathRate = 0.1f;
        slowdownDistance = 2;
        anim.SetBool("Dash", true);
        dashEffect.Play();

        while (maxSpeed < maxDashSpeed)
        {
            maxSpeed += 0.5f;
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.2f);

        maxSpeed = startSpeed;
        slowdownDistance = 6;
        repathRate = 0.3f;
        anim.SetBool("Dash", false);
        dashEffect.Stop();

        castingTimer = castingDelay;
        dashing = false;

    }

}
