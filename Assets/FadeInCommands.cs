using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInCommands : MonoBehaviour {

    [SerializeField] CommandPlayer[] commands;

    public CommandPlayer[] MyCommands
    {
        get
        {
            return commands;
        }

        set
        {
            commands = value;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            foreach (CommandPlayer command in MyCommands)
            {
                StartCoroutine(command.FadeIn());
            }
        }
    }
}
