using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterPortal : MonoBehaviour {

    PlayerController player;

	void Start ()
    {
        player = PlayerController.instance;
	}
	

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            Debug.Log("Player entered the portal");
            StartCoroutine(Portal());
        }
    }

    IEnumerator Portal()
    {
        ParticleSystemHolder.instance.PlaySpellEffect(PlayerController.instance.transform.position, transform.name);
        player.transform.Find("Skeleton").gameObject.SetActive(false);
        player.Portal = true;
        SoundManager.instance.PlayEnvironmentSound("enter_portal");

        yield return new WaitForSeconds(1);

        SceneManager.LoadSceneAsync(0);
        PlayerController.instance.transform.position = new Vector2(-12f, 4);
        DungeonManager dungeon = DungeonManager.Instance;
        dungeon.playerHasBossKey = false;
    }
}
