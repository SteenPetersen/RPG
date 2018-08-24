using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPortalOnDeath : MonoBehaviour {

    [SerializeField] GameObject Portal;
    [SerializeField] bool dead, portalSpawned;
    [SerializeField] EnemyAI myScript;
    [SerializeField] Vector2 portPosition;
    [SerializeField] CommandPlayer welcomeSign;

	void Start ()
    {
        myScript = GetComponent<EnemyAI>();
	}
	
	void Update ()
    {
        if (myScript.isDead && !portalSpawned)
        {
            //spawn portal
            Instantiate(Portal, portPosition, Quaternion.identity);
            portalSpawned = true;
            TutorialManager.instance.finalMonsterDead = true;
            StartCoroutine(welcomeSign.FadeIn());
        }
	}
}
