using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ImpGiant : EnemyAI {

    /// <summary>
    /// Phase in which the boss is during the fight
    /// </summary>
    [SerializeField] int phase;

    /// <summary>
    /// Radius in which the bombs ultimate resting position can be placed
    /// </summary>
    [SerializeField] float bombRadius;

    public GameObject bossBomb;

    [Header("Giant Specific variables")]
    public float novaRotationSpeed; // speed at which fireball launcher rotates around boss. this relates directly to how many fireballs are launched
    public float fireDelay;

    [HideInInspector] public bool dialogueFinished;  // so boss doesnt start fighting before dialogue is finished

    /// <summary>
    /// Needs to be accessed by the boom from the fireNova in order to alter them
    /// </summary>
    public bool magicShieldUp, stunned;

    /// <summary>
    /// Needs to be accessed by the boom from the fireNova in order to disactivate it
    /// </summary>
    public ParticleSystem magicShield;

    [SerializeField] GameObject FireCircle;

    protected override void Start()
    {
        InitializeEnemy();
        setter = GetComponent<AIDestinationSetter>();
        playerObj = PlayerController.instance.gameObject.transform;
    }

    protected override void Update()
    {
        if (dialogueFinished)
        {
            if (!magicShieldUp && !pausingMovement)
            {
                magicShield.Play();
                myStats.shielded = true;
                magicShieldUp = true;
            }

            timer -= Time.deltaTime;

            if (isDead)
            {
                EnemyHolder.instance.bossIsAlive = false;
                return;
            }

            base.Update();

            if (timer < 0 && haveAggro)
            {
                SpreadBombs(3);

                Debug.Log("summon");
                anim.SetTrigger("Summon");

                timer = fireDelay;
            }

        }
    }

    /// <summary>
    /// Disables Boss monsters magic shield
    /// </summary>
    public void DisableMagicShield()
    {
        magicShield.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        myStats.shielded = false;
        magicShieldUp = false;
    }

    /// <summary>
    /// Casts the heatseeking fire circle
    /// </summary>
    public override void CastFireCircle()
    {
        GameObject i = Instantiate(FireCircle, null);
        i.gameObject.transform.position = playerObj.transform.position;
    }

    /// <summary>
    /// Boss scatters a certain amount of bombs randomly around himself
    /// </summary>
    void SpreadBombs(int amountOfBombs)
    {
        // determine position of boss
        Vector2 bossPosition = transform.position;

        for (int i = 0; i < amountOfBombs; i++)
        {
            // determine positions bombs will stop
            Vector2 pos = bossPosition + UnityEngine.Random.insideUnitCircle * bombRadius;

            // spawn bombs at boss position
            GameObject tmp = Instantiate(bossBomb, bossPosition, Quaternion.identity);

            // Identify the script on the bomb
            LerpToPosition script = tmp.GetComponent<LerpToPosition>();

            // send bombs from boss position to stop position
            script.SetDestination(pos);
        }
        

        // play bomb sounds
        // SoundManager.instance.PlayUiSound("lootdrop");
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
