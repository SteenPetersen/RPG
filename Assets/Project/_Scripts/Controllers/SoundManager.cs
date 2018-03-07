using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance;
    [HideInInspector]
    public AudioClip impact1, impact2, impact3, impact4, impact5, impactWood, impactStone, bow, 
                     bladeSwing, impBossHit, impBossDeath, impSwing, impHit, impDeath, playerHurt, playerHurt1,
                     impactHit, impactHit1;
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
        playerHurt = Resources.Load<AudioClip>("Sound/" + "player_hit");
        playerHurt1 = Resources.Load<AudioClip>("Sound/" + "player_hit1");

        impact1 = Resources.Load<AudioClip>("Sound/" + "impact1");

        impactHit = Resources.Load<AudioClip>("Sound/" + "impact4");
        impactHit1 = Resources.Load<AudioClip>("Sound/" + "impact2");



        impactWood = Resources.Load<AudioClip>("Sound/" + "impact_wood");
        impactStone = Resources.Load<AudioClip>("Sound/" + "impact_stone");
        bow = Resources.Load<AudioClip>("Sound/" + "bow");
        bladeSwing = Resources.Load<AudioClip>("Sound/" + "blade_swing");

        impSwing = Resources.Load<AudioClip>("Sound/" + "imp_swing");
        impHit = Resources.Load<AudioClip>("Sound/" + "short_hit_monster_imp");
        impDeath = Resources.Load<AudioClip>("Sound/" + "short_death_monster_imp");


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
            case "player_hurt":
                int rand = Random.Range(1, 3);
                if (rand == 1)
                {
                    audioSrc.PlayOneShot(playerHurt);
                    break;
                }
                audioSrc.PlayOneShot(playerHurt1);
                break;

            case "hit_wall":
                audioSrc.PlayOneShot(impactStone);
                break;

            case "impact":
                audioSrc.PlayOneShot(impact1);
                break;

            case "impact_wood":
                audioSrc.PlayOneShot(impactWood);
                break;

            case "impact_stone":
                audioSrc.PlayOneShot(impactStone);
                break;

            case "impact_hit":
                int randHit = Random.Range(1, 3);
                if (randHit == 1)
                {
                    audioSrc.PlayOneShot(impactHit);
                    break;
                }
                audioSrc.PlayOneShot(impactHit1);
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

            case "Imp_swing":
            case "Imp(Clone)_swing":
                audioSrc.PlayOneShot(impSwing);
                break;

            case "Imp_hit":
            case "Imp(Clone)_hit":
                audioSrc.PlayOneShot(impHit);
                break;

            case "Imp_death":
            case "Imp(Clone)_death":
                audioSrc.PlayOneShot(impDeath);
                break;


        }
    }
}
