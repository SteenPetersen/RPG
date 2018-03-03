using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyAI : AIPath {

    AIDestinationSetter setter;

    public float distanceToLook;
    public bool isDead, inRange, haveAggro, pausingMovement, enemiesInRange;
    public bool facingRight = true;
    Transform playerObj;
    Vector3 lastKnownPlayerPosition;
    public Animator anim;

    bool moving;

    public List<Transform> enemies = new List<Transform>();
    public GameObject child;
    public CircleCollider2D myCollider;


    protected override void Start () {

        base.Start();

        maxSpeed = UnityEngine.Random.Range(maxSpeed - 1, maxSpeed + 1);
        distanceToLook = UnityEngine.Random.Range(distanceToLook - 2, distanceToLook + 2);

        setter = GetComponent<AIDestinationSetter>();
        playerObj = PlayerController.instance.gameObject.transform;
	}
	
	protected override void Update () {

        base.Update();

        FaceTarget();

        if (Mathf.Approximately(velocity.x, 0) && Mathf.Approximately(velocity.y, 0) && moving)
        {
            Debug.Log("stopping");
            anim.SetBool("Walk", false);
            moving = false;
        }
        else if (velocity.x > 0.03 || velocity.y > 0.03 && !moving)
        {
            Debug.Log("moving");
            anim.SetBool("Walk", true);
            moving = true;
        }
    }


    IEnumerator ReengageAfterAShortStop(Vector2 pos)
    {
        Debug.Log("stopping up");
        float randX = UnityEngine.Random.Range(pos.x + 2, pos.x - 2);
        float randY = UnityEngine.Random.Range(pos.y + 2, pos.y - 2);
        float waitTime = UnityEngine.Random.Range(0.2f, 0.6f);

        setter.targetASTAR = null;


        setter.ai.destination = new Vector3(randX, randY, 0);
        pausingMovement = true;
        myCollider.enabled = false;
        yield return new WaitForSeconds(waitTime);

        setter.targetASTAR = playerObj;
        myCollider.enabled = true;
        pausingMovement = false;
        Debug.Log("continuing movement");


    }


    public virtual void DetermineAggro(Vector3 pos)
    {
        if (inRange || isDead || pausingMovement)
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
                lastKnownPlayerPosition = playerObj.position;
                if (!haveAggro)
                {
                    Debug.Log("I see you!");
                    setter.targetASTAR = playerObj;
                    haveAggro = true;
                }
            }
            else if (haveAggro && hit.transform.name != "Player")
            {
                haveAggro = false;
                setter.targetASTAR = null;
                Debug.LogWarning("Lost  Line of sight to the player");
            }
        }
        else if (hit.transform == null)
        {
            haveAggro = false;
            setter.targetASTAR = null;
            Debug.LogWarning("Ran out of my distance");
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

        Vector3 theScale = transform.localScale;

        theScale.x *= -1;

        facingRight = !facingRight;

        transform.localScale = theScale;

        //tmp.SetParent(transform);
        //tmp.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Enemy" && col.gameObject != child)
        {
            Debug.Log("enemy!");
            StartCoroutine(ReengageAfterAShortStop(col.transform.position));
        }
    }
}
