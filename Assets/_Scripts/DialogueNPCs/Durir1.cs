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
            player = playerManager.player.transform;
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
            playerManager.stage = 1;
            playerManager.kingSpeech = currentParagraph;
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

        currentParagraph = playerManager.kingSpeech;
    }
}
