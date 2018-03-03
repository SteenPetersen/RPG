using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAboveCharacter : MonoBehaviour {

    public static CollisionAboveCharacter instance;

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

    public bool mouseAbove = false;

    private void OnMouseOver()
    {
        mouseAbove = true;
    }

    private void OnMouseEnter()
    {
        mouseAbove = true;
    }

    private void OnMouseExit()
    {
        mouseAbove = false;
    }
}
