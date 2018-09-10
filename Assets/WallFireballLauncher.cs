using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallFireballLauncher : MonoBehaviour {

    [SerializeField] GameObject projectile;
    [SerializeField] float projectileSpeed;

    [SerializeField] Transform close;
    [SerializeField] Transform far;
    [SerializeField] Vector2 dir;
    [SerializeField] ParticleSystem shootEffect;

    [SerializeField] bool fired;
    [SerializeField] int amountOfShots;
    [SerializeField] float timeBetweenShots;

    [SerializeField] float distanceToWall;

    float resetTimer;
    [SerializeField] float resetTime;
    [SerializeField] bool gotTarget;

    void Start()
    {
        if (dir == Vector2.zero)
        {
            dir = (far.position - close.position).normalized;
        }

        SetRayLength();
    }



    void Update()
    {
        if (Time.frameCount % 5 == 0)
        {
            if (gotTarget == false)
            {
                CheckLOS();
            }
        }

        if (fired)
        {
            resetTimer -= Time.deltaTime;

            if(resetTimer < 0)
            {
                fired = false;
                gotTarget = false;
            }
        }
    }

    private void SetRayLength()
    {
        //create layer masks for the player
        int wallLayer = 13;
        var wallLayerMask = 1 << wallLayer;

        RaycastHit hit;

        if (Physics.Raycast(transform.parent.transform.position, dir, out hit, Mathf.Infinity, wallLayerMask))
        {
            distanceToWall = hit.distance;
            Debug.DrawLine(transform.position, dir.normalized * distanceToWall, Color.red, 2);
            Debug.Log(distanceToWall + " " + dir.normalized + " " + dir.normalized * distanceToWall);
        } 
    }

    private void CheckLOS()
    {
        //Debug.DrawLine(transform.parent.transform.position, dir.normalized * distanceToWall, Color.red, 2);

        Debug.DrawRay(transform.parent.transform.position, dir * distanceToWall, Color.cyan);

        //////create layer masks for the player
        int playerLayer = 10;
        var playerlayerMask = 1 << playerLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.parent.transform.position, dir, distanceToWall, playerlayerMask);

        if (hit.transform != null)
        {
            if (hit.collider.name == "Player")
            {
                StartCoroutine(Fire());
            }
        }
    }

    IEnumerator Fire()
    {
        gotTarget = true;

        while (!fired)
        {
            for (int i = 0; i < amountOfShots; i++)
            {
                Shoot();
                yield return new WaitForSeconds(timeBetweenShots);
            }

            fired = true;
            resetTimer = resetTime;
        }
    }

    void Shoot()
    {
        GameObject p = PooledProjectilesController.instance.GetEnemyProjectile(gameObject.name, projectile);

        var projectileScript = p.GetComponent<enemy_Projectile>();
        projectileScript.MakeProjectileReady();

        if (projectileScript.particles != null)
        {
            projectileScript.particles.GetComponent<ParticleEffectOutlineCreator>().Go();
        }

        p.transform.position = far.position;
        p.transform.rotation = Quaternion.identity;

        p.GetComponent<Rigidbody2D>().AddForce(dir * projectileSpeed);
        shootEffect.Play();

        SoundManager.instance.PlayEnvironmentSound("cannon_shoot");
    }
}
