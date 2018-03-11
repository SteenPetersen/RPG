using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHolder : MonoBehaviour {

    public static EnemyHolder instance;

    public List<GameObject> enemies = new List<GameObject>();

    public int maxAmountOfEnemies;


    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start () {
		
	}
	
	void Update () {
		
	}
}
