using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    #region Singleton


    // something wrong with this.
    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    public bool paused;

    public Plane m_Plane = new Plane(Vector3.forward, Vector3.zero);

    [SerializeField]
    public List<GameObject> generalObjects = new List<GameObject>();

    // after character creation load character to this position
    public GameObject player;


    void Update()
    {
        HandleInput();

        if (paused)
        {
            Time.timeScale = 0;
        }
        else if (!paused)
        {
            Time.timeScale = 1;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
        }
    }

    public void KillPlayer()
    {
        Debug.Log("killing player");
    }
}
