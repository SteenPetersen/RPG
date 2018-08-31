using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DungeonLevelLoadLogic : MonoBehaviour {

    [SerializeField] string ZoneToLoad;
    [SerializeField] string[] loadingTexts;
    bool loading;
    [SerializeField] bool makingDecision;
    public bool routineStarted;

    [SerializeField] float decisionTimer;
    [SerializeField] float decisionTime;

    GameObject _text;
    Animator anim;

    void Start()
    {
        _text = transform.Find("Canvas").gameObject;
        anim = _text.GetComponent<Animator>();
    }

    void Update()
    {
        if (makingDecision)
        {
            decisionTimer += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                LoadNextLevel();
            }

            if (decisionTimer > decisionTime)
            {
                if (!routineStarted)
                {
                    StartCoroutine(GameDetails.instance.LerpTime(1, 0.5f, this));
                    routineStarted = true;
                    makingDecision = false;
                    decisionTimer = 0;
                    anim.SetTrigger("popdown");
                }
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (!makingDecision && !routineStarted)
            {
                anim.SetTrigger("popup");
                GameDetails.instance.slowMotion = true;
                makingDecision = true;
            }
        }
    }

    void LoadNextLevel()
    {
        if (!loading)
        {
            decisionTimer = 100;
            // add to statistics
            Debug.Log("Loading Level");
            GameDetails.dungeonFloorsExplored++;
            DungeonManager.dungeonLevel++;

            int rnd = UnityEngine.Random.Range(0, loadingTexts.Length);

            StartCoroutine(GameDetails.MyInstance.FadeOutAndLoadScene(ZoneToLoad, loadingTexts[rnd]));
            loading = true;
        }
    }


}
