using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePartner : MonoBehaviour {

    Color startColor;
    Color endColor;
    Color currentColor;

    public FadeBuildingTop partner;


    float startTime;
    public float seconds = 0.5f;
    float t;


    public Material fadeable;
    public Material nonFadeable;

    bool hasFadeMaterial, faded, startTimeSet;

    void Start () {

        startColor = GetComponent<MeshRenderer>().material.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0.0f);
    }
	

	void Update ()
    {
        if (partner.playerInside && !faded)
        {
            if (!startTimeSet)
            {
                gameObject.GetComponent<MeshRenderer>().material = fadeable;
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

        else if (!partner.playerInside && faded)
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
                gameObject.GetComponent<MeshRenderer>().material = nonFadeable;
            }
        }







        //if (partner.playerInside && !faded)
        //{
        //    FadeThisItem();
        //    return;
        //}

        //if (faded)
        //{
        //    UnFadeItem();
        //}

    }

    private void UnFadeItem()
    {
        GetComponent<MeshRenderer>().material.color = partner.gameObject.GetComponent<MeshRenderer>().material.color;


        // if the current color of the item is equal to the start color
        if (GetComponent<MeshRenderer>().material.color == startColor)
        {
            // change the material to a non fadeable material
            GetComponent<MeshRenderer>().material = nonFadeable;

            // let the script know that this item no longer has a fadeable material
            hasFadeMaterial = false;

            // let the script know that the item is not faded
            faded = false;
        }
    }

    private void FadeThisItem()
    {
        // if this item does not have the fadeable material
        if (!hasFadeMaterial)
        {
            // provide it a material that can fade
            GetComponent<MeshRenderer>().material = fadeable;

            // allow the script to know that it now has a fadeable material
            hasFadeMaterial = true;
        }

        if (!startTimeSet)
        {
            startTime = Time.time;
            startTimeSet = true;
        }

        float duration = Time.time - startTime;
        t = duration / seconds;

        currentColor = Color.Lerp(startColor, endColor, t);

        gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", currentColor);

        //GetComponent<MeshRenderer>().material.color = partner.gameObject.GetComponent<MeshRenderer>().material.color;
        

        // if the current color is equal to the end color
        if (GetComponent<MeshRenderer>().material.color == endColor)
        {
            // let the script know that the item is now faded
            faded = true;
        }
    }
}
