using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpGiantDialogue : DialogueNPC {

    bool hasTalkedToGiant = false;
    ImpGiant combatScript;



    protected override void Start()
    {
        base.Start();


        GameDetails._instance.fadeToBlack.enabled = true;

        // make a reference to the combat "main" script
        combatScript = GetComponent<ImpGiant>();
    }

    void Update () {

        if (!hasTalkedToGiant)
        {
            if (!PlayerController.instance.facingRight)
            {
                PlayerController.instance.FlipPlayer();
            }
        }

        if (GameDetails._instance.fadeToBlack.color.a <= 0.02)
        {
            if (!currentlyInteractingWithPlayer && !hasTalkedToGiant)
            {
                base.Interact();
            }
        }


        if (currentlyInteractingWithPlayer && currentParagraph == currentParagraphIncrement)
        {
            currentParagraphIncrement = currentParagraph + 1;
            speechText.text = textLines[currentParagraph];
            SoundManager.instance.PlayDialogueSound(gameObject.name);
        }
    }

    public override void AdvanceSpeech()
    {
        base.AdvanceSpeech();
    }

    protected override void CloseDialogue()
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
