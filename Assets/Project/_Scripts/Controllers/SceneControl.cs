using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour {

    public static SceneControl instance;
    [SerializeField]
    public static int dungeonLevel;
    public int dungeonFloorCount;

    private void OnEnable()
    {
        // Tell our 'OnLevelFinishedLoading' function to start 
        // listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "dungeon_indoor")
        {
            dungeonLevel += 1;
        }
        else if (scene.name != "dungeon_indoor" && scene.name != "Loading")
        {
            dungeonLevel = 0;
        }
        if (scene.name == "bossroom1_indoor")
        {
            var spawn = GameObject.Find("PlayerSpawnPosition");
            PlayerController.instance.gameObject.transform.position = spawn.transform.position;
        }

        dungeonFloorCount = dungeonLevel;
    }

    private void OnDisable()
    {
        // Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. 
        // Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
}
