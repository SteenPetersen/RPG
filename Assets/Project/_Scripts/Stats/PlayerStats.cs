﻿using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;
using System.Collections;
using System;

public class PlayerStats : CharacterStats {

    public Animator anim;
    public PlayerController playerControl;

    float maxStamina;
    float currentStamina;

    public float exp;

    [SerializeField] float regen;
    [SerializeField] float staminaRegen;

    /// <summary>
    /// used to stop regen when holding some sort of charge
    /// </summary>
    public bool chargeHeld;
    public bool sprinting;
    public bool cantRegen;

    /// <summary>
    /// True when players stamina bar is full
    /// </summary>
    public bool staminaFull;


    public Stat Agi;
    public Stat Str;
    public Stat Sta;

    // UI display
    public Text maxhps;
    public Text hps;
    public Text dmg;
    public Text ac;
    public Text stmina;
    public Text strn;
    public Text agil;
    public Text lvl;

    GameObject statHolder;

    public static PlayerStats instance;

    public float MyRegen
    {
        get { return regen; }
        set { regen = value; }
    }

    public float MyCurrentStamina
    {
        get { return currentStamina; }

        set { currentStamina = Mathf.Clamp(value, 0, MyMaxStamina); }
    }

    public float MyMaxStamina
    {
        get { return maxStamina; }
    }

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
        EquipmentManager.instance.onEquipmentChanged += OnEquipmentChanged;
        anim = GetComponent<Animator>();
        playerControl = PlayerController.instance;

        UpdateStats();
        MyCurrentHealth = MyMaxHealth;
        currentStamina = maxStamina;


        statHolder = GameObject.Find("UiCanvas").transform.Find("EquipmentWindow").transform.Find("Stats").gameObject;
        maxhps = statHolder.transform.Find("Maxhps").GetComponent<Text>();
        hps = statHolder.transform.Find("Hps").GetComponent<Text>();
        dmg = statHolder.transform.Find("Damage").GetComponent<Text>();
        ac = statHolder.transform.Find("Armor").GetComponent<Text>();
        stmina = statHolder.transform.Find("Stamina").GetComponent<Text>();
        strn = statHolder.transform.Find("Strength").GetComponent<Text>();
        agil = statHolder.transform.Find("Agility").GetComponent<Text>();
        lvl = statHolder.transform.Find("Level").GetComponent<Text>();

