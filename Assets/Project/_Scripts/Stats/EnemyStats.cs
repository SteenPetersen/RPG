using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats {

    public Animator anim;
    public EnemyAI enemyAI;
    public bool hitByProjectile;

    ExperienceManager playerExp;

    public List<GameObject> specialLoot = new List<GameObject>();

    private void Start()
    {
        enemyAI = gameObject.GetComponent<EnemyAI>();
        //anim = GetComponent<Animator>();
        playerExp = ExperienceManager.instance;
    }

    private void Update()
    {
        if (hitByProjectile)
        {
            enemyAI.haveAggro = true;
            hitByProjectile = false;
        }
    }

    public override void Die()
    {
        base.Die();

        // If this enemy was on the players enemies list
        if (PlayerController.instance.enemies.Contains(gameObject))
        {
            //  then remove him from it
            PlayerController.instance.enemies.Remove(gameObject);
        }


        if (EnemyHolder.instance != null)
        {
            // If this enemy was on the EnemyHolderBoss event list
            if (EnemyHolder.instance.enemies.Contains(gameObject))
            {
                //  then remove him from it
                EnemyHolder.instance.enemies.Remove(gameObject);
            }
        }

        // Add to statistics
        GameDetails.enemiesKilled++;


        // Give Player experience
        playerExp.AddExp(enemyAI.experienceGain);
        // TODO something fancy with combattext or something

        // Add death animation
        anim.SetTrigger("Dead");
        enemyAI.isDead = true;

        // Loot logic

        LootController.instance.EnemyLoot(enemyAI.tier, transform.position);

        Destroy(gameObject, 5f);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // hit sound
        SoundManager.instance.PlayCombatSound(gameObject.name + "_hit");
        SoundManager.instance.PlayCombatSound("impact_hit");
        int newDmg = 0;
        bool crit = false;

        DamageVariance(damage, out crit, out newDmg);

        if (crit)
        {
            Quaternion rot = new Quaternion(0, 0, 0, 0);
            var system = ParticleSystemHolder.instance.CritWord();
            var go = Instantiate(system, transform.position, rot, enemyAI.skeleton);
            go.transform.localPosition = Vector3.zero;

            int bonusDmg = Random.Range(0, newDmg / 2);
            newDmg += bonusDmg;
        }

        currentHealth -= newDmg;

        if (currentHealth <= 0)
        {
            SoundManager.instance.PlayCombatSound(gameObject.name + "_death");
            Die();
        }



        var text = CombatTextManager.instance.FetchText(transform.position);
        var textScript = text.GetComponent<CombatText>();
        if (crit) { textScript.Crit(newDmg.ToString(), transform.position); }
        else { textScript.White(newDmg.ToString(), transform.position); }
        text.transform.position = transform.position;
        text.gameObject.SetActive(true);

        if (currentHealth > 0)
        {

            anim.SetTrigger("Hurt");
            SoundManager.instance.PlayCombatSound(gameObject.name);
        }
        enemyAI.healthbar.value = enemyAI.CalculateHealth(currentHealth, maxHealth);
    }
}
