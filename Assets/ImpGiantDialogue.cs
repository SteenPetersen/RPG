using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpGiantDialogue : DialogueNPC {

    bool hasTalkedToGiant = false;
    ImpGiant combatScript;

    private void Start()
    {
        GameDetails.instance.fadeToBlack.enabled = true;
        dialogueAvailable = GetComponentInChildren<ParticleSystem>();
        gameDetails = GameDetails.instance;
        playerControl = PlayerController.instance;
        cam = CameraController.instance;


        // make a reference to the combat "main" script
        combatScript = GetComponent<ImpGiant>();

        if (textFile != null)
        {
            textLines = (textFile.text.Split('#'));
        }

        Debug.Log(GameDetails.instance.fadeToBlack.color.a);
    }
    // Update is called once per frame
    void Update () {

        if (!hasTalkedToGiant)
        {
            if (!PlayerController.instance.facingRight)
            {
                PlayerController.instance.FlipPlayer();
            }
        }



        if (GameDetails.instance.fadeToBlack.color.a <= 0.02)
        {
            if (!currentlyInteractingWithPlayer && !hasTalkedToGiant)
            {
                player = PlayerController.instance.gameObject.transform;
                base.Interact();
            }
        }


        if (currentlyInteractingWithPlayer)
        {
            speechText.text = textLines[currentParagraph];
        }
    }

    public override void AdvanceSpeech()
    {
        base.AdvanceSpeech();
    }
    public override void CloseDialogue()
    {
        base.CloseDialogue();
        combatScript.dialogueFinished = true;
        Flip();
        hasTalkedToGiant = true;
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
