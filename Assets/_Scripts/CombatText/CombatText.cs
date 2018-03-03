using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatText : MonoBehaviour {

    float speed;
    Vector3 direction;
    public float fadeTime;
    string text;
    public Text textObject;
    float startAlpha;

    Color dmgRed;

    private void Start()
    {

    }

    private void OnEnable()
    {
        Invoke("Destroy", 1f);
    }

    private void Update()
    {
        float translation = speed * Time.deltaTime;
        transform.Translate(direction * translation);
    }

    public void Initialize(float speed, Vector3 direction)
    {
        this.speed = speed;
        this.direction = direction;
    }

    public void Red(string damage, Vector3 position)
    {
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(true);
        textObject.text = "-" + damage;
        gameObject.transform.position = position;
        textObject.color = dmgRed;
        StartCoroutine(FadeOut());
    }

    public void Crit(string damage, Vector3 position)
    {
        gameObject.SetActive(true);
        textObject.text = "-" + damage;
        gameObject.transform.position = position;
        textObject.color = new Color32(236, 221, 61, 255);
        gameObject.transform.localScale = new Vector3(2, 2, 2);
        StartCoroutine(FadeOut());
    }

    public void White(string text, Vector3 position)
    {
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        string displayText = text.Replace("(Clone)", "");
        gameObject.SetActive(true);
        textObject.text = displayText;
        gameObject.transform.position = position;
        textObject.color = Color.white;
        StartCoroutine(FadeOut());
    }

    private void Destroy()
    {
        gameObject.SetActive(false);
        StopCoroutine(FadeOut());
        GetComponent<Text>().color = new Color(255, 255, 255, 255);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private IEnumerator FadeOut()
    {
        gameObject.GetComponent<Outline>().effectColor = new Color(textObject.color.r / 1.5f, textObject.color.g / 1.5f, textObject.color.b / 1.5f);
        float startAlpha = GetComponent<Text>().color.a;
        float rate = 1.0f / fadeTime;
        float progress = 0.0f;
        //Debug.Log(progress + "   " + rate);
        while (progress < 1.0)
        {
            Color tmpColor = GetComponent<Text>().color;
            GetComponent<Text>().color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));

            progress += rate * Time.deltaTime;

            yield return null;
        }


    }

}


