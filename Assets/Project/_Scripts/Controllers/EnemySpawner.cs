using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;

public class EnemySpawner : MonoBehaviour {

    public GameObject[] enemy;
    public float timer;
    public float spawnDelay;
    public Transform enemyHolder;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            SpawnEnemy();
            timer = spawnDelay;
        }
    }

    private void SpawnEnemy()
    {
        if (EnemyHolder.instance.enemies.Count < EnemyHolder.instance.maxAmountOfEnemies)
        {
            Debug.Log("spawning enemy");
            int rand = UnityEngine.Random.Range(0, enemy.Length);
            Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
            var spawnedEnemy = Instantiate(enemy[rand], pos, Quaternion.identity, enemyHolder);
            EnemyHolder.instance.enemies.Add(spawnedEnemy);
            var script = spawnedEnemy.gameObject.GetComponent<EnemyAI>();
            script.AggroFromDistance(100);
        }
    }
}
