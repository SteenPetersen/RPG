using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats {

    public Animator anim;
    public EnemyAI enemyAI;
    GameDetails gameDetails;
    public bool hitByProjectile;

    public List<GameObject> specialLoot = new List<GameObject>();

    private void Start()
    {
        enemyAI = gameObject.GetComponent<EnemyAI>();
        //anim = GetComponent<Animator>();
        gameDetails = GameDetails.instance;
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

        // remove all the projectiles if there are any
        var projectiles = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name == "projectile(Clone)")
            {
                projectiles.Add(child);
            }
        }
        projectiles.ForEach(child => Destroy(child.gameObject));

        // Add death animation
        anim.SetTrigger("Dead");
        enemyAI.isDead = true;

        // Loot logic
        int randomIndex = Random.Range(0, gameDetails.generalObjects.Length);
        int roll = Random.Range(0, 100);
        if (roll < gameDetails.generalObjects[randomIndex].gameObject.GetComponent<ItemPickup>().chanceToDrop)
        {
            Instantiate(gameDetails.generalObjects[randomIndex], transform.position, Quaternion.identity);
            // play loot sound
        }

        // Special loot
        if (specialLoot.Count != 0)
        {
            for (int i = 0; i < specialLoot.Count; i++)
            {
                float dropChance = Random.Range(0, 100);
                Debug.Log(dropChance + " in special loot");
                if (dropChance < gameDetails.generalObjects[i].GetComponent<ItemPickup>().chanceToDrop)
                {
                    Instantiate(specialLoot[i], transform.position, Quaternion.identity);
                    // play loot sound
                    break;
                }
            }
        }

        Destroy(gameObject, 5f);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // hit sound
        SoundManager.instance.PlaySound(gameObject.name + "_hit");
        SoundManager.instance.PlaySound("impact_hit");
        int newDmg = 0;
        bool crit = false;

        DamageVariance(damage, out crit, out newDmg);

        if (crit)
        {
            //Debug.LogWarning("CRIT!");
            int bonusDmg = Random.Range(0, newDmg / 2);
            newDmg += bonusDmg;
        }

        currentHealth -= newDmg;

        if (currentHealth <= 0)
        {
            SoundManager.instance.PlaySound(gameObject.name + "_death");
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
            SoundManager.instance.PlaySound(gameObject.name);
            //Debug.Log(gameObject.name);
        }
        enemyAI.healthbar.value = enemyAI.CalculateHealth(currentHealth, maxHealth);
    }


    //public override bool Heal(int healthIncrease)
    //{
    //    bool tmp = base.Heal(healthIncrease);

    //    enemyAI.healthbar.value = enemyAI.CalculateHealth(currentHealth, maxHealth);

    //    return tmp;
    //}
}
