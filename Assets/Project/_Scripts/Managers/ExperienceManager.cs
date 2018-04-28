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


    public int level;
    [SerializeField]
    public float experience;
    [SerializeField]
    public float experienceRequired;

    public Image fillImage;
    public float lerpSpeed;


    bool isLerping;
    float lerpStartTime;

    private void Start()
    {
        level = 1;
        experience = 0;
        experienceRequired = UpdateExperienceRequired();
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

        if (Input.GetKeyDown(KeyCode.K))
        {
            AddExp(10);
        }
    }

    void LevelUp()
    {
        level += 1;
        experience = 0;

        SoundManager.instance.PlayUiSound("levelup");

        experienceRequired = UpdateExperienceRequired();

        fillImage.fillAmount = 0;

        PlayerController.instance.gameObject.GetComponent<PlayerStats>().LevelUpStats();
    }

    public void AddExp(float exp)
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

        value = (level * level + level + 3) * 6;
        Debug.Log(value);

        return value;
    }

    private void startLerping()
    {
        isLerping = true;
        lerpStartTime = Time.time;
    }
}
