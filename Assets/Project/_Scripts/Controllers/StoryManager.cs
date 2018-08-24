using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour {

    public static StoryManager instance;

    [SerializeField] Image notification;
    [SerializeField] Text notice;
    [SerializeField] Animator noticeAnimator;

    public static int reylithIntro;
    [SerializeField] public static int stage;
    public static int tutorialStage;
    public static int questLine;

    /// <summary>
    /// necessary to ensure that player cannot save 
    /// and reload to keep being given items
    /// </summary>
    public static int givenItems;
    [SerializeField] int givenItemsSerialization;

    [SerializeField] bool showStatics;
    [SerializeField] int currentStage;

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

    /// <summary>
    /// Assets used to create the dialogue environment
    /// </summary>
    [SerializeField] Image speechBubble;
    [SerializeField] Sprite bubbleRight;
    [SerializeField] Sprite bubbleLeft;
    [SerializeField] Text speechText;
    [SerializeField] GameObject dialogueUi;
    [SerializeField] DialogueNPC currentDialogueNpc;

    private void Update()
    {
        if (givenItems != givenItemsSerialization)
        {
            givenItemsSerialization = givenItems;
        }

        if (showStatics)
        {
            currentStage = tutorialStage;
        }
    }


    /// <summary>
    /// Used to make sure the StoryManager always knows 
    /// what NPC the player is interacting with
    /// </summary>
    public DialogueNPC MyCurrentDialogueNpc
    {
        get { return currentDialogueNpc; }
        set { currentDialogueNpc = value; }
    }

    public GameObject MyDialogueUi
    {
        get { return dialogueUi; }
    }

    public Image MySpeechBubble
    {
        get { return speechBubble; }
    }

    public Sprite MyBubbleRight
    {
        get { return bubbleRight; }
    }

    public Sprite MyBubbleLeft
    {
        get { return bubbleLeft; }
    }

    public Text MySpeechText
    {
        get { return speechText; }
    }

    public void NotifyPlayer(string note)
    {
        if (!noticeAnimator.GetCurrentAnimatorStateInfo(0).IsName("notificationNotify"))
        {
            notice.text = note;
            noticeAnimator.SetTrigger("notify");
        }
    }
}
