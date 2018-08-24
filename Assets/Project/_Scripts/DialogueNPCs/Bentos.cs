using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bentos : DialogueNPC {

    SphereCollider col;
    [SerializeField] Item bow;
    [SerializeField] Item pants;
    [SerializeField] bool bowGiven;
    [SerializeField] bool pantsGiven;
    [SerializeField] TutorialManager tut;
    [SerializeField] bool speaking;
    [SerializeField] string[] sayings;
    [SerializeField] Transform speechPosition;

    protected override void Start()
    {
        base.Start();

        col = GetComponent<SphereCollider>();
        tut = TutorialManager.instance;
    }

    void Update()
    {
        if (!currentlyInteractingWithPlayer)
        {
            speechEffect.Stop();

            if (!dialogueAvailable.isPlaying)
            {
                if (contentAvailable)
                {
                    dialogueAvailable.Play();

                    if (!col.enabled)
                    {
                        col.enabled = true;
                    }
                }
            }
        }

        if (StoryManager.tutorialStage == 1)
        {
            if (tut.MyOverTheLine)
            {
                if (!speaking)
                {
                    int rnd = UnityEngine.Random.Range(0, sayings.Length - 1);
                    SpeechBubbleManager.instance.FetchBubble(speechPosition, sayings[rnd]);
                    speaking = true;
                }
            }
            else if (!tut.MyOverTheLine && speaking)
            {
                speaking = false;
            }
        }


        ConversationProgress();

    }

    /// <summary>
    /// Organisation of this specific NPCs conversation
    /// </summary>
    private void ConversationProgress()
    {
        if (currentlyInteractingWithPlayer && currentParagraph == currentParagraphIncrement)
        {
            /// Start conversation
            /// if you havent done anything and you've reached the end of NPC intro
            if (StoryManager.tutorialStage == 0 && currentParagraph > 5)
            {
                currentParagraph = 5;
                contentAvailable = false;
                StoryManager.tutorialStage = 1;
                CloseDialogue();
            }


            else if (StoryManager.tutorialStage == 2)
            {
                GivePlayerItem(pants, pantsGiven);
                StoryManager.tutorialStage = 3;
            }

            currentParagraphIncrement = currentParagraph + 1;
            speechText.text = textLines[currentParagraph];
        }

        if (StoryManager.tutorialStage == 2)
        {
            if (!contentAvailable)
            {
                ReadyToTalkAgain(6);
            }
        }

        else if (StoryManager.tutorialStage == 3 && !currentlyInteractingWithPlayer)
        {
            currentParagraph = 7;

            if (!TutorialManager.instance.archeryDoorsOpen)
            {
                TutorialManager.instance.OpenArcheryDoors();
            }
        }


    }



    private void GivePlayerItem(Item item, bool boolToCHange)
    {
        InventoryScript.instance.AddItem(item);

        bool closedBag = InventoryScript.instance.MyBags.Find(x => !x.MyBagScript.isOpen);

        if (closedBag)
        {
            InventoryScript.instance.OpenClose();
        }

        boolToCHange = true;
    }

    public override void AdvanceSpeech()
    {
        base.AdvanceSpeech();
    }

    protected override void CloseDialogue()
    {
        base.CloseDialogue();
    }

    public override void StopDialoguePrematurely()
    {
        base.StopDialoguePrematurely();
    }

    public override void Interact()
    {
        base.Interact();
    }
}

#region oldConversation
//if (StoryManager.tutorialConversation == 5)
//{

//    if (currentParagraph == 4 && StoryManager.givenItems < 2)
//    {
//        InventoryScript.instance.AddItem(bow);

//        bool closedBag = InventoryScript.instance.MyBags.Find(x => !x.MyBagScript.isOpen);

//        if (closedBag)
//        {
//            InventoryScript.instance.OpenClose();
//        }

//        StoryManager.givenItems = 2;
//        StoryManager.tutorialConversation = 6;
//    }
//}

//if (StoryManager.tutorialConversation == 6)
//{
//    if (GameDetails.arrowsFired < 10)
//    {
//        contentAvailable = false;
//        currentParagraph = 4;
//    }
//    else
//    {
//        currentParagraph = 5;
//        StoryManager.tutorialConversation = 7;
//    }
//}

//if (StoryManager.tutorialConversation == 7)
//{
//    if (!pantsGiven)
//    {
//        InventoryScript.instance.AddItem(pants);

//        bool closedBag = InventoryScript.instance.MyBags.Find(x => !x.MyBagScript.isOpen);

//        if (closedBag)
//        {
//            InventoryScript.instance.OpenClose();
//        }

//        pantsGiven = true;

//        StoryManager.tutorialConversation = 8;
//    }

//}

//if (StoryManager.tutorialConversation == 8)
//{
//    if (currentParagraph == 6)
//    {
//        StoryManager.tutorialConversation = 9;
//    }
//}

//if (StoryManager.tutorialConversation == 9)
//{
//    currentParagraph = 6;
//    StoryManager.stage = 3;
//    GameDetails.instance.Save();
//}

#endregion oldConversation

