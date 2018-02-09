using UnityEngine;

public class CharacterStats : MonoBehaviour {

    public float maxHealth = 100;
    public float currentHealth { get; private set; }

    public float regenRate;

    public Stat damage;
    public Stat armor;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        damage -= armor.GetValue();
        damage = Mathf.Clamp(damage, 0, int.MaxValue);
        currentHealth -= damage;
        Debug.Log(transform.name + " takes " + damage + " points of damage " + transform.name + " armor is " + armor.GetValue());

        if (currentHealth <= 0)
        {
            Die();
        }
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
}
