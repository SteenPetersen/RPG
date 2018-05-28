using UnityEngine;
using UnityEngine.UI;

public abstract class DialogueNPC : Interactable {

    [HideInInspector] protected CameraController cam;
    [SerializeField] protected bool currentlyInteractingWithPlayer;

    [SerializeField] protected GameObject front;
    [SerializeField] protected GameObject back;

    protected GameObject dialogueUi;

    [SerializeField] protected ParticleSystem dialogueAvailable;
    [SerializeField] protected ParticleSystem speechEffect;

    /// <summary>
    /// an int used to make sure the text is only loaded once not every frame.
    /// </summary>
    [SerializeField] protected int currentParagraphIncrement;

    #region Text
    [SerializeField] protected TextAsset textFile;
    [SerializeField] protected string[] textLines;
    [SerializeField] protected int currentParagraph;
    protected Text speechText;

    [SerializeField] protected bool contentAvailable = true;
    #endregion

    protected virtual void Start () {

        cam = CameraController.instance;
        dialogueUi = StoryManager.instance.MyDialogueUi;
        speechText = StoryManager.instance.MySpeechText;

        if (textFile != null)
        {
            textLines = (textFile.text.Split('#'));
        }
    }

    public override void Interact()
    {
        base.Interact();

        MakeNPCFacePlayer();

        PlayerController.instance.anim.SetFloat("VelocityX", 0);
        PlayerController.instance.anim.SetFloat("VelocityY", 0);

        StoryManager.instance.MyCurrentDialogueNpc = this;

        currentlyInteractingWithPlayer = true;

        PlayerController.instance.dialogue = true;
       
        GameDetails.instance.paused = true;

        float distanceFromLeftToPlayer = Vector3.Distance(cam.measurementTransform.position, PlayerController.instance.transform.position);
        float distanceFromLeftToNPC = Vector3.Distance(cam.measurementTransform.position, transform.position);

        if (distanceFromLeftToPlayer > distanceFromLeftToNPC)
        {
            GameDetails.instance.dialogueCamera.transform.localPosition = GameDetails.instance.dialogueNPCIsStandingOnTheLeft;
            dialogueUi.SetActive(true);
            StoryManager.instance.MySpeechBubble.sprite = StoryManager.instance.MyBubbleRight;
        }

        else if (distanceFromLeftToPlayer < distanceFromLeftToNPC)
        {
            GameDetails.instance.dialogueCamera.transform.localPosition = GameDetails.instance.dialogueNPCIsStandingOnTheRight;
            StoryManager.instance.MySpeechBubble.sprite = StoryManager.instance.MyBubbleLeft;

            dialogueUi.SetActive(true);
        }

        else
        {
            Debug.LogWarning("something wrong with camera positions in dialogue, distances are being measured incorrectly");
        }

        GameDetails.instance.dialogueCamera.SetActive(!GameDetails.instance.dialogueCamera.activeSelf);


        if (dialogueAvailable != null)
        {
            dialogueAvailable.Stop();
        }

        if (speechEffect != null)
        {
            speechEffect.Play();
        }

    }

    protected void Flip()
    {
        Vector3 theScale = transform.localScale;
        Vector3 speechScale = speechEffect.transform.localScale;

        theScale.x *= -1;
        speechScale.x *= -1;

        transform.localScale = theScale;
        speechEffect.transform.localScale = speechScale;

    }

    protected void MakeNPCFacePlayer()
    {
        Vector2 playerPos = PlayerController.instance.transform.position;

        float frontPos = Vector3.Distance(front.transform.position, playerPos);
        float backPos = Vector3.Distance(back.transform.position, playerPos);

        if (frontPos > backPos)
        {
            Flip();
        }
    }

    protected virtual void CloseDialogue()
    {
        GameDetails.instance.dialogueCamera.SetActive(!GameDetails.instance.dialogueCamera.activeSelf);
        dialogueUi.SetActive(false);
        PlayerController.instance.dialogue = false;
        GameDetails.instance.paused = false;
        currentlyInteractingWithPlayer = false;
        StoryManager.instance.MyCurrentDialogueNpc = null;
    }

    /// <summary>
    /// Advances the speech in the speech bubble to the next section
    /// This function is a method called by the UI when pressing the "continue" button
    /// It will call this function on the dialogueManagers currentDialogueNPC
    /// </summary>
    public virtual void AdvanceSpeech()
    {
        if (currentParagraph < textLines.Length - 1)
        {
            currentParagraph += 1;
        }

        // else if so it issues the if statement and leaves the text up until button is pressed again
        else if (currentParagraph == textLines.Length - 1)
        {
            CloseDialogue();
            contentAvailable = false;
        }
    }

    /// <summary>
    /// Closes the dialogue Window and sets the 
    /// dialogueManagers currentDialogueNPC to null as well as the currentParagraph to 0
    /// This function is a method called by the UI when pressing the "Stop"/"X" button
    /// It will call this function on the dialogueManagers currentDialogueNPC
    /// </summary>
    public virtual void StopDialoguePrematurely()
    {
        CloseDialogue();
        currentParagraph = 0;
        currentParagraphIncrement = 0;
    }

}
