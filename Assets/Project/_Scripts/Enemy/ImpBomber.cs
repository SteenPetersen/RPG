using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using EZCameraShake;
using UnityEngine.SceneManagement;

public class ImpBomber : EnemyAI {

    [Header("Bomber Unique Variables")]
    public GameObject explosion;

    public bool hasExploded;
    public Transform bombPos;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        timer -= Time.deltaTime;
        base.Update();
    }

    public override void OnExplode()
    {
        if (!hasExploded)
        {
            Instantiate(explosion, bombPos.position, Quaternion.identity);
            bombPos.gameObject.SetActive(false);
            PlayerController.instance.enemies.Remove(gameObject);
            healthbar.gameObject.SetActive(false);
            myCollider.enabled = false;
            Destroy(gameObject, 3f);
            hasExploded = true;

            if (SceneManager.GetActiveScene().name.EndsWith("_indoor"))
            {
                DungeonManager dungeon = DungeonManager.instance;

                dungeon.enemiesInDungeon.Remove(gameObject);

                if (dungeon.enemiesInDungeon.Count == 0 && !dungeon.bossKeyHasDropped && dungeon.bossRoomAvailable)
                {
                    Instantiate(DungeonManager.instance.bossRoomKey, tr.position, Quaternion.identity);
                }

                Debug.LogWarning(" there are now " + DungeonManager.instance.enemiesInDungeon.Count + " enemies on the list");
            }

            CameraShaker.Instance.ShakeOnce(3f, 3f, 0.1f, 0.5f);
            SoundManager.instance.PlayCombatSound("bomb");

            foreach (var wound in woundGraphics)
            {
                if (wound.GetComponent<SpriteRenderer>().sprite != null)
                {
                    wound.GetComponent<SpriteRenderer>().sprite = null;
                }
            }
        }

    }
}
