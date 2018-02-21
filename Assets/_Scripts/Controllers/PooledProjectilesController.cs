using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PooledProjectilesController : MonoBehaviour {

    public static PooledProjectilesController instance;
    public GameObject Arrow;
    public int pooledAmount;
    public bool willGrow = true;

    public List<GameObject> pooledArrows = new List<GameObject>();

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
            GameObject obj = (GameObject)Instantiate(Arrow);
            pooledArrows.Add(obj);
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
            GameObject obj = (GameObject)Instantiate(proj);
            obj.SetActive(false);
            pooledArrows.Add(obj);
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

        if (EquipmentManager.instance.currentEquipment[3] != null && (int)EquipmentManager.instance.currentEquipment[3].equipType == 1)
        {
            EquipmentManager.instance.SetProjectileType(EquipmentManager.instance.currentEquipment[3].rangedProjectile);
            return;
        }

        pooledArrows.Clear();

    }

}
