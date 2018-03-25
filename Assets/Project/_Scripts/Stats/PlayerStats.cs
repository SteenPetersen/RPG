using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;


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




    void Start () {
        EquipmentManager.instance.onEquipmentChanged += OnEquipmentChanged;
        anim = GetComponent<Animator>();
        playerControl = PlayerController.instance;

        UpdateStats();
        currentHealth = maxHealth;
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
        base.TakeDamage(damage);

        SoundManager.instance.PlayCombatSound("player_hurt");

        int newDmg;
        bool crit;

        DamageVariance(damage, out crit, out newDmg);

        currentHealth -= newDmg;

        if (currentHealth <= 0)
        {
            Die();
        }

        CameraShaker.Instance.ShakeOnce(1f, 1f, 0.1f, 0.1f);

        var text = CombatTextManager.instance.FetchText(transform.position);
        var textScript = text.GetComponent<CombatText>();
        textScript.Red(newDmg.ToString(), transform.position);
        text.transform.position = transform.position;
        text.gameObject.SetActive(true);

        if (playerControl.healthGroup.alpha == 0)
        {
            playerControl.healthGroup.alpha = 1f;
        }

        playerControl.healthbar.value = playerControl.CalculateHealth(currentHealth, maxHealth);
    }

    public override bool Heal(int healthIncrease)
    {
        bool tmp = base.Heal(healthIncrease);

        playerControl.healthbar.value = playerControl.CalculateHealth(currentHealth, maxHealth);

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
        maxHealth = ExperienceManager.instance.level + (Sta.GetValue() * 10);
        damage.SetValue(Str.GetValue());
        armor.SetValue(Agi.GetValue());
    }

    private void OnDisable()
    {
        EquipmentManager.instance.onEquipmentChanged -= OnEquipmentChanged;
    }
}
