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
        //Debug.Log(transform.name + "died");
    }

    public virtual void Regenerate()
    {
        if (currentHealth < maxHealth)
        {
            Debug.Log("regen started");
            //currentHealth += 1 * 
        }
    }


    /// <summary>
    /// Create a damage variance so that the results of hitting are not so similar
    /// </summary>
    /// <param name="damage">Amount of damage that has come in</param>
    /// <param name="crit">This function will determine if the hit was a crit or not</param>
    /// <param name="newDamage">The new damage amount after applying the variance</param>
    /// 
    public virtual void DamageVariance(int damage, out bool crit, out int newDamage)
    {
        // Set a minimum Value
        var minValue = 0;

        if (!PlayerController.instance.blocking)
        {
            minValue = Random.Range(1, 5);
        }


        damage -= armor.GetValue() / 2;

        //generate a random number
        float rand = Random.Range(0.0f, damage);

        float limit = damage * 0.9f;

        newDamage = Mathf.Clamp(damage + (int)rand, minValue, int.MaxValue);

        // if the random number is above the 90% mark which means 10% chance to crit
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
