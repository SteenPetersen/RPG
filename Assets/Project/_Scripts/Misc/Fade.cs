using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour
{
    Color startColor;
    Color currentColor;
    Color endColor;
    float startTime;
    public float seconds = 0.5f;
    float t;

    bool shouldFade = true;

    void Start()
    {
        startColor = GetComponent<SpriteRenderer>().material.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0.0f);
        startTime = Time.time;

        for (int childIndex = 0; childIndex < gameObject.transform.childCount; childIndex++)
        {
            if (gameObject.transform.GetChild(childIndex).GetComponent<SpriteRenderer>() != null)
            {
                Transform child = gameObject.transform.GetChild(childIndex);

                child.gameObject.AddComponent<Fade>();
            }
        }
    }

    void Update()
    {
        if (shouldFade)
        {
            FadeStart();
        }
    }

    void FadeStart()
    {
        float duration = Time.time - startTime;
        t = duration / seconds;

        currentColor = Color.Lerp(startColor, endColor, t);

        gameObject.GetComponent<SpriteRenderer>().material.SetColor("_Color", currentColor);


        if (currentColor == endColor)
        {
            shouldFade = false;
            currentColor = startColor;
            gameObject.GetComponent<SpriteRenderer>().material.SetColor("_Color", startColor);
            startTime = 0.0f;
            t = 0.0f;
            Destroy(gameObject);
        }
    
    }
}
