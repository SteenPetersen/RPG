using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterPortal : MonoBehaviour {

    PlayerController player;
    bool routineRunning;

	void Start ()
    {
        player = PlayerController.instance;
	}
	

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player" && !routineRunning)
        {
            StartCoroutine(Portal());
        }
    }

    IEnumerator Portal()
    {
        routineRunning = true;
        ParticleSystemHolder.instance.PlaySpellEffect(PlayerController.instance.transform.position, transform.name);
        player.transform.Find("Skeleton").gameObject.SetActive(false);
        player.Portal = true;
        SoundManager.instance.PlayEnvironmentSound("enter_portal");
        BoardCreator.instance.followLight.SetActive(false);

        yield return new WaitForSeconds(1);

        SceneManager.LoadSceneAsync(0);
        PlayerController.instance.transform.position = new Vector2(-12f, 4);
        DungeonManager dungeon = DungeonManager.instance;
        dungeon.playerHasBossKey = false;
    }
}
