using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterPortal : MonoBehaviour {

    PlayerController player;
    bool routineRunning;

    [SerializeField] int sceneToLoad;
    [SerializeField] Vector2 loadInPosition;
    GameObject followLight;

	void Start ()
    {
        player = PlayerController.instance;

        if (SceneManager.GetActiveScene().name.Contains("_indoor"))
        {
            followLight = GameObject.Find("FollowLight");
        }

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

        DungeonManager dungeon = DungeonManager.instance;
        dungeon._PlayerHasBossKey = false;

        StartCoroutine(GameDetails.MyInstance.FadeOutAndLoadScene(sceneToLoad, loadInPosition));

        if (followLight != null)
        {
            followLight.SetActive(false);
        }

        yield return new WaitForSeconds(1);

    }
}
