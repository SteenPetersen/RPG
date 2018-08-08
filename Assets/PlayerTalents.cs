using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTalents : MonoBehaviour {

    public static PlayerTalents instance;

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

    [SerializeField] int projectile = 0;

    public int MyProjectile
    {
        get
        {
            return projectile;
        }

        set
        {
            projectile = value;
            PlayerController.instance.MyProjectileSpeed = 800 + (value * 25); //TODO Play with this number to ensure the change is noticable and fun
        }
    }
}
