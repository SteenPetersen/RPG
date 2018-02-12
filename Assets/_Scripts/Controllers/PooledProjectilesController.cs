using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledProjectilesController : MonoBehaviour {

    public static PooledProjectilesController instance;
    public GameObject Arrow;
    public int pooledAmount;
    public bool willGrow = true;

    List<GameObject> pooledArrows = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    void Start () {

        if (Arrow != null)
        {
            for (int i = 0; i < pooledAmount; i++)
            {
                GameObject obj = (GameObject)Instantiate(Arrow);
                obj.SetActive(false);
                pooledArrows.Add(obj);
            }
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

    public void ReplaceArrows(GameObject proj)
    {
        //pooledArrows.Clear();

        for (int i = 0; i < pooledArrows.Count; i++)
        {
            if (!pooledArrows[i].activeInHierarchy)
            {
                Destroy(pooledArrows[i]);
            }
        }

        pooledArrows.Clear();

        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(proj);
            obj.SetActive(false);
            pooledArrows.Add(obj);
        }
    }

}
