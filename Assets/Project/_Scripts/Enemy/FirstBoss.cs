﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class FirstBoss : EnemyAI
{
    [Header("Caster Variables")]
    [SerializeField] GameObject myProjectile;
    [SerializeField] GameObject aoeProjectile = null;
    [SerializeField] float castingRange;
    [SerializeField] float projectileSpeed = 0;
    [SerializeField] bool bossAggro;
    [SerializeField] Vector2 castingPosition;

    [Tooltip("The value to which the timer shall be reset after casting")]
    [SerializeField] float castingDelay;
    [SerializeField] float castingTimer;

    [Tooltip("The value to which the timer shall be reset after casting")]
    [SerializeField] float aoeDelay;
    [SerializeField] float aoeTimer;

    Transform projectileLaunchPoint;
    public bool inCastingRange, aoeCasting;

    [Tooltip("How many fireballs does this boss Aoe?")]
    [SerializeField] int amountOfFireballs;

    [Tooltip("How much time before next aoe fireball?")]
    [SerializeField] float timeBetweenAoe;

    [Tooltip("How much time before next aoe fireball?")]
    [SerializeField] ParticleSystem FireShield;

    [Tooltip("The Boxes the boss will toss around")]
    [SerializeField] GameObject box;

    [Tooltip("How Many boxes shall he toss?")]
    [SerializeField] int amountOfBoxes;

    [Tooltip("Radius within which the box will land")]
    [SerializeField] float tossingRadius;

    [SerializeField] Transform portalPosition;

    bool deadState;

    protected override void Start()
    {
        base.Start();

        projectileLaunchPoint = child.transform.Find("ProjectileLaunchPoint");

        if (GameObject.Find("BossFollowLight") != null)
        {
            GameObject.Find("BossFollowLight").GetComponent<FollowLight>().Target = this.transform;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isDead && !deadState)
        {
            CameraShaker.Instance.ShakeOnce(6f, 6f, 0.2f, 15);
            SoundManager.instance.PlayCombatSound("first_boss_death");
            StopAllCoroutines();
            deadState = true;
            SpeechBubbleManager.instance.DisactivateBubble(speechBubbleId);
            SpeechBubbleManager.instance.FetchBubble(MySpeechLocation, this,  "AAARRGGHHHHH!");
            ParticleSystemHolder.instance.SpawnPort(portalPosition.position, "boss_portal");
            FireShield.Stop();
        }

        CheckCastingRange();

        castingTimer -= Time.deltaTime;
        aoeTimer -= Time.deltaTime;

        if (haveAggro)
        {
            if (inCastingRange && castingTimer < 0)
            {
                anim.SetTrigger("Shoot");
                castingTimer = castingDelay;
                timer = meleeDelay;
            }

            if (aoeTimer < 0 && !aoeCasting)
            {
                pausingMovement = true;
                aoeCasting = true;

                anim.SetTrigger("aoe");

                if (DebugControl.debugEnemies)
                {
                    Debug.Log("aoe trigger");
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

    public override void DetermineAggro(Vector3 pos)
    {
        if (inRange || isDead || pausingMovement)
            return;

        if (!bossAggro)
        {
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
                        setter.targetASTAR = playerObj;
                        bossAggro = true;
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
                            //Debug.Log("Player is still in range");
                            return;
                        }
                    }
                    haveAggro = false;
                    setter.ai.destination = setter.targetASTAR.transform.position;
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

    public override void OnEnemyCastComplete()
    {
        if (setter.targetASTAR != null)
        {
            Vector2 direction = new Vector2(setter.targetASTAR.transform.position.x - transform.position.x, setter.targetASTAR.transform.position.y - transform.position.y);
            direction.Normalize();

            FourDirectionalShot(direction, 1);
        }

    }

    public override void OnAoeCastComplete()
    {
        int rand = UnityEngine.Random.Range(0, 4);

        if (rand == 0)
        {
            StartCoroutine(FourDirectionalAoe());
        }
        else if (rand == 1)
        {
            StartCoroutine(RoundShot());
        }
        else if (rand == 2)
        {
            StartCoroutine(SpreadShotAoe());
        }
        else
        {
            StartCoroutine(PulseShotAoe());
        }

        castingTimer = castingDelay;
        aoeTimer = aoeDelay;
    }

    public override void StartParticleOne()
    {
        SpeechBubbleManager.instance.FetchBubble(MySpeechLocation, this, "HERE COMES FIRE!");

        gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        SoundManager.instance.PlayCombatSound(gameObject.name + "_prep");
        setter.targetASTAR = null;
        distanceToLook = 0;

        StartCoroutine(StartFireCircle());

    }

    public override void Flip()
    {
        if (aoeCasting)
        {
            return;
        }
        base.Flip();
    }

    /// <summary>
    /// Boss scatters a certain amount of Boxes randomly around himself
    /// </summary>
    void SpreadBoxes()
    {
        // determine position of boss
        Vector2 bossPosition = tr.position;

        for (int i = 0; i < amountOfBoxes; i++)
        {
            // determine positions bombs will stop
            Vector2 pos = bossPosition + UnityEngine.Random.insideUnitCircle * tossingRadius;
            Vector3 endPos = new Vector3(pos.x, pos.y, -0.25f);

            // spawn bombs at boss position
            GameObject tmp = Instantiate(box, new Vector3(bossPosition.x, bossPosition.y, -0.25f), Quaternion.identity);

            // Identify the script on the bomb and set the destination
            tmp.GetComponent<LerpToPosition>().SetStartAndDestination(new Vector3(bossPosition.x, bossPosition.y, -0.25f), endPos, tmp);
        }


        // play bomb sounds
        // SoundManager.instance.PlayUiSound("lootdrop");
    }

    IEnumerator StartFireCircle()
    {
        yield return new WaitForSeconds(0.5f);

        FireShield.Play();
        SpreadBoxes();
    }

    IEnumerator ReAggroPlayer()
    {
        yield return new WaitForSeconds(1);
        gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
        distanceToLook = 8;
        setter.targetASTAR = playerObj;
        aoeCasting = false;
        pausingMovement = false;
    }

    IEnumerator FourDirectionalAoe()
    {
        for (int i = 0; i <= amountOfFireballs; i++)
        {
            // at the player
            Vector2 direction = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
            direction.Normalize();

            FourDirectionalShot(direction, 4);

            yield return new WaitForSeconds(timeBetweenAoe);
        }

        StartCoroutine(ReAggroPlayer());
    }

    IEnumerator RoundShot()
    {
        Vector2 direction = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
        direction.Normalize();

        for (int i = 0; i <= amountOfFireballs * 4; i++)
        {
            if (i != 0)
            {
                direction = direction.Rotate(15f);
            }

            GameObject projectile = PooledProjectilesController.instance.GetEnemyProjectile(gameObject.name, aoeProjectile);

            projectile.GetComponent<enemy_Projectile>().MakeProjectileReady();
            projectile.transform.position = projectileLaunchPoint.transform.position;
            projectile.transform.rotation = Quaternion.identity;
            projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
            SoundManager.instance.PlayCombatSound(gameObject.name + "_shot");

            yield return new WaitForSeconds(timeBetweenAoe / 4);
        }

        StartCoroutine(ReAggroPlayer());
    }

    IEnumerator SpreadShotAoe()
    {
        for (int i = 0; i <= amountOfFireballs; i++)
        {
            // at the player
            Vector2 direction = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
            direction.Normalize();

            SpreadShot(direction);

            yield return new WaitForSeconds(timeBetweenAoe);
        }

        StartCoroutine(ReAggroPlayer());
    }

    IEnumerator PulseShotAoe()
    {
        for (int i = 1; i <= 5; i++)
        {
            // at the player
            Vector2 direction = new Vector2(playerObj.transform.position.x - transform.position.x, playerObj.transform.position.y - transform.position.y);
            direction.Normalize();

            PulseShot(direction);

            yield return new WaitForSeconds(timeBetweenAoe * 2);
        }

        StartCoroutine(ReAggroPlayer());
    }

    private void PulseShot(Vector2 direction)
    {
        for (int i = 0; i <= 18; i++)
        {
            if (i != 0)
            {
                direction = direction.Rotate(20);
            }

            GameObject projectile = PooledProjectilesController.instance.GetEnemyProjectile(gameObject.name, aoeProjectile);

            projectile.GetComponent<enemy_Projectile>().MakeProjectileReady();
            projectile.transform.position = projectileLaunchPoint.transform.position;
            projectile.transform.rotation = Quaternion.identity;
            projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed / 4);
            SoundManager.instance.PlayCombatSound(gameObject.name + "_shot");
        }
    }

    private void SpreadShot(Vector2 direction)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i == 1)
            {
                direction = direction.Rotate(25f);
            }
            else if (i == 2)
            {
                direction = direction.Rotate(315f);
            }

            GameObject projectile = PooledProjectilesController.instance.GetEnemyProjectile(gameObject.name, aoeProjectile);

            projectile.GetComponent<enemy_Projectile>().MakeProjectileReady();
            projectile.transform.position = projectileLaunchPoint.transform.position;
            projectile.transform.rotation = Quaternion.identity;
            projectile.GetComponent<Rigidbody2D>().AddForce(direction * projectileSpeed);
            SoundManager.instance.PlayCombatSound(gameObject.name + "_shot");
        }
    }

    /// <summary>
    /// Fires Projectiles at its initial direction and then at the same time 
    /// fires shots at perpendicual directions to the original shot.
    /// </summary>
    /// <param name="initialDirection">Initial direction of the projectile fed in as a paramter</param>
    /// <param name="amountOfDirection">How many shots to fire (Anything above 4 is meaningless)</param>
    void FourDirectionalShot(Vector3 initialDirection, int amountOfDirection)
    {
        for (int i = 1; i <= amountOfDirection; i++)
        {
            if (i != 1)
            {
                initialDirection = Vector2.Perpendicular(initialDirection);
            }

            GameObject projectile = PooledProjectilesController.instance.GetEnemyProjectile(gameObject.name, aoeProjectile);

            projectile.GetComponent<enemy_Projectile>().MakeProjectileReady();
            projectile.transform.position = projectileLaunchPoint.transform.position;
            projectile.transform.rotation = Quaternion.identity;
            projectile.GetComponent<Rigidbody2D>().AddForce(initialDirection * projectileSpeed);
            SoundManager.instance.PlayCombatSound(gameObject.name + "_shot");
        }

    }

}