        UpdateUiStats();
    }

    private void Update()
    {
        if (Time.frameCount % 3 == 0)
        {
            if (playerControl == null)
            {
                playerControl = PlayerController.instance;
            }

            hps.text = "Health: " + MyCurrentHealth.ToString();


            if (staminaFull && MyCurrentStamina < MyMaxStamina || !staminaFull && MyCurrentStamina >= MyMaxStamina)
            {
                staminaFull = !staminaFull;
            }
        }
    }

    /// <summary>
    /// Method Listening to the ONequipment changed method on inventoryscript
    /// In order to correctly remove or add modifiers that gear may have
    /// </summary>
    /// <param name="newItem"></param>
    /// <param name="oldItem"></param>
    private void OnEquipmentChanged(Equipment newItem, Equipment oldItem)
    {
        if (oldItem != null)
        {
            armor.RemoveModifier(oldItem.armorModifier);
            damage.RemoveModifier(oldItem.damageModifier);
            Sta.RemoveModifier(oldItem.sta);
            Str.RemoveModifier(oldItem.str);
            Agi.RemoveModifier(oldItem.agi);
        }

        if (newItem != null)
        {
            armor.AddModifier(newItem.armorModifier);
            damage.AddModifier(newItem.damageModifier);
            Sta.AddModifier(newItem.sta);
            Str.AddModifier(newItem.str);
            Agi.AddModifier(newItem.agi);
        }

        UpdateUiStats();

    }

    /// <summary>
    /// Mainly used for unequipping the shield
    /// </summary>
    /// <param name="oldItem"></param>
    public void RemoveItemStats(Equipment oldItem)
    {
        armor.RemoveModifier(oldItem.armorModifier);
        damage.RemoveModifier(oldItem.damageModifier);
        Sta.RemoveModifier(oldItem.sta);
        Str.RemoveModifier(oldItem.str);
        Agi.RemoveModifier(oldItem.agi);
    }

    /// <summary>
    /// Updates the Ui with the text corresponding to 
    /// the players current stats. done in the delegate
    /// of changing gear and at start
    /// </summary>
    private void UpdateUiStats()
    {
        maxhps.text = "Max health: " + MyMaxHealth.ToString();
        dmg.text = "Damage: " + damage.GetValue().ToString();
        ac.text = "Armor: " + armor.GetValue().ToString();
        stmina.text = "Stamina: " + Sta.GetValue().ToString();
        strn.text = "Strength: " + Str.GetValue().ToString();
        agil.text = "Agilty: " + Agi.GetValue().ToString();
        lvl.text = "Level: " + ExperienceManager.MyLevel.ToString();
    }

    public override void Die()
    {
        base.Die();

        // kill the player in some way
        if (playerControl.blocking)
        {
            anim.SetBool("Block", false);
        }

        playerControl.speed = 0;
        playerControl.isDead = true;
        SoundManager.instance.PlayUiSound("deathsound");
        anim.SetTrigger("Dead");
        GameDetails.instance.KillPlayer();
    }

    public override void TakeDamage(int damage)
    {
        int newDmg;
        bool crit;

        DamageVariance(damage, out crit, out newDmg);

        MyCurrentHealth -= newDmg;

        if (MyCurrentHealth <= 0)
        {
            Die();
        }

        // if player is NOT blocking
        if (!playerControl.blocking)
        {
            SoundManager.instance.PlayCombatSound("player_hurt");

            // careful changing values here it feels correct
            CameraShaker.Instance.ShakeOnce(0.5f, 3, 0.1f, 0.5f);

            var text = CombatTextManager.instance.FetchText(transform.position);
            var textScript = text.GetComponent<CombatText>();
            textScript.Red(newDmg.ToString(), transform.position);
            text.transform.position = transform.position;
            text.gameObject.SetActive(true);
        }

        // if player is blocking
        if (playerControl.blocking)
        {
            // If the player timed the block well
            if (playerControl.timedBlock)
            {
                playerControl.StartCoroutine(playerControl.Riposte());
            }


            // play blocking sound
            SoundManager.instance.PlayCombatSound("shieldblock");
            playerControl.KnockBack(0.005f);
            StartCoroutine(returnMovement());
            GameDetails.blocks += 1;

            var blockText = CombatTextManager.instance.FetchText(transform.position);
            var blockTextScript = blockText.GetComponent<CombatText>();
            blockTextScript.Gray("-" + newDmg.ToString() + " block", transform.position);
            blockText.transform.position = transform.position;
            blockText.gameObject.SetActive(true);
        }

        playerControl.healthBar.fillAmount = CalculateHealth(MyCurrentHealth, MyMaxHealth);
    }

    public override bool Heal(int healthIncrease)
    {
        bool tmp = base.Heal(healthIncrease);

        playerControl.healthBar.fillAmount = CalculateHealth(MyCurrentHealth, MyMaxHealth);

        return tmp;
    }

    public void Regen()
    {
        // health
        MyCurrentHealth += regen;
        playerControl.healthBar.fillAmount = CalculateHealth(MyCurrentHealth, MyMaxHealth);

        // stamina
        if (!chargeHeld && !sprinting && !cantRegen)
        {
            MyCurrentStamina += staminaRegen;
            LerpStaminaBar();
        }
    }

    public void LevelUpStats()
    {
        Agi.SetValue(ExperienceManager.MyLevel);
        Str.SetValue(ExperienceManager.MyLevel);
        Sta.SetValue(ExperienceManager.MyLevel);

        UpdateStats();
        UpdateUiStats();

        MyCurrentHealth = MyMaxHealth;

        playerControl.healthBar.fillAmount = CalculateHealth(MyCurrentHealth, MyMaxHealth);
    }

    /// <summary>
    /// Updates the stats, used at Start() and when leveling up
    /// </summary>
    public void UpdateStats()
    {
        MyMaxHealth = 100 + ExperienceManager.MyLevel + (Sta.GetValue() * 10);
        maxStamina = 100;
        damage.SetValue(Str.GetValue());
        armor.SetValue(Agi.GetValue());
    }

    /// <summary>
    /// returns the float value of division between maximum health and current health
    /// This value will always be a value between 0 and 1
    /// </summary>
    /// <param name="currentHealth"></param>
    /// <param name="maxHealth"></param>
    /// <returns></returns>
    public float CalculateHealth(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// returns the float value of division between maximum Stamina and current Stamina
    /// This value will always be a value between 0 and 1
    /// </summary>
    /// <param name="currentStamina"></param>
    /// <param name="maxStamina"></param>
    /// <returns></returns>
    public float CalculateStamina(float currentStamina, float maxStamina)
    {
        return currentStamina / maxStamina;
    }

    /// <summary>
    /// Lerps the stamina Bar between its before cost and after values
    /// </summary>
    public void LerpStaminaBar()
    {
        playerControl.staminaBar.fillAmount = Mathf.Lerp(playerControl.staminaBar.fillAmount,
        CalculateStamina(MyCurrentStamina, MyMaxStamina),
        Time.deltaTime * 8);
    }

    private void OnDisable()
    {
        EquipmentManager.instance.onEquipmentChanged -= OnEquipmentChanged;
    }

    // amount of time player is interupted when being hit by a boss projectile
    IEnumerator returnMovement()
    {
        yield return new WaitForSeconds(0.2f);

        PlayerController.instance.interruptMovement = false;
    }
}
