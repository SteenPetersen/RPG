using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionBelowCharacter : MonoBehaviour {

    public static CollisionBelowCharacter instance;
    public bool mouseBelow = false;

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

    private void OnMouseOver()
    {
        mouseBelow = true;
    }

    private void OnMouseEnter()
    {
        mouseBelow = true;
    }

    private void OnMouseExit()
    {
        mouseBelow = false;
    }
}
