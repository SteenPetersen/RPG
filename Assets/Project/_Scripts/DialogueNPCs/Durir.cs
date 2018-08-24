using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class Durir : DialogueNPC {

    [SerializeField] Item[] swordAndShield;
    SphereCollider col;
    bool conversationDone;

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

        ConversationProgress();

        if (!contentAvailable)
        {
            if (!conversationDone)
            {
                speechEffect.Stop();
                StoryManager.stage = 2;
                GameDetails.instance.Save();
                conversationDone = true;
            }
        }
    }

    /// <summary>
    /// Organisation of this specific NPCs conversation
    /// </summary>
    private void ConversationProgress()
    {
        // if player is currently talking to this NPC
        if (StoryManager.instance.MyCurrentDialogueNpc == this)
        {
            if (currentlyInteractingWithPlayer && currentParagraph == currentParagraphIncrement)
            {
                currentParagraphIncrement = currentParagraph + 1;
                speechText.text = textLines[currentParagraph];
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

