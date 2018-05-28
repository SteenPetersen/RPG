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
    float progress = 0.0f;


    private void Start()
    {

    }

    private void OnEnable()
    {
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
        textObject.color = new Color32(178, 47, 47, 255);
        gameObject.GetComponent<Outline>().effectColor = new Color32(144, 37, 37, 255);
        gameObject.transform.position = position;
        startAlpha = GetComponent<Text>().color.a;
        StartCoroutine(FadeOut());
    }

    public void Gray(string text, Vector3 position)
    {
        gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 1);
        gameObject.SetActive(true);
        textObject.text = text;
        textObject.color = new Color32(141, 141, 141, 255);
        gameObject.GetComponent<Outline>().effectColor = new Color32(113, 113, 113, 255);
        gameObject.transform.position = position;
        startAlpha = GetComponent<Text>().color.a;
        StartCoroutine(FadeOut());
    }

    public void Green(string heal, Vector3 position)
    {
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(true);
        textObject.text = "+" + heal;
        textObject.color = new Color32(47, 184, 42, 255);
        gameObject.GetComponent<Outline>().effectColor = new Color32(40, 140, 37, 255);
        gameObject.transform.position = position;
        startAlpha = GetComponent<Text>().color.a;
        StartCoroutine(FadeOut());
    }

    public void Crit(string damage, Vector3 position)
    {
        gameObject.SetActive(true);
        textObject.text = "-" + damage;
        gameObject.transform.position = position;
        textObject.color = new Color32(236, 221, 61, 255);
        gameObject.GetComponent<Outline>().effectColor = new Color32(195, 183, 57, 255);
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        startAlpha = GetComponent<Text>().color.a;
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
        gameObject.GetComponent<Outline>().effectColor = Color.gray;
        textObject.transform.rotation = Camera.main.transform.rotation;
        startAlpha = GetComponent<Text>().color.a;
        StartCoroutine(FadeOut());
    }

    public void Purple(string damage, Vector3 position)
    {
        gameObject.SetActive(true);
        textObject.text = damage;
        gameObject.transform.position = position;
        textObject.color = new Color32(164, 29, 178, 255);
        gameObject.GetComponent<Outline>().effectColor = new Color32(129, 18, 141, 255);
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        startAlpha = GetComponent<Text>().color.a;
        StartCoroutine(FadeOut());
    }

    private void MakeTextInactive()
    {
        gameObject.SetActive(false);
        StopCoroutine(FadeOut());
        GetComponent<Text>().color = new Color(255, 255, 255, 255);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    public IEnumerator FadeOut()
    {
        gameObject.GetComponent<Outline>().effectColor = new Color(textObject.color.r / 1.5f, textObject.color.g / 1.5f, textObject.color.b / 1.5f);
        float rate = 0.7f / fadeTime;

        while (progress < 1.0)
        {
            Color tmpColor = GetComponent<Text>().color;
            GetComponent<Text>().color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));

            progress += rate * Time.deltaTime;

            yield return null;
        }

        MakeTextInactive();
        progress = 0.0f;
    }

}


