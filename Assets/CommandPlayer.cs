using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandPlayer : MonoBehaviour {

    [SerializeField] Text command;
    [SerializeField] bool doneFading;
    [SerializeField] float increment = 0.05f;
    [SerializeField] float waitForSeconds = 0.05f;
    [SerializeField] bool playerInside;
    [SerializeField] ParticleSystem particles;
    [SerializeField] GameObject shakeObject;
    [SerializeField] Image blinkObject;
    [SerializeField] Color endResultOfBlink;

    [SerializeField] bool noCollider;

    /// <summary>
    /// Accessed by the _TutorialManager
    /// </summary>
    public bool MyDoneFading
    {
        get
        {
            return doneFading;
        }

        set
        {
            doneFading = value;
        }
    }

    void Start()
    {
        Color tmp = command.color;
        tmp.a = 0;
        command.color = tmp;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
        StartCoroutine(FadeIn());

        if (!playerInside)
        {
            if (particles != null)
            {
                particles.Play();
            }
            if (shakeObject != null)
            {
                //ShakeManager.instance.shakeGameObject(shakeObject, 3f, 1f, true);
            }
            if (blinkObject != null)
            {
                BlinkColorOnObject();
            }
        }

        playerInside = true;
    }

    private void BlinkColorOnObject()
    {
        Color startColor = blinkObject.color;

        StartCoroutine(Blink(startColor));
    }

    void OnTriggerExit2D(Collider2D col)
    {
        Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
        StartCoroutine(FadeOut());

        if (playerInside)
        {
            if (particles != null)
            {
                particles.Stop();
            }
        }

        playerInside = false;
    }

    IEnumerator Blink(Color startColor)
    {
        yield return null;

        while (playerInside)
        {
            Color tmp = blinkObject.color;
            tmp.r = endResultOfBlink.r;
            tmp.g = endResultOfBlink.g;
            tmp.b = endResultOfBlink.b;
            blinkObject.color = tmp;
            Debug.Log("first!");
            yield return new WaitForSeconds(0.2f);

            Color start = blinkObject.color;
            start.r = startColor.r;
            start.g = startColor.g;
            start.b = startColor.b;
            blinkObject.color = start;
            Debug.Log("second!");
            yield return new WaitForSeconds(0.2f);
        }


        //blinkObject.color = startColor;
    }

    /// <summary>
    /// Fades in the text of this command
    /// Public because it is also accessed by the _Tutorial Manager
    /// </summary>
    /// <returns></returns>
    public IEnumerator FadeIn()
    {
        while (!MyDoneFading)
        {
            Color tmp = command.color;
            tmp.a += increment;
            command.color = tmp;


            if (command.color.a > 0.99f)
            {
                MyDoneFading = true;
            }

            yield return new WaitForSeconds(waitForSeconds);
        }

        if (!noCollider)
        {
            if (!playerInside)
            {
                StartCoroutine(FadeOut());
            }
        }

    }

    /// <summary>
    /// Fades out the text of this command
    /// Public because it is also accessed by the _Tutorial Manager
    /// </summary>
    /// <returns></returns>
    public IEnumerator FadeOut()
    {
        while (MyDoneFading)
        {
            Color tmp = command.color;
            tmp.a -= increment;
            command.color = tmp;

            if (command.color.a < 0.003f)
            {
                MyDoneFading = false;
            }

            yield return new WaitForSeconds(waitForSeconds);
        }

        if (playerInside)
        {
            StartCoroutine(FadeIn());
        }
    }
}
