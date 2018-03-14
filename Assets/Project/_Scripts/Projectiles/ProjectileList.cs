using System.Collections.Generic;
using UnityEngine;

public class ProjectileList : MonoBehaviour {

    public static ProjectileList instance;

    private void Awake()
    {
        instance = this;
    }

    public List<GameObject> projectileList = new List<GameObject>();
    public List<Sprite> arrowHitGraphics = new List<Sprite>();

    public GameObject GetProjectile(int id)
    {
        GameObject obj = projectileList[id];
        return obj;
    }

}

public enum ArrowType { StartArrow, Arrow }
public enum ProjectileType { Arrow, Sword, Axe, FireBall }


