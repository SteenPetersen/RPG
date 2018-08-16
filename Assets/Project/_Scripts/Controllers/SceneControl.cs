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

    /// <summary>
    /// The position to which the player will return on saved game 
    /// or when porting back to this zone
    /// </summary>
    public Vector2 MyCurrentHomePosition
    {
        get
        {
            Debug.Log("Trying to fetch homePosition");
            return currentHomePosition;
        }

        set
        {
            currentHomePosition = value;
        }
    }

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

        if (scene.name == "main")
        {
            MyCurrentHomePosition = new Vector2(-18, -18);

            Debug.Log("I have set the current pos" + MyCurrentHomePosition);

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
}
