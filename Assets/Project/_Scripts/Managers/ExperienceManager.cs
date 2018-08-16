using System;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour {

    public static ExperienceManager instance;
    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }


    static int level;
    static int talentPoints;

    public float experience;
    public float experienceRequired;

    public Image fillImage;
    public float lerpSpeed;


    bool isLerping;
    float lerpStartTime;
    PlayerStats pStats;

    /// <summary>
    /// The level of the chracter
    /// </summary>
    public static int MyLevel
    {
        get
        {
            return level;
        }

        set
        {
            level = value;
        }
    }

    /// <summary>
    /// The talent points of the chracter
    /// </summary>
    public static int MyTalentPoints
    {
        get
        {
            return talentPoints;
        }

        set
        {
            talentPoints = value;
        }
    }

    private void Start()
    {
        MyLevel = 1;
        experience = 0;
        experienceRequired = UpdateExperienceRequired();
        pStats = PlayerController.instance.gameObject.GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (isLerping)
        {
            float timeSinceStarted = Time.time - lerpStartTime;
            float percentageComplete = timeSinceStarted / lerpSpeed;

            var one = Mathf.Clamp(experience / experienceRequired, 0, 1);

            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, one, percentageComplete);

            //Debug.Log("lerping");

            if (percentageComplete >= 1.0f)
            {
                isLerping = false;
            }
        }

        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    AddExp(10);
        //}
    }

    void LevelUp()
    {
        MyLevel += 1;
        MyTalentPoints += 1;
        experience = 0;

        Vector3 pos = PlayerController.instance.transform.position;

        GameObject tmp = ParticleSystemHolder.instance.PlaySpellEffect(PlayerController.instance.transform.position, "level up");
        tmp.transform.parent = PlayerController.instance.transform;
        SoundManager.instance.PlayUiSound("levelup");

        var text = CombatTextManager.instance.FetchText(pos);
        var textScript = text.GetComponent<CombatText>();
        textScript.White("Level up!", pos);
        text.transform.position = pos;
        text.SetActive(true);
        textScript.FadeOut();

        experienceRequired = UpdateExperienceRequired();

        fillImage.fillAmount = 0;

        pStats.LevelUpStats();
    }

    /// <summary>
    /// Determine how much experience is given depending on 
    /// enemy tier and player level
    /// </summary>
    /// <param name="exp"></param>
    /// <param name="tier"></param>
    public void AddExp(float exp, int tier)
    {
        switch (tier)
        {
            case 0:
                if (MyLevel < 5)
                {
                    GrantExp(exp);
                }
                else
                {
                    GrantExp(exp / MyLevel);
                }
                break;
            case 1:
                if (MyLevel < 10)
                {
                    GrantExp(exp);
                }
                else
                {
                    GrantExp(exp / MyLevel);
                }
                break;
            case 2:
                if (MyLevel < 15)
                {
                    GrantExp(exp);
                }
                else
                {
                    GrantExp(exp / MyLevel);
                }
                break;
        }
    }

    private void GrantExp(float exp)
    {
        experience += exp;

        startLerping();

        if (experience >= experienceRequired)
        {
            LevelUp();
        }
    }

    // needed so we dont have a sound effect upon loading of dinging etc.
    public void AddExpFromLoadedGame(float exp)
    {
        experience += exp;
        startLerping();
    }

    private float UpdateExperienceRequired()
    {
        float value = 0;

        value = (MyLevel * MyLevel + MyLevel + 3) * 6;

        return value;
    }

    private void startLerping()
    {
        isLerping = true;
        lerpStartTime = Time.time;
    }
}
