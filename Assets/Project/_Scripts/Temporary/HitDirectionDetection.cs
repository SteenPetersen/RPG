using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDirectionDetection : MonoBehaviour {

    public CoordinateDirection dir;
    PlayerController player;

    public bool mouseAbove = false;

    private void Start()
    {
        player = PlayerController.instance;

    }

    private void OnMouseOver()
    {
        mouseAbove = true;
        if (player != null)
        {
            //player.SetMouseQuadrant(dir);

        }
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

public enum CoordinateDirection { NW, NE, SW, SE }

