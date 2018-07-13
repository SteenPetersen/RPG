﻿using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class Reylith : DialogueNPC {

    [SerializeField] AIDestinationSetter setter;
    [SerializeField] Animator anim;
    Transform dungeonEntrance;
    [SerializeField] int progress;
    [SerializeField] bool conversationDone;
    GameObject skeleton;
    bool fading;
    [SerializeField] BoatMovement boat;

    protected override void Start()
    {
        base.Start();
        dungeonEntrance = GameObject.Find("BoatLocation").transform;
        boat = GameObject.Find("Boat").GetComponent<BoatMovement>();
        skeleton = transform.Find("logic").transform.Find("Skeleton").gameObject;
    }

    void Update()
    {
        if (contentAvailable)
        {
            if (!dialogueAvailable.isPlaying && !currentlyInteractingWithPlayer)
            {
                dialogueAvailable.Play();
                speechEffect.Stop();
            }
        }


        ConversationProgress();


        if (!contentAvailable)
        {
            if (!fading)
            {
                if (!conversationDone)
                {
                    speechEffect.Stop();
                    StoryManager.stage = 1;
                    GameDetails._instance.Save();
                    conversationDone = true;
                }


                if (setter.targetASTAR == null)
                {
                    Debug.Log("Out of content");
                    setter.targetASTAR = dungeonEntrance;
                    anim.SetBool("run", true);

                    float frontPos = Vector3.Distance(front.transform.position, dungeonEntrance.transform.position);
                    float backPos = Vector3.Distance(back.transform.position, dungeonEntrance.transform.position);

                    if (frontPos > backPos)
                    {
                        Flip();
                    }
                }
            }


            if (transform.position == dungeonEntrance.transform.position)
            {
                if (!fading)
                {
                    FadeOut();
                }

            }
        }

    }

    /// <summary>
    /// Fades out Reylith 
    /// </summary>
    public void FadeOut()
    {
        skeleton.AddComponent<Fade>();
        fading = true;
        Destroy(gameObject, 3);
    }

    /// <summary>
    /// sets the boat to start sailing just before the 
    /// reylith gameObject is destroyed
    /// </summary>
    private void OnDestroy()
    {
        boat.readyToSail = true;
    }

    /// <summary>
    /// Organisation of this specific NPCs conversation
    /// </summary>
    private void ConversationProgress()
    {
        if (currentlyInteractingWithPlayer && currentParagraph == currentParagraphIncrement)
        {
            currentParagraphIncrement = currentParagraph + 1;
            speechText.text = textLines[currentParagraph];

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
