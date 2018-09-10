using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretWall : Interactable {

    [SerializeField] bool open;
    [SerializeField] Animator anim;
    [SerializeField] bool mouseOver;
    [SerializeField] BoxCollider2D col;

    public SecretRoom myRoom;

	void Start ()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
	}
	
	void Update ()
    {
        if (mouseOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!hasInteracted)
                {
                    Interact();
                    hasInteracted = true;
                }
            }
        }
	}

    public override void Interact()
    {
        OpenDoor();
    }

    void OpenDoor()
    {
        anim.SetTrigger("open");
        SoundManager.instance.PlayEnvironmentSound("stone_wall_open");

        StartCoroutine(RemoveCollider());

        if (!myRoom.fading)
        {
            SoundManager.instance.PlayEnvironmentSound("secretDoorSfx");
            open = true;
            myRoom.fading = true;
            StartCoroutine(myRoom.FadeCover());
        }

    }

    public override void OnMouseOver()
    {
        base.OnMouseOver();

        if (!mouseOver)
        {
            mouseOver = true;
        }
    }

    public override void OnMouseExit()
    {
        base.OnMouseExit();

        mouseOver = false;
    }

    IEnumerator RemoveCollider()
    {
        yield return new WaitForSeconds(0.5f);
        col.enabled = false;
    }

    //IEnumerator UnFadeCover()
    //{
    //    myRoom.secretCoverTiles.Shuffle();

    //    foreach (GameObject secretCover in myRoom.secretCoverTiles)
    //    {
    //        secretCover.AddComponent<Fade>();
    //        yield return new WaitForSeconds(0.03f);
    //    }


    //    foreach (GameObject torch in myRoom.secretCornerTorches)
    //    {
    //        torch.transform.Find("Flame").GetComponent<ParticleSystem>().Play();
    //        SoundManager.instance.PlayEnvironmentSound("ignite_target");
    //        yield return new WaitForSeconds(0.5f);
    //    }
    //}
}
