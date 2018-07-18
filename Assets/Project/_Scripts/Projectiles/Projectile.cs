using UnityEngine;

public class Projectile : MonoBehaviour {

    [Header("Stats Settings")]
    public int damage;
    public bool knockBack;
    public ArrowType arrowType;

    [Header("Logic Settings")]
    public Rigidbody2D rigid;
    public ParticleSystem[] impacts;

    [Tooltip("How long till we add it back to the pool of projectile of this type?")]
    public float destroyAfter;

    public Collider2D myCollider;
    public SpriteRenderer projectileSpriteRenderer;
    private PlayerStats playerStats;

    [Tooltip("Make swords invisible and disable colliders shortly after launch but leaves them alive for particle effects")]
    public ProjectileType projectileType;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        impacts = GetComponentsInChildren<ParticleSystem>();
        myCollider = GetComponent<Collider2D>();
        projectileSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        playerStats = PlayerController.instance.gameObject.GetComponent<PlayerStats>();
    }

    private void Invisible()
    {
        projectileSpriteRenderer.enabled = false;
        myCollider.enabled = false;
    }

    private void OnEnable()
    {
        if (projectileType == ProjectileType.Sword)
        {
            Invoke("Invisible", 0.1f);
        }
        Invoke("Destroy", destroyAfter);
    }

    private void Destroy()
    {
        gameObject.SetActive(false);
        transform.parent = null;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // was the collider hit an enemy?
        if (col.tag == "Enemy")
        {
            // grab the enemies stats
            EnemyStats targetStatsScript = col.transform.parent.GetComponent<EnemyStats>();
            // make a ref to the script on the enemy
            EnemyAI script = col.transform.parent.GetComponent<EnemyAI>();

            // check to see if he had stats
            if (targetStatsScript != null && script != null)
            {
                // if he still has health left
                if (targetStatsScript.currentHealth > 0)
                {
                    // play the impact particles that belongs to this enemy
                    ParticleSystemHolder.instance.PlayImpactEffect(col.transform.parent.name + "_impact", transform.position);

  

                    // if this projectile is not a sword
                    if ((int)projectileType == 0)
                    {
                        // increment the amount of arrows that have hit the enemy
                        script.ProjectileWoundGraphics(arrowType);
                    }

                    // Chanage the behaviour of the enemy because he has been hit
                    script.HyperAlert();

                    // does this projectile have the ability to knock things back?
                    if (knockBack)
                    {
                        // create a direction
                        Vector3 dir = col.transform.position - PlayerController.instance.transform.position;
                        // normalize the direction to give it magnitude of 1
                        dir = dir.normalized;
                        // call the knockback method in the direction
                        script.Knockback(dir);
                    }


                    // if this is NOT a max charged hit
                    if (!PlayerController.instance.maxChargedHit)
                    {
                        // stop the projectile from moving any further
                        rigid.isKinematic = true;
                        rigid.velocity = Vector2.zero;

                        // disable the projectile collider so it cannot hit anything else
                        myCollider.enabled = false;

                        // make the arrow invisible
                        projectileSpriteRenderer.enabled = false;

                        // make the enemy take the damage
                        targetStatsScript.TakeDamage(playerStats.damage.GetValue());
                    }

                    // if it is a charged hit let the arrow penetrate all enemies
                    else if (PlayerController.instance.maxChargedHit)
                    {
                        // make the enemy take the damage
                        targetStatsScript.TakeDamage(playerStats.damage.GetValue());
                    }

                }
            }
        }

        else if (col.tag == "ProjectileSurface")
        {
            SoundManager.instance.PlayCombatSound("hit_wall");
            rigid.isKinematic = true;
            myCollider.enabled = false;
            rigid.velocity = Vector2.zero;
            foreach (var impact in impacts)
            {
                if (impact.name == "ArrowImpact")
                {
                    impact.Play();
                }
            }
        }
    }

    /// <summary>
    /// Prepares the projectile to be fired
    /// This method can be optimized
    /// </summary>
    public void MakeProjectileReady()
    {
        // check if this projectile has a chargedParticles effect attached to it and destroy it if it does
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "ChargedShotParticles(Clone)")
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        myCollider.enabled = true;
        rigid.isKinematic = false;
        gameObject.SetActive(true);
        projectileSpriteRenderer.enabled = true;


    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
