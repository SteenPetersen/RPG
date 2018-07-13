using UnityEngine;

public class SpikeTrapActivate : MonoBehaviour {

    [SerializeField] Animator anim;
    [SerializeField] int damagePercent;
    [Tooltip("The range at which this trap can hit the player when activated - check yellow Gizmo")]
    [SerializeField] float range;
    SpriteRenderer rend;  // needed to stop the trap from playing reset sound if it is not on screen

    void Start () {
        anim = GetComponent<Animator>();
        rend = GetComponentInParent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            anim.SetBool("Fire", true);
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("SpikeTrap"))
            {
                SoundManager.instance.PlayEnvironmentSound("spike_trap_fire");
                Debug.Log("everything checks out!");
            }

        }
    }

    /// <summary>
    /// shoots out a ray to check if player was within 
    /// range for the trap to hit or not
    /// </summary>
    public void CheckIfPlayerWasDamaged()
    {
        Debug.Log("Checking damage");

        //create layer masks for the player
        int playerLayer = 10;
        var playerlayerMask = 1 << playerLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, PlayerController.instance.gameObject.transform.position - transform.position, range, playerlayerMask);

        if (hit.transform != null)
        {
            if (hit.collider.name == "Player")
            {
                if (hit.collider.transform.root.GetComponent<PlayerStats>() != null)
                {
                    // play the impact particles
                    ParticleSystemHolder.instance.PlayImpactEffect("player", hit.collider.transform.position);

                    var script = hit.collider.transform.root.GetComponent<PlayerStats>();
                    script.TakeDamage(((int)PlayerStats.instance.maxHealth / 100) * damagePercent);
                }
            }
        }
    }

    /// <summary>
    /// Resets the trap and plays a reset sound 
    /// if the trap is visible on screen
    /// </summary>
    public void ResetTrap()
    {
        anim.SetBool("Fire", false);
        if (rend.isVisible)
        {
            SoundManager.instance.PlayEnvironmentSound("spike_trap_reset");
        }
    }


    /// <summary>
    /// Draws the gizmos for range to more easily assess 
    /// the distance this trap can hit
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // set the color of the gizmo
        Gizmos.color = Color.yellow;
        // set the size of the gizmo ( this one relates to the range at which the player will consider enemies to be "in range")
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
