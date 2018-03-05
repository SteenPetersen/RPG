using System.Collections.Generic;
using UnityEngine;

public class ProjectileList : MonoBehaviour {

    public static ProjectileList instance;

    private void Awake()
    {
        instance = this;
    }

    public List<GameObject> projectileList = new List<GameObject>();

    public GameObject GetProjectile(int id)
    {
        GameObject obj = projectileList[id];
        return obj;
    }
}
