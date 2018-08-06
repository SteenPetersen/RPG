using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeechFollow : MonoBehaviour {

    public Transform currentTarget;
    [SerializeField] Text myText;
    [SerializeField] float timeVisible;
    [SerializeField] float timer;

    void Update()
    {
        if (timer > -1)
        {
            timer -= Time.deltaTime;
        }

        if (currentTarget != null && gameObject.activeInHierarchy == true)
        {
            transform.position = currentTarget.position;
        }

        if (timer < 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void Init(Transform tar, string text, float time)
    {
        gameObject.SetActive(true);
        currentTarget = tar;
        myText.text = text;
        timer = time;
    }
	
}
