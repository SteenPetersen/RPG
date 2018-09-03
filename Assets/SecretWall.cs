using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretWall : Interactable {

    [SerializeField] bool open;
    [SerializeField] Animator anim;
    [SerializeField] bool mouseOver;

    public Room myRoom;
    [SerializeField] bool faded;

	void Start ()
    {
        anim = GetComponent<Animator>();
	}
	
	void Update ()
    {
        if (mouseOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Interact();
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

        if (!faded)
        {
            StartCoroutine(UnFadeCover());



            open = true;
            faded = true;
        }

    }

    void OnMouseOver()
    {
        Debug.Log("Mouse over secret wall");
        mouseOver = true;
    }

    void OnMouseExit()
    {
        Debug.Log("Mouse Off secret wall");
        mouseOver = false;
    }

    IEnumerator UnFadeCover()
    {
        myRoom.secretCoverTiles.Shuffle();

        foreach (GameObject secretCover in myRoom.secretCoverTiles)
        {
            secretCover.AddComponent<Fade>();
            yield return new WaitForSeconds(0.03f);
        }

        foreach (GameObject torch in myRoom.secretCornerTorches)
        {
            torch.transform.Find("Flame").GetComponent<ParticleSystem>().Play();
            SoundManager.instance.PlayEnvironmentSound("ignite_target");
            yield return new WaitForSeconds(0.5f);
        }
    }
}
