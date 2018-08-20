using System.Collections.Generic;
using UnityEngine;

public class SpeechBubbleManager : MonoBehaviour {

    public static SpeechBubbleManager instance;
    GameObject speechCanvas;

    [SerializeField] GameObject speechBubble;
    [SerializeField] List<GameObject> speechPool = new List<GameObject>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        speechCanvas = GameObject.Find("SpeechBubbleCanvas");
    }


    /// <summary>
    /// Pools speechBubbles and makes sure that one is created if 
    /// all currently instantiated speechbubbles are being used.
    /// </summary>
    /// <param name="target">The target which to follow</param>
    /// <param name="text">The text that will be in the speech bubble</param>
    /// <param name="time">How long shall this speech bubble being visible (Default is 4 seconds)</param>
    /// <returns></returns>
    public GameObject FetchBubble(Transform target, EnemyAI script, string text, float time = 4)
    {
        for (int i = 0; i < speechPool.Count; i++)
        {
            if (!speechPool[i].activeInHierarchy)
            {
                speechPool[i].transform.SetParent(speechCanvas.transform);

                speechPool[i].GetComponent<RectTransform>().localScale = new Vector3(0.03f, 0.03f, 0.03f);

                speechPool[i].GetComponent<SpeechFollow>().Init(target, text, time);

                script.speechBubbleId = i;

                return speechPool[i];
            }
        }

        GameObject bubble = Instantiate(speechBubble, target.position, Quaternion.identity);

        bubble.transform.SetParent(speechCanvas.transform);

        bubble.GetComponent<RectTransform>().localScale = new Vector3(0.03f, 0.03f, 0.03f);

        bubble.GetComponent<SpeechFollow>().Init(target, text, time);

        speechPool.Add(bubble);

        script.speechBubbleId = speechPool.Count - 1;

        return bubble;
    }

    /// <summary>
    /// Override method for any object that should gfet a speech bubble but doesnt have an enemyScript
    /// Pools speechBubbles and makes sure that one is created if 
    /// all currently instantiated speechbubbles are being used.
    /// </summary>
    /// <param name="target">The target which to follow</param>
    /// <param name="text">The text that will be in the speech bubble</param>
    /// <param name="time">How long shall this speech bubble being visible (Default is 4 seconds)</param>
    /// <returns></returns>
    public GameObject FetchBubble(Transform target, string text, float time = 4)
    {
        for (int i = 0; i < speechPool.Count; i++)
        {
            if (!speechPool[i].activeInHierarchy)
            {
                speechPool[i].transform.SetParent(speechCanvas.transform);

                speechPool[i].GetComponent<RectTransform>().localScale = new Vector3(0.03f, 0.03f, 0.03f);

                speechPool[i].GetComponent<SpeechFollow>().Init(target, text, time);


                return speechPool[i];
            }
        }

        GameObject bubble = Instantiate(speechBubble, target.position, Quaternion.identity);

        bubble.transform.SetParent(speechCanvas.transform);

        bubble.GetComponent<RectTransform>().localScale = new Vector3(0.03f, 0.03f, 0.03f);

        bubble.GetComponent<SpeechFollow>().Init(target, text, time);

        speechPool.Add(bubble);

        return bubble;
    }

    /// <summary>
    /// Disactivates a specific speechBubble
    /// </summary>
    /// <param name="id">Identification in the list of which bubble it is.</param>
    public void DisactivateBubble(int id)
    {
        speechPool[id].SetActive(false);
    }
}
