using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance;
    [HideInInspector]
    public AudioClip impact1, impact2, impact3, impact4, impact5, impactWood, impactStone, bow, 
                     bladeSwing, impBossHit, impBossDeath, impSwing, impHit, impHit1, impDeath, impDeath1, playerHurt, playerHurt1,
                     impactHit, impactHit1, potionInteract, potionPickup, fireBurst, firebuildup,
                     levelup, deathsound;
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
        impHit1 = Resources.Load<AudioClip>("Sound/" + "short_hit_monster_imp_1");
        impDeath1 = Resources.Load<AudioClip>("Sound/" + "short_death_monster_imp_1");


        impBossHit = Resources.Load<AudioClip>("Sound/" + "imp_boss_hit");
        impBossDeath = Resources.Load<AudioClip>("Sound/" + "imp_boss_death");
        fireBurst = Resources.Load<AudioClip>("Sound/" + "fire_burst");
        firebuildup = Resources.Load<AudioClip>("Sound/" + "fire_buildup");

        potionInteract = Resources.Load<AudioClip>("Sound/" + "potion_interact");
        potionPickup = Resources.Load<AudioClip>("Sound/" + "potion_pickup");

        // UI
        levelup = Resources.Load<AudioClip>("Sound/" + "levelup");
        deathsound = Resources.Load<AudioClip>("Sound/" + "deathsound");

        audioSrc = GetComponent<AudioSource>();
    }
	
	void Update () {
		
	}

    public void PlayCombatSound(string clip)
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


            case "ImpGiant_sound1":
            case "ImpGiant(Clone)_sound1":
                audioSrc.PlayOneShot(fireBurst);
                break;

            case "ImpGiant_sound2":
            case "ImpGiant(Clone)_sound2":
                audioSrc.PlayOneShot(firebuildup);
                break;

            case "Imp_swing":
            case "Imp(Clone)_swing":
                audioSrc.PlayOneShot(impSwing);
                break;

            case "Imp_hit":
            case "Imp(Clone)_hit":
                int Imp_hit = Random.Range(1, 3);
                if (Imp_hit == 1)
                {
                    audioSrc.PlayOneShot(impHit);
                    break;
                }
                audioSrc.PlayOneShot(impHit1);
                break;

            case "Imp_death":
            case "Imp(Clone)_death":
                audioSrc.PlayOneShot(impDeath);
                break;

            case "ImpRanged_hit":
            case "ImpRanged(Clone)_hit":
                int ImpRanged_hit = Random.Range(1, 3);
                if (ImpRanged_hit == 1)
                {
                    audioSrc.PlayOneShot(impHit);
                    break;
                }
                audioSrc.PlayOneShot(impHit1);
                break;

            case "ImpRanged_death":
            case "ImpRanged(Clone)_death":
                audioSrc.PlayOneShot(impDeath);
                break;


        }
    }

    public void PlayInventorySound(string clip)
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

            case "AddItem":
                audioSrc.PlayOneShot(impactStone);
                break;

            case "gulp":
                audioSrc.PlayOneShot(potionPickup);
                break;

            case "Small Health Potion_pickup":
            case "Small Health Potion(Clone)_pickup":
                audioSrc.PlayOneShot(potionInteract);
                break;

        }
    }

    public void PlayUiSound(string clip)
    {
        switch (clip)
        {
            case "levelup":
                audioSrc.PlayOneShot(levelup);
                break;

            case "deathsound":
                audioSrc.PlayOneShot(deathsound);
                break;


        }
    }
}
