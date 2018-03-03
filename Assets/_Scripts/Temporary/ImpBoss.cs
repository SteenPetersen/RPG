using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpBoss : EnemyController {

    // special move
    public bool ImpNova, castingNova, hasBeenReset;

    // timer countdown
    float impNovaTimer;

    // nova shooter
    public GameObject novaCannon;
    public ParticleSystem nova;

    public bool includeChildren = true;


    public float resetDelay = 10;
    public float resetTimer;
    public float originalNovaDelay;
    public float novaDelay = 8;
    public float novaTimer = 8;

    void Start()
    {
        combat = GetComponent<CharacterCombat>();
        anim = GetComponent<Animator>();
        nav = GetComponent<PolyNavAgent>();
        playercontrol = PlayerController.instance;
        player = playercontrol.gameObject.transform;
        target = player;
        logic = transform.Find("Logic").gameObject;

        pooledProjectiles = PooledProjectilesController.instance;

        originalNovaDelay = novaDelay;
        resetTimer = resetDelay;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        novaTimer -= Time.deltaTime;

        novaCannon.transform.Rotate(0, 0, 3);
        DisplayHealth();
        DetermineLookRadius();

        if (isDead)
        {
            nav.SetDestination(transform.position);
            return;
        }

        Attack();
        Chase();

        if (novaTimer <= 0)
        {
            CastImpNova();
            novaTimer = novaDelay;
        }

        if (haveAggro && !hasBeenReset)
        {
            resetTimer = resetDelay;
            hasBeenReset = true;
        }
        else if (!haveAggro && !hasBeenReset)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0)
            {
                novaDelay = originalNovaDelay;
                myStats.Heal((int)myStats.maxHealth);
                hasBeenReset = true;
            }
        }


    }

    public override void Attack()
    {
        base.Attack();
    }

    public void CastImpNova()
    {
        //Debug.Log("castNova");
        NovaLauncher();
    }

    void NovaLauncher()
    {
        if (!haveAggro)
            return;

        nova.Play();
        var em = nova.emission;
        em.enabled = true;
        float AmountToLaunch = 100 / ((healthbar.value) * 100) * 5;
        AmountToLaunch = Mathf.Clamp(AmountToLaunch, 5, 20);
       // Debug.Log(AmountToLaunch);
        nova.Emit((int)AmountToLaunch);
        novaDelay = (novaDelay / 1.1f);
        novaDelay = Mathf.Clamp(novaDelay, 3, originalNovaDelay);
    }

    public override void DetermineLookRadius()
    {
        if (haveAggro)
        {
            distanceToLook = 15;
        }

        else
        {
            distanceToLook = 10;
        }
    }

    public override void DetermineAggro(Vector3 pos)
    {
        if (ImpNova || isDead)
            return;

        base.DetermineAggro(pos);
    }

}
