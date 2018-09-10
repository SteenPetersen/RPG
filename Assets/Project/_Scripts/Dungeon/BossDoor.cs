using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDoor : MonoBehaviour {

    [SerializeField] bool open;

    [Tooltip("Can this door be closed after the player has walked past it?")]
    [SerializeField] bool permanentlyLocked;

    [Tooltip("Doors that should close behind the player when opening these if any")]
    [SerializeField] BossDoor[] doorsToCloseBehind;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (DungeonManager.instance._PlayerHasBossKey)
            {
                if (!open && !permanentlyLocked)
                {
                    GetComponentInParent<Animator>().SetTrigger("open");
                    SoundManager.instance.PlayEnvironmentSound("stone_wall_open");
                    open = true;
                    CloseDoors();
                }
            }
            else
            {
                StoryManager.instance.NotifyPlayer("You do not have the Key!");
            }

        }

    }

    public void DisableCube()
    {
        gameObject.SetActive(false);
    }

    void CloseDoors()
    {
        foreach (BossDoor door in doorsToCloseBehind)
        {
            if (door.open)
            {
                door.CloseDoor();
                door.open = false;
                door.permanentlyLocked = true;
            }
        }
    }

    public void CloseDoor()
    {
        SoundManager.instance.PlayEnvironmentSound("stone_wall_open");
        GetComponentInParent<Animator>().SetTrigger("close");
    }
}
