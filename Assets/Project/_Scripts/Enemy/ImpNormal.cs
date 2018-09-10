using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpNormal : EnemyAI {


    [Tooltip("The value to which the timer shall be reset after casting")]
    [SerializeField] float castingDelay;
    [SerializeField] float castingTimer;

    bool stoppedMoving;
    bool playerApproriateLevel;
    float startSpeed;
    float maxDashSpeed;
    bool dashing;
    [SerializeField] ParticleSystem dashEffect;

    protected override void Start()
    {
        base.Start();

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

        if (canMove)
        {
            if (haveAggro)
            {
                castingTimer -= Time.deltaTime;

                if (castingTimer < 0)
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

        whenCloseToDestination = Pathfinding.CloseToDestinationMode.ContinueToExactDestination;

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

        whenCloseToDestination = Pathfinding.CloseToDestinationMode.Stop;
        castingTimer = castingDelay;
        dashing = false;

    }

}
