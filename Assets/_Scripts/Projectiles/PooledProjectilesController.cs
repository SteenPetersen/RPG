using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PooledProjectilesController : MonoBehaviour {

    public static PooledProjectilesController instance;
    public GameObject Arrow;
    public GameObject impDagger;
    public int pooledAmount;
    public bool willGrow = true;

    public List<GameObject> impDaggers = new List<GameObject>();
    public List<GameObject> pooledArrows = new List<GameObject>();

    Transform projectileHolder;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

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

    public GameObject GetPooledArrow()
    {
        for (int i = 0; i < pooledArrows.Count; i++)
        {
            if (!pooledArrows[i].activeInHierarchy)
            {
                return pooledArrows[i];
            }
        }

        if (willGrow)
        {
            GameObject obj = Instantiate(Arrow) as GameObject;
            pooledArrows.Add(obj);
            obj.transform.SetParent(projectileHolder);
            return obj;
        }

        return null;
    }

    public GameObject GetEnemyProjectile(string enemyName)
    {
        switch (enemyName)
        {
            case "Imp(Clone)":
            case "Imp":

                for (int i = 0; i < impDaggers.Count; i++)
                {
                    if (!impDaggers[i].activeInHierarchy)
                    {
                        return impDaggers[i];
                    }
                }
                Debug.Log("Creating an enemy projectile!");

                GameObject obj = Instantiate(impDagger) as GameObject;
                impDaggers.Add(obj);
                obj.transform.SetParent(projectileHolder);
                return obj;
        }


        return null;
    }

    public void PopulateArrows(GameObject proj)
    {
        pooledArrows.Clear();

        // Pool arrows
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = Instantiate(proj) as GameObject;
            obj.SetActive(false);
            pooledArrows.Add(obj);
            obj.transform.SetParent(projectileHolder);
        }
       
    }

    private void OnDisable()
    {
        // unsubscribe from the scsnemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //TODO this.
        Debug.Log("loading arrows");

        projectileHolder = new GameObject("ProjectileHolder").transform;

        if (EquipmentManager.instance.currentEquipment[3] != null && (int)EquipmentManager.instance.currentEquipment[3].equipType == 1)
        {
            EquipmentManager.instance.SetProjectileType(EquipmentManager.instance.currentEquipment[3].rangedProjectile);
            return;
        }

        pooledArrows.Clear();

    }

}
