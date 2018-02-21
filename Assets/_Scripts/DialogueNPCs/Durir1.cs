using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Durir1 : DialogueNPC {

    void Update () {

        InteractWithNPC();

        if (!dialogueAvailable.isPlaying && !currentlyInteractingWithPlayer && contentAvailable)
        {
            dialogueAvailable.Play();
        }

        if (currentlyInteractingWithPlayer)
        {
            speechText.text = textLines[currentParagraph];
        }

        if (player == null)
        {
            player = gameDetails.player.transform;
        }
        if (playercontrol == null)
        {
            playercontrol = PlayerController.instance;
        }
    }

    public override void InteractWithNPC()
    {
        base.InteractWithNPC();
    }

    public override void AdvanceSpeech()
    {
        base.AdvanceSpeech();

        if (currentParagraph == textLines.Length - 1 && !contentAvailable)
        {
            gameDetails.stage = 1;
            gameDetails.kingSpeech = currentParagraph;
        }
    }
    public override void CloseDialogue()
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

        currentParagraph = gameDetails.kingSpeech;
    }
}
