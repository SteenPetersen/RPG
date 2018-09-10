using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance;
    [HideInInspector]
    public AudioClip impact1, impact2, impact3, impact4, impact5, impactWood, impactStone, bow,
                     bladeSwing, impBossHit, impBossDeath, impSwing, impHit, impHit1, impDeath, impDeath1, playerHurt, playerHurt1,
                     impactHit, impactHit1, fireballimpact, fireballimpact1, fireballimpact2, fireballimpact3, firstBossPrepAoe, bigGreenFireball, bigGreenFireball1, bigGreenFireball2, bigGreenFireball3,
                     potionInteract, potionPickup, meleePickup, rangedPickup, bookPickup, keyPickup, fireBurst, firebuildup,
                     levelup, deathsound, bomb, shieldblock, shieldriposte, crit,
                     lootdrop, lootAppearsChest, purchase, chestopen, chestclose,
                     demontalk1, demontalk2, demontalk3,
                     spikeTrapFire, spikeTrapReset,
                     stoneWallOpen, secretRoomSfx,
                     impactBox, destroyBox, igniteTarget, cannonShoot,
                     portalAppears, enterPortal, portIn,
                     firstBossDeath,
                     dungeonEnv;
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

    void OnEnable()
    {
        audioSrc = GetComponent<AudioSource>();

        // subscribe from the scenemanager.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start () {
        playerHurt = Resources.Load<AudioClip>("Sound/" + "player_hit");
        playerHurt1 = Resources.Load<AudioClip>("Sound/" + "player_hit1");

        impact1 = Resources.Load<AudioClip>("Sound/" + "impact1");
        igniteTarget = Resources.Load<AudioClip>("Sound/" + "ignite_target");

        impactHit = Resources.Load<AudioClip>("Sound/" + "impact4");
        impactHit1 = Resources.Load<AudioClip>("Sound/" + "impact2");
        shieldblock = Resources.Load<AudioClip>("Sound/" + "shieldblock");
        shieldriposte = Resources.Load<AudioClip>("Sound/" + "shieldriposte");
        fireballimpact = Resources.Load<AudioClip>("Sound/" + "Imp_Fireball_impact");
        fireballimpact1 = Resources.Load<AudioClip>("Sound/" + "Imp_Fireball_impact1");
        fireballimpact2 = Resources.Load<AudioClip>("Sound/" + "Imp_Fireball_impact2");
        fireballimpact3 = Resources.Load<AudioClip>("Sound/" + "Imp_Fireball_impact3");
        bigGreenFireball = Resources.Load<AudioClip>("Sound/" + "big_green_fireball");
        bigGreenFireball1 = Resources.Load<AudioClip>("Sound/" + "big_green_fireball_1");
        bigGreenFireball2 = Resources.Load<AudioClip>("Sound/" + "big_green_fireball_2");
        bigGreenFireball3 = Resources.Load<AudioClip>("Sound/" + "big_green_fireball_3");
        firstBossPrepAoe = Resources.Load<AudioClip>("Sound/" + "first_boss_prep_aoe");
        bomb = Resources.Load<AudioClip>("Sound/" + "bomb");

        crit = Resources.Load<AudioClip>("Sound/" + "crit");

        impactWood = Resources.Load<AudioClip>("Sound/" + "impact_wood");
        impactStone = Resources.Load<AudioClip>("Sound/" + "impact_stone");
        bow = Resources.Load<AudioClip>("Sound/" + "bow");
        bladeSwing = Resources.Load<AudioClip>("Sound/" + "blade_swing");

        impSwing = Resources.Load<AudioClip>("Sound/" + "imp_swing");
        impHit = Resources.Load<AudioClip>("Sound/" + "short_hit_monster_imp");
        impDeath = Resources.Load<AudioClip>("Sound/" + "short_death_monster_imp");
        impHit1 = Resources.Load<AudioClip>("Sound/" + "short_hit_monster_imp_1");
        impDeath1 = Resources.Load<AudioClip>("Sound/" + "short_death_monster_imp_1");


        demontalk1 = Resources.Load<AudioClip>("Sound/" + "demontalk1");
        demontalk2 = Resources.Load<AudioClip>("Sound/" + "demontalk2");
        demontalk3 = Resources.Load<AudioClip>("Sound/" + "demontalk3");
        impBossHit = Resources.Load<AudioClip>("Sound/" + "imp_boss_hit");
        impBossDeath = Resources.Load<AudioClip>("Sound/" + "imp_boss_death");
        fireBurst = Resources.Load<AudioClip>("Sound/" + "fire_burst");
        firebuildup = Resources.Load<AudioClip>("Sound/" + "fire_buildup");

        potionInteract = Resources.Load<AudioClip>("Sound/" + "potion_interact");
        potionPickup = Resources.Load<AudioClip>("Sound/" + "potion_pickup");
        meleePickup = Resources.Load<AudioClip>("Sound/" + "pickup_melee");
        rangedPickup = Resources.Load<AudioClip>("Sound/" + "pickup_wood");
        bookPickup = Resources.Load<AudioClip>("Sound/" + "pickup_book");
        keyPickup = Resources.Load<AudioClip>("Sound/" + "pickup_key");

        firstBossDeath = Resources.Load<AudioClip>("Sound/" + "first_boss_death");

        // UI
        levelup = Resources.Load<AudioClip>("Sound/" + "levelup");
        deathsound = Resources.Load<AudioClip>("Sound/" + "deathsound");

        lootdrop = Resources.Load<AudioClip>("Sound/" + "lootdrop");
        lootAppearsChest = Resources.Load<AudioClip>("Sound/" + "loot_appears_chest");
        purchase = Resources.Load<AudioClip>("Sound/" + "purchase");
        chestopen = Resources.Load<AudioClip>("Sound/" + "chestopen");
        chestclose = Resources.Load<AudioClip>("Sound/" + "chestclose");

        // environment
        spikeTrapFire = Resources.Load<AudioClip>("Sound/" + "spike_trap_fire");
        spikeTrapReset = Resources.Load<AudioClip>("Sound/" + "spike_trap_reset");
        impactBox = Resources.Load<AudioClip>("Sound/" + "impact_box");
        destroyBox = Resources.Load<AudioClip>("Sound/" + "destroy_box");

        stoneWallOpen = Resources.Load<AudioClip>("Sound/" + "stone_wall_open");
        portalAppears = Resources.Load<AudioClip>("Sound/" + "portal_appears");
        enterPortal = Resources.Load<AudioClip>("Sound/" + "enter_portal");
        portIn = Resources.Load<AudioClip>("Sound/" + "port_in");
        cannonShoot = Resources.Load<AudioClip>("Sound/" + "cannon_fire");

        secretRoomSfx = Resources.Load<AudioClip>("Sound/" + "secret_appear_sfx");

        dungeonEnv = Resources.Load<AudioClip>("Sound/" + "dungeon_env");

    }

    public void PlayCombatSound(string clip)
    {
        if (GameDetails.soundEffects)
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

                case "first_boss_death":
                    audioSrc.PlayOneShot(firstBossDeath);
                    break;

                case "shieldblock":
                    audioSrc.PlayOneShot(shieldblock);
                    break;

                case "shieldriposte":
                    audioSrc.PlayOneShot(shieldriposte);
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

                case "Imp_Fireball_impact":
                case "Imp_Fireball(Clone)_impact":

                    int randHitFire = Random.Range(1, 5);
                    if (randHitFire == 1)
                    {
                        audioSrc.PlayOneShot(fireballimpact);
                        break;
                    }
                    else if (randHitFire == 2)
                    {
                        audioSrc.PlayOneShot(fireballimpact1);
                        break;
                    }
                    else if (randHitFire == 3)
                    {
                        audioSrc.PlayOneShot(fireballimpact2);
                        break;
                    }
                    else if (randHitFire == 4)
                    {
                        audioSrc.PlayOneShot(fireballimpact3);
                        break;
                    }
                    audioSrc.PlayOneShot(fireballimpact1);
                    break;

                case "Imp_Fireball_wallimpact":
                case "Imp_Fireball(Clone)_wallimpact":
                    int randHitFireWall = Random.Range(1, 5);
                    if (randHitFireWall == 1)
                    {
                        audioSrc.PlayOneShot(fireballimpact);
                        break;
                    }
                    else if (randHitFireWall == 2)
                    {
                        audioSrc.PlayOneShot(fireballimpact1);
                        break;
                    }
                    else if (randHitFireWall == 3)
                    {
                        audioSrc.PlayOneShot(fireballimpact2);
                        break;
                    }
                    else if (randHitFireWall == 4)
                    {
                        audioSrc.PlayOneShot(fireballimpact3);
                        break;
                    }
                    audioSrc.PlayOneShot(fireballimpact1);
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

                case "FirstBoss_shot":
                case "FirstBoss(Clone)_shot":
                    int randGreenFireball = Random.Range(1, 5);
                    if (randGreenFireball == 1)
                    {
                        audioSrc.PlayOneShot(bigGreenFireball);
                        break;
                    }
                    else if (randGreenFireball == 2)
                    {
                        audioSrc.PlayOneShot(bigGreenFireball1);
                        break;
                    }
                    else if (randGreenFireball == 3)
                    {
                        audioSrc.PlayOneShot(bigGreenFireball2);
                        break;
                    }
                    audioSrc.PlayOneShot(bigGreenFireball3);
                    break;

                case "FirstBoss_prep":
                case "FirstBoss(Clone)_prep":
                    audioSrc.PlayOneShot(firstBossPrepAoe);
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

                case "bomb":
                    audioSrc.PlayOneShot(bomb);
                    break;

                case "crit":
                    audioSrc.PlayOneShot(crit);
                    break;

            }
        }
    }

    public void PlayInventorySound(string clip)
    {
        if (GameDetails.soundEffects)
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

                case "Potion_pickup":
                case "Potion(Clone)_pickup":
                    audioSrc.PlayOneShot(potionInteract);
                    break;

                case "Melee_pickup":
                case "Melee(Clone)_pickup":
                    audioSrc.PlayOneShot(meleePickup);
                    break;

                case "Ranged_pickup":
                case "Ranged(Clone)_pickup":
                    audioSrc.PlayOneShot(rangedPickup);
                    break;

                case "Armor_pickup":
                case "Armor(Clone)_pickup":
                    audioSrc.PlayOneShot(meleePickup);
                    break;

                case "Key_pickup":
                case "Key(Clone)_pickup":
                    audioSrc.PlayOneShot(keyPickup);
                    break;

                case "SpellBook_pickup":
                case "SpellBook(Clone)_pickup":
                    audioSrc.PlayOneShot(bookPickup);
                    break;

            }

        }
    }

    public void PlayUiSound(string clip)
    {
        if (GameDetails.soundEffects)
        {
            switch (clip)
            {
                case "levelup":
                    audioSrc.PlayOneShot(levelup);
                    break;

                case "deathsound":
                    audioSrc.PlayOneShot(deathsound);
                    break;

                case "lootdrop":
                    audioSrc.PlayOneShot(lootdrop);
                    break;

                case "lootAppearsChest":
                    audioSrc.PlayOneShot(lootAppearsChest);
                    break;

                case "purchase":
                    audioSrc.PlayOneShot(purchase);
                    break;

                case "chestopen":
                    audioSrc.PlayOneShot(chestopen);
                    break;

                case "chestclose":
                    audioSrc.PlayOneShot(chestclose);
                    break;


            }

        }
    }

    public void PlayDialogueSound(string clip)
    {
        if (GameDetails.soundEffects)
        {
            switch (clip)
            {
                case "ImpGiant":
                case "ImpGiant(Clone)":
                    int rnd = UnityEngine.Random.Range(0, 2);
                    switch (rnd)
                    {
                        case 0:
                            audioSrc.PlayOneShot(demontalk1);
                            break;
                        case 1:
                            audioSrc.PlayOneShot(demontalk2);
                            break;
                        case 2:
                            audioSrc.PlayOneShot(demontalk3);
                            break;
                    }
                    break;
            }
        }
    }

    public void PlayEnvironmentSound(string clip)
    {
        if (GameDetails.soundEffects)
        {
            switch (clip)
        {
            case "spike_trap_fire":
                //int rand = Random.Range(1, 3);
                //if (rand == 1)
                //{
                    //audioSrc.PlayOneShot(playerHurt);
                    //break;
                //}
                audioSrc.PlayOneShot(spikeTrapFire);
                break;

            case "spike_trap_reset":
                audioSrc.PlayOneShot(spikeTrapReset);
                break;

            case "stone_wall_open":
                audioSrc.PlayOneShot(stoneWallOpen);
                break;


            case "impact_box":
                audioSrc.PlayOneShot(impactBox);
                break;


            case "destroy_box":
                audioSrc.PlayOneShot(destroyBox);
                break;

            case "portal_appears":
                audioSrc.PlayOneShot(portalAppears);
                break;

            case "enter_portal":
                audioSrc.PlayOneShot(enterPortal);
                break;

            case "port_in":
                audioSrc.PlayOneShot(portIn);
                break;

            case "ignite_target":
                audioSrc.PlayOneShot(igniteTarget);
                break;

            case "secretDoorSfx":
                audioSrc.PlayOneShot(secretRoomSfx);
                break;

            case "cannon_shoot":
                audioSrc.PlayOneShot(cannonShoot);
                break;

            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Caves_dungeon_indoor":
                if (audioSrc.clip != dungeonEnv)
                {
                    audioSrc.clip = dungeonEnv;
                    audioSrc.Play();
                }

                break;

            default:
                audioSrc.clip = null;
                break;
        }

    }

    private void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
