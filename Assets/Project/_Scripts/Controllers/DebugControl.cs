using UnityEngine;

public class DebugControl : MonoBehaviour {

    public static DebugControl instance;

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

    public static bool debugOn;
    public static bool debugEnemies;
    public static bool debugEnvironment;
    public static bool debugInventory;
    public static bool debugItemGenerator;
    public static bool debugDungeon;

    [SerializeField] public bool debugOnBool;
    [SerializeField] bool debugEnemiesBool;
    [SerializeField] bool debugEnvironmentBool;
    [SerializeField] bool debugInventoryBool;
    [SerializeField] bool debugItemGeneratorBool;
    [SerializeField] bool debugDungeonBool;

    void Start()
    {
        debugOn = debugOnBool;
        debugEnemies = debugEnemiesBool;
        debugEnvironment = debugEnvironmentBool;
        debugInventory = debugInventoryBool;
        debugItemGenerator = debugItemGeneratorBool;
        debugDungeon = debugDungeonBool;
    }


}
