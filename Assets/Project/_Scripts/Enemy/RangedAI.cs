using EZCameraShake;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RangedAI : EnemyAI {

    [Header("Caster Variables")]
    [SerializeField] GameObject myProjectile;
    [SerializeField] float castingRange;
    [SerializeField] float projectileSpeed = 0;

    [Tooltip("The value to which the timer shall be reset after casting")]
    [SerializeField] float castingDelay;
    float originalCastDelay; /// Used to reset the castDelay back to its original number when enemy is moving
    [SerializeField] float castingTimer;


    Transform projectileLaunchPoint;
    bool inCastingRange;

    bool stoppedMoving;

    protected override void Start () {

        base.Start();

        projectileLaunchPoint = child.transform.Find("ProjectileLaunchPoint");

        originalCastDelay = castingDelay;
    }

    protected override void Update () {

        base.Update();

        CheckCastingRange();

        if (!moving)
        {
            if (!stoppedMoving)
            {
                castingTimer = 0;
                stoppedMoving = true;
            }

            castingDelay = originalCastDelay / 4;
        }
        else if (moving)
        {
            stoppedMoving = false;
            castingDelay = originalCastDelay;
        }

        castingTimer -= Time.deltaTime;

        if (haveAggro)
        {
            if (inCastingRange && castingTimer < 0)
            {
                anim.SetTrigger("Shoot");
                castingTimer = castingDelay;
                timer = meleeDelay;
            }
        }
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

            if (projectileScript.particles != null)
            {
                projectileScript.particles.GetComponent<ParticleEffectOutlineCreator>().Go();
            }

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

    public override void DieBurning()
    {
        GameObject tmp = ParticleSystemHolder.instance.PlaySpellEffect(transform.Find("Imp_A*/Skeleton/Body").position, "imp die burning");
        Destroy(tmp, 3);
    }

    public override void altDeath()
    {
        SoundManager.instance.PlayCombatSound("bomb");
        ParticleSystemHolder.instance.PlaySpellEffect(tr.position, "explode");
        CameraShaker.Instance.ShakeOnce(3f, 3f, 0.1f, 0.5f);
    }

}
