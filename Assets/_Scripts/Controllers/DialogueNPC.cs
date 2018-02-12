using UnityEngine;
using UnityEngine.UI;

public class DialogueNPC : Interactable {

    //[HideInInspector]
    public ParticleSystem dialogueAvailable;
    //public Transform player;
    PlayerController playerControl;
    CameraController cam;

    GameObject front;
    GameObject back;

    public GameObject dialogueProper;

    //[HideInInspector]
    public bool currentlyInteractingWithPlayer;

    #region Text
    public TextAsset textFile;
    public string[] textLines;
    public Text speechText;
    public int currentParagraph;

    public Image speechBubble;
    public Sprite bubbleRight;
    public Sprite bubbleLeft;

    //[HideInInspector]
    public bool contentAvailable = true;
    #endregion

    void Start () {
        dialogueAvailable = GetComponentInChildren<ParticleSystem>();
        playerManager = PlayerManager.instance;
        player = playerManager.player.transform;
        playerControl = PlayerController.instance;
        cam = CameraController.instance;

        front = transform.Find("Logic/Front").gameObject;
        back = transform.Find("Logic/Back").gameObject;

        if (textFile != null)
        {
            textLines = (textFile.text.Split('#'));
        }
    }

    public virtual void InteractWithNPC()
    {
        if (isFocus && !hasInteracted)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (distance <= radius)
            {
                Interact();
            }
            hasInteracted = true;
        }
    }

    public override void Interact()
    {
        base.Interact();

        MakeNPCFacePlayer();
        currentlyInteractingWithPlayer = true;
        dialogueAvailable.Stop();
        playerControl.dialogue = true;
        playerManager.paused = true;

        float distanceFromLeftToPlayer = Vector3.Distance(cam.measurementTransform.position, player.position);
        float distanceFromLeftToNPC = Vector3.Distance(cam.measurementTransform.position, transform.position);

        if (distanceFromLeftToPlayer > distanceFromLeftToNPC)
        {
            playerManager.dialogueCamera.transform.localPosition = playerManager.dialogueLeft;
            dialogueProper.SetActive(true);
            speechBubble.sprite = bubbleRight;
        }

        else if (distanceFromLeftToPlayer < distanceFromLeftToNPC)
        {
            playerManager.dialogueCamera.transform.localPosition = playerManager.dialogueRight;
            speechBubble.sprite = bubbleLeft;

            dialogueProper.SetActive(true);
        }

        else
        {
            Debug.LogWarning("something wrong with camera positions in dialogue, distances are being measured incorrectly");
        }

        playerManager.dialogueCamera.SetActive(!playerManager.dialogueCamera.activeSelf);

    }

    public void Flip()
    {
        Transform tmp = selector.transform;

        Vector3 pos = tmp.position;

        tmp.SetParent(null);

        Vector3 theScale = transform.localScale;

        theScale.x *= -1;

        transform.localScale = theScale;

        tmp.SetParent(transform);
        tmp.position = pos;
    }

    public virtual void StopDialoguePrematurely()
    {
        CloseDialogue();
        currentParagraph = 0;
    }

    public override void AdvanceSpeech()
    {
        base.AdvanceSpeech();

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

    public virtual void CloseDialogue()
    {
        playerManager.dialogueCamera.SetActive(!playerManager.dialogueCamera.activeSelf);
        dialogueProper.SetActive(false);
        playerControl.dialogue = false;
        playerManager.paused = false;
        currentlyInteractingWithPlayer = false;
    }

    public void MakeNPCFacePlayer()
    {
        float frontPos = Vector3.Distance(front.transform.position, player.position);
        float backPos = Vector3.Distance(back.transform.position, player.position);

        if (frontPos > backPos)
        {
            Flip();
        }
    }
}
