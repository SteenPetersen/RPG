using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour {

    public static SceneControl instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    public static int dungeonLevel;
    public int dungeonFloorCount;

    [SerializeField] GameObject reylith;
    [SerializeField] Vector2 currentHomePosition;

    Dictionary<string, Vector2> homePositions = new Dictionary<string, Vector2>();

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

        if (scene.name == "Start_Area")
        {
            Debug.Log("saving start area");
            Vector2 homePos = new Vector2(-18, -18);

            if (!homePositions.ContainsKey(scene.name))
            {
                homePositions.Add(scene.name, homePos);
            }

            if (StoryManager.stage == 0)
            {
                Vector3 pos = new Vector3(-22.75f, -15f, 0);
                Instantiate(reylith, pos, Quaternion.identity);
            }
        }

        dungeonFloorCount = dungeonLevel;
    }

    private void OnDisable()
    {
        // Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. 
        // Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    /// <summary>
    /// Returns the current homeposition of the scene requested.
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    public Vector2 MyCurrentHomePosition(string scene)
    {
        foreach (KeyValuePair<string, Vector2> s in homePositions)
        {
            if (s.Key == scene)
            {
                return s.Value;
            }
        }

        Debug.LogError("Invalid Scene name detected return 0");
        return Vector2.zero;
    }
}
