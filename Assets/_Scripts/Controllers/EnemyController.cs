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
    }

    private void Update()
    {

        Follow();
        DisplayHealth();

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
        {
            return;
        }

        if (isDead)
        {
            nav.SetDestination(transform.position);
            foreach (GameObject arrow in arrows)
            {
                arrow.transform.SetParent(null);
            }
            logic.SetActive(false);
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
