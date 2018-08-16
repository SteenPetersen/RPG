using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats {

    Animator anim;
    EnemyAI enemyAI;
    public bool hitByProjectile;

    ExperienceManager playerExp;

    [SerializeField] bool noLoot;

    [HideInInspector] public bool shielded;

    public List<GameObject> specialLoot = new List<GameObject>();

    private void Start()
    {
        enemyAI = gameObject.GetComponent<EnemyAI>();
        anim = enemyAI.child.GetComponent<Animator>();
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

        if (DungeonManager.instance != null)
        {
            DungeonManager dungeon = DungeonManager.instance;

            // If this enemy was died in a dungeon
            if (dungeon.enemiesInDungeon.Contains(gameObject))
            {
                if (!dungeon.bossKeyHasDropped && DungeonManager.instance.bossRoomAvailable)
                {
                    // check to see if it dropped the key
                    LootController.instance.EnemyBossRoomKeyDrop(dungeon.enemiesInDungeon.Count, gameObject.transform.position);
                }

                //  then remove him from the dungeon list
                dungeon.enemiesInDungeon.Remove(gameObject);
            }
        }

        // Add to statistics
        GameDetails.enemiesKilled++;


        // Give Player experience
        playerExp.AddExp(enemyAI.experienceGain, enemyAI.tier);
        // TODO something fancy with combattext or something

        // Add death animation
        int rand = UnityEngine.Random.Range(0,2);

        if (rand == 0)
        {
            anim.SetTrigger("Dead");
        }
        else if (rand == 1)
        {
            anim.SetTrigger("Dead1");
            enemyAI.altDeath();
        }
        enemyAI.isDead = true;

        // Loot logic
        if (!noLoot)
        {
            LootController.instance.EnemyLoot(enemyAI.tier, transform.position);
        }

        Destroy(gameObject, 5f);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        int newDmg = 0;
        bool crit = false;

        // if this enemy is not shielded from damage by for example a magic shield or special phase
        if (!shielded)
        {
            DamageVariance(damage, out crit, out newDmg);

            if (crit)
            {
                Quaternion rot = new Quaternion(0, 0, 0, 0);
                var system = ParticleSystemHolder.instance.CritWord();
                var go = Instantiate(system, transform.position, rot, enemyAI.transform);
                go.transform.localPosition = Vector3.zero;
                SoundManager.instance.PlayCombatSound("crit");
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

            enemyAI.healthbar.value = CalculateHealth(currentHealth, maxHealth);

            // hit sound
            SoundManager.instance.PlayCombatSound(gameObject.name + "_hit");
            SoundManager.instance.PlayCombatSound("impact_hit");

            return;
        }

        var shieldedText = CombatTextManager.instance.FetchText(transform.position);
        var shieldedTextScript = shieldedText.GetComponent<CombatText>();
        shieldedTextScript.Purple(newDmg.ToString(), transform.position);
        shieldedText.transform.position = transform.position;
        shieldedText.gameObject.SetActive(true);
    }

    public float CalculateHealth(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth;
    }
}
