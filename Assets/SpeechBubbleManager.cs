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
}
