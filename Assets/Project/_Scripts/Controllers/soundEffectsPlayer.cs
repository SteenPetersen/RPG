using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundEffectsPlayer : MonoBehaviour {


    public void Sound1()
    {
        SoundManager.instance.PlayCombatSound(transform.parent.name + "_sound1");
    }
    public void Sound2()
    {
        SoundManager.instance.PlayCombatSound(transform.parent.name + "_sound2");
    }

}
