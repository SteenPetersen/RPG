﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PooledProjectilesController : MonoBehaviour {

    public static PooledProjectilesController instance;
    public GameObject Arrow, swordShot, impDagger, ImpBossProjectile;

    public int pooledAmount;
    public bool willGrow = true;

    public List<GameObject> trapCannonBalls = new List<GameObject>();
    public List<GameObject> impFireballs = new List<GameObject>();
    public List<GameObject> aoeFireballs = new List<GameObject>();

    public List<GameObject> pooledArrows = new List<GameObject>();
    public List<GameObject> pooledSwords = new List<GameObject>();

    // Boss'



    public Transform projectileHolder;

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


    public GameObject GetEnemyProjectile(string enemyName, GameObject projectile)
    {
        if (enemyName.EndsWith("(Clone)"))
        {
            enemyName = enemyName.Remove(enemyName.Length - 7);
        }

        switch (enemyName)
        {
            case "ImpRanged":
            case "ImpRanged_Tutorial":

                if (impFireballs.Count != 0)
                {
                    for (int i = 0; i < impFireballs.Count; i++)
                    {
                        if (!impFireballs[i].activeInHierarchy)
                        {
                            impFireballs[i].transform.SetParent(projectileHolder);
                            return impFireballs[i];
                        }
                    }
                }


                GameObject obj = Instantiate(projectile) as GameObject;
                impFireballs.Add(obj);
                obj.transform.SetParent(projectileHolder);
                return obj;


            case "FirstBoss":

                if (aoeFireballs.Count != 0)
                {
                    for (int i = 0; i < aoeFireballs.Count; i++)
                    {
                        if (!aoeFireballs[i].activeInHierarchy)
                        {
                            aoeFireballs[i].transform.SetParent(projectileHolder);
                            return aoeFireballs[i];
                        }
                    }
                }

                GameObject greenfireball = Instantiate(projectile) as GameObject;
                aoeFireballs.Add(greenfireball);
                greenfireball.transform.SetParent(projectileHolder);
                return greenfireball;

            case "fireball_Cannon":

                if (trapCannonBalls.Count != 0)
                {
                    for (int i = 0; i < trapCannonBalls.Count; i++)
                    {
                        if (!trapCannonBalls[i].activeInHierarchy)
                        {
                            trapCannonBalls[i].transform.SetParent(projectileHolder);
                            return trapCannonBalls[i];
                        }
                    }
                }

                GameObject fb = Instantiate(projectile) as GameObject;
                trapCannonBalls.Add(fb);
                fb.transform.SetParent(projectileHolder);
                return fb;
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
        //Debug.Log("loading arrows");

        projectileHolder = new GameObject("ProjectileHolder").transform;

        pooledArrows.Clear();
        trapCannonBalls.Clear();
        impFireballs.Clear();
        pooledSwords.Clear();

        if (EquipmentManager.instance.currentEquipment[3] != null && (int)EquipmentManager.instance.currentEquipment[3].equipType == 1)
        {
            EquipmentManager.instance.SetProjectileType(EquipmentManager.instance.currentEquipment[3].rangedProjectile);
            return;
        }


    }

}
