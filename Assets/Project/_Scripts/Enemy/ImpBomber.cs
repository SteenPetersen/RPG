using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ImpBomber : EnemyAI {

    [Header("Bomber Unique Variables")]
    public GameObject explosion;

    public bool hasExploded;
    public Transform bombPos;

    protected override void Start()
    {

        maxSpeed = UnityEngine.Random.Range(maxSpeed - 1, maxSpeed + 1);
        distanceToLook = UnityEngine.Random.Range(distanceToLook - 2, distanceToLook + 2);
        meleeDelay = UnityEngine.Random.Range(meleeDelay - 0.9f, meleeDelay + 0.9f);

        setter = GetComponent<AIDestinationSetter>();
        playerObj = PlayerController.instance.gameObject.transform;

    }

    protected override void Update()
    {

        timer -= Time.deltaTime;

        DisplayHealth();


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
            }
        }

    }

    public override void OnExplode()
    {
        if (!hasExploded)
        {
            Instantiate(explosion, bombPos.position, Quaternion.identity);
            bombPos.gameObject.SetActive(false);
            PlayerController.instance.enemies.Remove(gameObject);
            healthGroup.gameObject.SetActive(false);
            myCollider.enabled = false;
            Destroy(gameObject, 3f);
            hasExploded = true;
        }

    }
}
