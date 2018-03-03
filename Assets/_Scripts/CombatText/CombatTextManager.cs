using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatTextManager : MonoBehaviour {

    public GameObject textPrefab;
    GameObject textCanvas;

    public static CombatTextManager instance;

    public float speed;
    public Vector3 direction;
    public List<GameObject> textPool = new List<GameObject>();

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
        textCanvas = GameObject.Find("CombatTextCanvas");
    }


    public GameObject FetchText(Vector3 position)
    {

        for (int i = 0; i < textPool.Count; i++)
        {
            if (!textPool[i].activeInHierarchy)
            {
                return textPool[i];
            }
        }

        GameObject sct = Instantiate(textPrefab, position, Quaternion.identity);

        sct.transform.SetParent(textCanvas.transform);

        sct.transform.position = position;
        sct.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        sct.GetComponent<CombatText>().Initialize(speed, direction);

        textPool.Add(sct);

        return sct;
    }
}

