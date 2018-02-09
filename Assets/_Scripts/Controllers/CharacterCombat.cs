using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour {

    public float attackSpeed = 1f;
    public float attackCooldown;
    public float damageDelay;

    // delegate returns void and has no arguments
    public event System.Action OnAttack;

    public CharacterStats myStats;

	void Start () {
        myStats = GetComponent<CharacterStats>();
	}

    private void Update()
    {
        if (attackCooldown > -1)
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    // takes in the stats of the character that we want to attack
    public void Attack (CharacterStats targetStats, Animator anim)
    {
        if (attackCooldown < 0f)
        {
            anim.SetTrigger("Hit1");
            StartCoroutine(DoDamage(targetStats, damageDelay));

            if (OnAttack != null)
            {
                OnAttack();
            }
            //play animation
            
            // the greater the attack speed the faster the cooldown
            attackCooldown = 1f / attackSpeed;
        }
    }

    IEnumerator DoDamage(CharacterStats stats, float delay)
    {
        yield return new WaitForSeconds(delay);
        stats.TakeDamage(myStats.damage.GetValue());
    }
}
