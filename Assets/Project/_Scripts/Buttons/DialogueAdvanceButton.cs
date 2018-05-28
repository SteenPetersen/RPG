using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueAdvanceButton : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData)
    {
        StoryManager.instance.MyCurrentDialogueNpc.AdvanceSpeech();
    }

}
