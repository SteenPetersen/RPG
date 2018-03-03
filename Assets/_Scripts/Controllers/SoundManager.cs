using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance;
    [HideInInspector]
    public AudioClip impact1, impact2, impact3, impact4, impact5, impactWood, impactStone, bow, bladeSwing, impBossHit, impBossDeath;
    public AudioSource audioSrc;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    void Start () {
        impact1 = Resources.Load<AudioClip>("Sound/" + "impact1");
        impactWood = Resources.Load<AudioClip>("Sound/" + "impact_wood");
        impactStone = Resources.Load<AudioClip>("Sound/" + "impact_stone");
        bow = Resources.Load<AudioClip>("Sound/" + "bow");
        bladeSwing = Resources.Load<AudioClip>("Sound/" + "bladeSwing");

        impBossHit = Resources.Load<AudioClip>("Sound/" + "imp_boss_hit");
        impBossDeath = Resources.Load<AudioClip>("Sound/" + "imp_boss_death");

        audioSrc = GetComponent<AudioSource>();
    }
	
	void Update () {
		
	}

    public void PlaySound(string clip)
    {
        switch (clip)
        {
            case "impact":
                audioSrc.PlayOneShot(impact1);
                break;
            case "impact_wood":
                audioSrc.PlayOneShot(impactWood);
                break;
            case "impact_stone":
                audioSrc.PlayOneShot(impactStone);
                break;
            case "bow":
                audioSrc.PlayOneShot(bow);
                break;
            case "bladeSwing":
                audioSrc.PlayOneShot(bladeSwing);
                break;
            case "Imp_Boss":
            case "Imp_Boss(Clone)":
                audioSrc.PlayOneShot(impBossHit);
                break;
        }
    }
}
