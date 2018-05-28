using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bentos : DialogueNPC {

    SphereCollider col;
    [SerializeField] Item bow, pants;
    [SerializeField] bool bowGiven, pantsGiven;

    protected override void Start()
    {
        base.Start();

        col = GetComponent<SphereCollider>();
    }

    void Update()
    {
        if (StoryManager.stage == 2)
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
        }

        if (StoryManager.stage == 3)
        {
            speechEffect.Stop();
            dialogueAvailable.Stop();
        }


        ConversationProgress();

    }

    /// <summary>
    /// Organisation of this specific NPCs conversation
    /// </summary>
    private void ConversationProgress()
    {
        if (StoryManager.stage == 2)
        {
            if (currentlyInteractingWithPlayer && currentParagraph == currentParagraphIncrement)
            {
                // Start conversation
                if (StoryManager.tutorialConversation == 5)
                {

                    if (currentParagraph == 4 && StoryManager.givenItems < 2)
                    {
                        InventoryScript.instance.AddItem(bow);

                        bool closedBag = InventoryScript.instance.MyBags.Find(x => !x.MyBagScript.isOpen);

                        if (closedBag)
                        {
                            InventoryScript.instance.OpenClose();
                        }

                        StoryManager.givenItems = 2;
                        StoryManager.tutorialConversation = 6;
                    }
                }

                if (StoryManager.tutorialConversation == 6)
                {
                    if (GameDetails.arrowsFired < 10)
                    {
                        contentAvailable = false;
                        currentParagraph = 4;
                    }
                    else
                    {
                        currentParagraph = 5;
                        StoryManager.tutorialConversation = 7;
                    }
                }

                if (StoryManager.tutorialConversation == 7)
                {
                    if (!pantsGiven)
                    {
                        InventoryScript.instance.AddItem(pants);

                        bool closedBag = InventoryScript.instance.MyBags.Find(x => !x.MyBagScript.isOpen);

                        if (closedBag)
                        {
                            InventoryScript.instance.OpenClose();
                        }

                        pantsGiven = true;

                        StoryManager.tutorialConversation = 8;
                    }

                }

                if (StoryManager.tutorialConversation == 8)
                {
                    if (currentParagraph == 6)
                    {
                        StoryManager.tutorialConversation = 9;
                    }
                }

                if (StoryManager.tutorialConversation == 9)
                {
                    currentParagraph = 6;
                    StoryManager.stage = 3;
                    GameDetails.instance.Save();
                }



                currentParagraphIncrement = currentParagraph + 1;
                speechText.text = textLines[currentParagraph];

            }

            // must be done outside loop as character is waiting for 
            // something to happen to continue conversation
            if (StoryManager.tutorialConversation == 6 && GameDetails.arrowsFired > 10)
            {
                contentAvailable = true;
            }
        }

        if (StoryManager.stage > 2)
        {
             currentParagraph = 7;
        }


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
