using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeBuildingTop : MonoBehaviour {


    Color startColor;
    Color currentColor;
    Color endColor;
    float startTime;
    public float seconds = 0.5f;
    float t;

    public Material fadeableMat;
    public Material nonFadeableMat;


    public GameObject topOfBuilding;
    public PolygonCollider2D col;
    public bool playerInside, startTimeSet, faded;


	void Start () {

        startColor = GetComponent<MeshRenderer>().material.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0.0f);

    }

    private void Update()
    {
        if (playerInside && !faded)
        {
            if (!startTimeSet)
            {
                gameObject.GetComponent<MeshRenderer>().material = fadeableMat;
                startTime = Time.time;
                startTimeSet = true;
            }

            float duration = Time.time - startTime;
            t = duration / seconds;

            currentColor = Color.Lerp(startColor, endColor, t);

            gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", currentColor);

            if (currentColor == endColor)
            {
                startTimeSet = false;
                faded = true;
            }
        }

        else if (!playerInside && faded)
        {
            if (!startTimeSet)
            {
                startTime = Time.time;
                startTimeSet = true;
            }

            float duration = Time.time - startTime;
            t = duration / seconds;

            currentColor = Color.Lerp(endColor, startColor, t);

            gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", currentColor);

            if (currentColor == startColor)
            {
                startTimeSet = false;
                faded = false;
                gameObject.GetComponent<MeshRenderer>().material = nonFadeableMat;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D playerCollider)
    {
        if (playerCollider.tag == "Player")
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D playerCollider)
    {
        if (playerCollider.tag == "Player")
        {
            playerInside = false;
        }
    }
}
