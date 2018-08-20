using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Durir : DialogueNPC {

    [SerializeField] Item[] swordAndShield;
    SphereCollider col;

    protected override void Start()
    {
        base.Start();

        col = GetComponent<SphereCollider>();
    }

    void Update()
    {

            if (StoryManager.stage == 1)
            {
                if (!currentlyInteractingWithPlayer)
                {
                    if (speechEffect.isPlaying)
                    {
                        speechEffect.Stop();
                    }

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

            if (StoryManager.stage == 2)
            {
                speechEffect.Stop();
            }

            if (StoryManager.stage == 3)
            {
                if (!currentlyInteractingWithPlayer)
                {
                    if (speechEffect.isPlaying)
                    {
                        speechEffect.Stop();
                    }

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

            if (StoryManager.stage == 4)
            {
                if (!currentlyInteractingWithPlayer)
                {
                    if (speechEffect.isPlaying)
                    {
                        speechEffect.Stop();
                    }

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

            ConversationProgress();
    }

    /// <summary>
    /// Organisation of this specific NPCs conversation
    /// </summary>
    private void ConversationProgress()
    {
        // if player is currently talking to this NPC
        if (StoryManager.instance.MyCurrentDialogueNpc == this)
        {
            #region tutorial conversation
            if (StoryManager.questLine == 0)
            {
                if (StoryManager.stage == 1)
                {
                    if (currentlyInteractingWithPlayer && currentParagraph == currentParagraphIncrement)
                    {
                        // intro text and up until player is handed sword and shield
                        if (StoryManager.tutorialStage == 0)
                        {
                            if (currentParagraph == 5 && StoryManager.givenItems < 1)
                            {
                                foreach (Item item in swordAndShield)
                                {
                                    InventoryScript.instance.AddItem(item);
                                }

                                bool closedBag = InventoryScript.instance.MyBags.Find(x => !x.MyBagScript.isOpen);

                                if (closedBag)
                                {
                                    InventoryScript.instance.OpenClose();
                                }

                                StoryManager.givenItems = 1;
                            }

                            if (currentParagraph == 6)
                            {
                                StoryManager.tutorialStage = 1;
                            }
                        }

                        // making sure player has equipped sword and shield
                        if (StoryManager.tutorialStage == 1)
                        {
                            if (EquipmentManager.instance.currentEquipment[3] == null ||
                                EquipmentManager.instance.currentEquipment[4] == null)
                            {
                                contentAvailable = false;
                                currentParagraph = 6;
                            }
                            else
                            {
                                contentAvailable = true;
                                StoryManager.tutorialStage = 2;
                                currentParagraph = 7;
                            }
                        }

                        // finish the sword and shield info
                        if (StoryManager.tutorialStage == 2)
                        {
                            if (currentParagraph == 9)
                            {
                                StoryManager.tutorialStage = 3;
                            }
                        }

                        // Introduce player to hitting a target.
                        if (StoryManager.tutorialStage == 3)
                        {
                            if (GameDetails.ripostes == 0 ||
                            GameDetails.blocks == 0 ||
                            GameDetails.hits == 0 ||
                            GameDetails.fullChargeHits == 0)
                            {
                                contentAvailable = false;
                                currentParagraph = 9;
                            }

                            else if (GameDetails.ripostes > 0 &&
                            GameDetails.blocks > 0 &&
                            GameDetails.hits > 0 &&
                            GameDetails.fullChargeHits > 0)
                            {
                                contentAvailable = true;
                                currentParagraph = 10;
                                StoryManager.tutorialStage = 4;
                            }

                        }


                        if (StoryManager.tutorialStage == 4)
                        {
                            if (currentParagraph == 11)
                            {
                                StoryManager.tutorialStage = 5;
                            }
                        }

                        // Done with conversation
                        if (StoryManager.tutorialStage == 5)
                        {
                            contentAvailable = false;
                            currentParagraph = 11;
                            StoryManager.stage = 2;
                            GameDetails.instance.Save();
                        }

                        currentParagraphIncrement = currentParagraph + 1;
                        speechText.text = textLines[currentParagraph];

                    }


                }

                if (StoryManager.stage == 3)
                {
                    if (currentlyInteractingWithPlayer && currentParagraph == currentParagraphIncrement)
                    {
                        if (StoryManager.questLine == 0)
                        {
                            contentAvailable = true;
                            currentParagraph = 12;
                            // analytics
                            ReportEndOfConversation(gameObject.name);
                            StoryManager.questLine = 1;
                            StoryManager.stage = 4;
                            GameDetails.instance.Save();
                        }
                    }

                    currentParagraphIncrement = currentParagraph + 1;
                    speechText.text = textLines[currentParagraph];
                }
            }

            #endregion tutorial conversation

            #region storyLine Start

            if (StoryManager.questLine == 1)
            {
                if (currentlyInteractingWithPlayer && currentParagraph == currentParagraphIncrement)
                {
                    if (StoryManager.stage == 4)
                    {
                        currentParagraph = 13;
                        StoryManager.stage = 5;
                    }

                    if (currentParagraph == 20)
                    {
                        StoryManager.questLine = 2;
                        StoryManager.stage = 6;
                        GameDetails.instance.Save();
                    }

                    //Debug.Log("current paragraph = " + currentParagraph);

                    currentParagraphIncrement = currentParagraph + 1;
                    speechText.text = textLines[currentParagraph];
                }
            }

            if (StoryManager.questLine == 2)
            {
                currentParagraph = 20;
            }

            #endregion storyLine Start
        }

        // if he is not currently talking to him
        if (!currentlyInteractingWithPlayer)
        {
            if (StoryManager.tutorialStage == 3)
            {
                if (GameDetails.ripostes > 0 &&
                    GameDetails.blocks > 0 &&
                    GameDetails.hits > 0 &&
                    GameDetails.fullChargeHits > 0)
                {
                    contentAvailable = true;
                }
            }

            // if he has finished talking to bentos and has not yet talked to durir
            if (StoryManager.stage == 3 && StoryManager.questLine == 0)
            {
                contentAvailable = true;
            }

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

    public void ReportEndOfConversation(string npcName)
    {
        Analytics.CustomEvent("Reached end of conversation", new Dictionary<string, object>
    {
        { "npc_name", npcName },
        { "time_elapsed", Time.timeSinceLevelLoad }
    });
    }
}

