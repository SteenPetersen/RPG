using UnityEngine;

public class CharacterStats : MonoBehaviour {

    public float maxHealth = 100;
    public float currentHealth { get; set; }

    public float regenRate;

    public Stat damage;
    public Stat armor;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {


    }

    public virtual void Die()
    {
        // Die in some way this method is meant to be overwritten
        Debug.Log(transform.name + "died");
    }

    public virtual void Regenerate()
    {
        if (currentHealth < maxHealth)
        {
            Debug.Log("regen started");
            //currentHealth += 1 * 
        }
    }


    // logic behind variance in damage
    public virtual void DamageVariance(int damage, out bool crit, out int newDamage)
    {
        // decrease damage by the armor of the player
        var minValue = Random.Range(1, 5);
        damage -= armor.GetValue() / 2;

        //generate a random number
        float rand = Random.Range(0.0f, damage);

        float limit = damage * 0.9f;

        newDamage = Mathf.Clamp(damage + (int)rand, minValue, int.MaxValue);

        if (rand > limit)
        {
            crit = true;
            return;
        }
        
        crit = false;
        //Debug.Log(transform.name + " takes " + newDamage + " points of damage " + transform.name + " armor is " + armor.GetValue());
    }

    public virtual bool Heal(int healthIncrease)
    {
        if (currentHealth < maxHealth)
        {

            var text = CombatTextManager.instance.FetchText(transform.position);
            var textScript = text.GetComponent<CombatText>();
            textScript.Green(healthIncrease.ToString(), transform.position);
            text.transform.position = transform.position;
            text.gameObject.SetActive(true);

            currentHealth += Mathf.Clamp(healthIncrease, 1, maxHealth - currentHealth);
            return true;
        }
        else if (currentHealth == maxHealth)
        {
            return false;
        }

        return false;
    }
}
