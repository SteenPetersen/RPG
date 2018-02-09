using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats {

    public Animator anim;
    public EnemyController enemyControl;
    PlayerManager playerManager;
    public bool hitByProjectile;

    public List<GameObject> specialLoot = new List<GameObject>();

    private void Start()
    {
        enemyControl = gameObject.GetComponent<EnemyController>();
        anim = GetComponent<Animator>();
        playerManager = PlayerManager.instance;
    }

    private void Update()
    {
        if (hitByProjectile)
        {
            enemyControl.currentLookRadius = enemyControl.pullRadiusAggro;
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
        enemyControl.speed = 0;
        enemyControl.isDead = true;

        // Loot logic
        for (int i = 0; i < playerManager.generalObjects.Count; i++)
        {
            float dropChance = Random.Range(0, 100);
            Debug.Log(dropChance);
            if (dropChance < playerManager.generalObjects[i].GetComponent<ItemPickup>().chanceToDrop)
            {
                Instantiate(playerManager.generalObjects[i], transform.position, Quaternion.identity);
                // play loot sound
            }
        }

        // Special loot
        if (specialLoot.Count != 0)
        {
            for (int i = 0; i < specialLoot.Count; i++)
            {
                float dropChance = Random.Range(0, 100);
                Debug.Log(dropChance + " in special loot");
                if (dropChance < playerManager.generalObjects[i].GetComponent<ItemPickup>().chanceToDrop)
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

        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt");
            // TODO sound of damage
        }
        enemyControl.healthbar.value = enemyControl.CalculateHealth(currentHealth, maxHealth);
    }
}
