﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

    public static TutorialManager instance;

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

    [SerializeField] PlayerStats playerStats;
    [SerializeField] bool doneWithMelee;
    [SerializeField] bool overtheLine;

    [SerializeField] CommandPlayer equipSword;
    [SerializeField] CommandPlayer equipShield;
    [SerializeField] CommandPlayer riposte;
    [SerializeField] CommandPlayer block;
    [SerializeField] CommandPlayer chargedHit;
    [SerializeField] CommandPlayer hit;

    [SerializeField] Transform startPosition;

    protected int targetsHit;
    [SerializeField] bool doneWithArchery;
    [SerializeField] Activated_Door[] archeryDoors;
    public bool archeryDoorsOpen;
    public bool finalMonsterDead;

    public CommandPlayer MyRiposte
    {
        get
        {
            return riposte;
        }

        set
        {
            riposte = value;
        }
    }

    public CommandPlayer MyBlock
    {
        get
        {
            return block;
        }

        set
        {
            block = value;
        }
    }

    public CommandPlayer MyChargedHit
    {
        get
        {
            return chargedHit;
        }

        set
        {
            chargedHit = value;
        }
    }

    public CommandPlayer MyHit
    {
        get
        {
            return hit;
        }

        set
        {
            hit = value;
        }
    }

    public bool MyOverTheLine
    {
        get
        {
            return overtheLine;
        }

        set
        {
            overtheLine = value;
        }
    }

    public bool MyDoneWithArchery
    {
        get
        {
            return doneWithArchery;
        }

        set
        {
            doneWithArchery = value;
        }
    }

    public Transform MyStartPosition
    {
        get
        {
            return startPosition;
        }

        set
        {
            startPosition = value;
        }
    }

    void Start ()
    {
        playerStats = PlayerStats.instance;
        //equipSword = GameObject.Find("CommandCanvas/Equip_Sword").GetComponent<CommandPlayer>();
        //equipShield = GameObject.Find("CommandCanvas/Equip_Shield").GetComponent<CommandPlayer>();
        //riposte     = GameObject.Find("CommandCanvas/Riposte").GetComponent<CommandPlayer>();
        //block       = GameObject.Find("CommandCanvas/Block").GetComponent<CommandPlayer>();
        //chargedHit  = GameObject.Find("CommandCanvas/ChargeedHit").GetComponent<CommandPlayer>();
        //hit         = GameObject.Find("CommandCanvas/Hit").GetComponent<CommandPlayer>();
        //startPosition = GameObject.Find("TutorialSpawnInPos").transform;

        PlayerController.instance.transform.position = MyStartPosition.position;
    }
	
	void Update ()
    {
        if (playerStats.MyCurrentHealth < 10)
        {
            playerStats.Heal(100);
        }

        if (!doneWithMelee)
        {
            CheckForMeleeCompletion();
        }
    }

    private void CheckForMeleeCompletion()
    {
        if (GameDetails.ripostes != 0)
        {
            StartCoroutine(MyRiposte.FadeOut());
        }
        if (GameDetails.blocks != 0)
        {
            StartCoroutine(MyBlock.FadeOut());
        }
        if (GameDetails.hits != 0)
        {
            StartCoroutine(MyHit.FadeOut());
        }
        if (GameDetails.fullChargeHits != 0)
        {
            StartCoroutine(MyChargedHit.FadeOut());
        }
        if (EquipmentManager.instance.currentEquipment[4] != null)
        {
            StartCoroutine(equipShield.FadeOut());
        }
        if (EquipmentManager.instance.currentEquipment[3] != null)
        {
            StartCoroutine(equipSword.FadeOut());
        }

        if (GameDetails.ripostes != 0 && GameDetails.blocks != 0 && GameDetails.hits != 0 && GameDetails.fullChargeHits != 0)
        {
            if (MyRiposte.MyAlpha < 0.05f && MyBlock.MyAlpha < 0.05f && MyHit.MyAlpha < 0.05f && MyChargedHit.MyAlpha < 0.05f)
            {
                doneWithMelee = true;
            }
        }
    }

    public void TargetBrickCounter()
    {
        targetsHit += 1;

        if (targetsHit == 3)
        {
            MyDoneWithArchery = true;
            StoryManager.tutorialStage = 2;
        }
    }

    public void OpenArcheryDoors()
    {
        foreach (var door in archeryDoors)
        {
            door.activated = true;
        }

        archeryDoorsOpen = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            MyOverTheLine = true;
        }
    }


    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            MyOverTheLine = false;
        }
    }
}

