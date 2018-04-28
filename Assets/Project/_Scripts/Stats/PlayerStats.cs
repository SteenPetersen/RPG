using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;
using System.Collections;

public class PlayerStats : CharacterStats {

    public Animator anim;
    public PlayerController playerControl;

    public float exp;

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
        currentHealth = maxHealth;


        statHolder = GameObject.Find("UiCanvas").transform.Find("EquipmentWindow").transform.Find("Stats").gameObject;
        maxhps = statHolder.transform.Find("Maxhps").GetComponent<Text>();
        hps = statHolder.transform.Find("Hps").GetComponent<Text>();
        dmg = statHolder.transform.Find("Damage").GetComponent<Text>();
        ac = statHolder.transform.Find("Armor").GetComponent<Text>();
        stmina = statHolder.transform.Find("Stamina").GetComponent<Text>();
        strn = statHolder.transform.Find("Strength").GetComponent<Text>();
        agil = statHolder.transform.Find("Agility").GetComponent<Text>();
        lvl = statHolder.transform.Find("Level").GetComponent<Text>();


    }

    private void Update()
    {
        if (currentHealth == maxHealth)
        {
            RemoveHealthBar();
        }

        if (currentHealth < 0)
        {
            RemoveHealthBar();
        }

        if (playerControl == null)
        {
            playerControl = PlayerController.instance;
        }

        maxhps.text = "Max health: " + maxHealth.ToString();
        hps.text = "Health: " + currentHealth.ToString();
        dmg.text = "Damage: " + damage.GetValue().ToString();
        ac.text = "Armor: " + armor.GetValue().ToString();
        stmina.text = "Stamina: " + Sta.GetValue().ToString();
        strn.text = "Strength: " + Str.GetValue().ToString();
        agil.text = "Agilty: " + Agi.GetValue().ToString();
        lvl.text = "Level: " + ExperienceManager.instance.level.ToString();
    }

    private void OnEquipmentChanged(Equipment newItem, Equipment oldItem)
    {

        if (oldItem != null)
        {
            armor.RemoveModifier(oldItem.armorModifier);
            damage.RemoveModifier(oldItem.damageModifier);
        }

        if (newItem != null)
        {
            armor.AddModifier(newItem.armorModifier);
            damage.AddModifier(newItem.damageModifier);
        }
    }

    public override void Die()
    {
        base.Die();
        // kill the player in some way
        RemoveHealthBar();
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

        currentHealth -= newDmg;

        if (currentHealth <= 0)
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

            var blockText = CombatTextManager.instance.FetchText(transform.position);
            var blockTextScript = blockText.GetComponent<CombatText>();
            blockTextScript.Gray("-" + newDmg.ToString() + " block", transform.position);
            blockText.transform.position = transform.position;
            blockText.gameObject.SetActive(true);
        }

        if (playerControl.healthGroup.alpha == 0)
        {
            playerControl.healthGroup.alpha = 1f;
        }

        playerControl.healthBar.value = CalculateHealth(currentHealth, maxHealth);
    }

    public override bool Heal(int healthIncrease)
    {
        bool tmp = base.Heal(healthIncrease);

        playerControl.healthBar.value = CalculateHealth(currentHealth, maxHealth);

        return tmp;
    }

    void RemoveHealthBar()
    {
        playerControl.healthGroup.alpha -= 0.02f;
    }

    public void LevelUpStats()
    {
        Agi.SetValue(ExperienceManager.instance.level);
        Str.SetValue(ExperienceManager.instance.level);
        Sta.SetValue(ExperienceManager.instance.level);

        UpdateStats();

        currentHealth = maxHealth;
    }

    public void UpdateStats()
    {
        maxHealth = 100 + ExperienceManager.instance.level + (Sta.GetValue() * 10);
        damage.SetValue(Str.GetValue());
        armor.SetValue(Agi.GetValue());
    }

    public float CalculateHealth(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth;
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
