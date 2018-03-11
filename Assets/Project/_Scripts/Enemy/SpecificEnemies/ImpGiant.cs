using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ImpGiant : EnemyAI {

    [Header("Giant Specific variables")]
    public GameObject novaCannon;
    public GameObject FireBalls;
    public float novaRotationSpeed; // speed at which fireball launcher rotates around boss. this relates directly to how many fireballs are launched
    public float fireDelay;

    protected override void Start()
    {
        setter = GetComponent<AIDestinationSetter>();
        playerObj = PlayerController.instance.gameObject.transform;
    }

    protected override void Update()
    {
        timer -= Time.deltaTime;

        novaCannon.transform.Rotate(0, 0, novaRotationSpeed);

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

        if (timer < 0 && haveAggro)
        {
            Debug.Log("summon");
            anim.SetTrigger("Summon");

            timer = fireDelay;
        }


    }

    public override void CastFireCircle()
    {
        GameObject i = Instantiate(FireBalls, null);
        i.gameObject.transform.position = playerObj.transform.position;
    }

    public override void DetermineAggro(Vector3 pos)
    {
        if (inRange || isDead || pausingMovement)
            return;

        Debug.Log("calling DetermineAggro");

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

        //if (hit.transform != null)
        //{
        //    Debug.Log(hit.transform.name);
        //}

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
}
