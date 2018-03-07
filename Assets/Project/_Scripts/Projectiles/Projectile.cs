using UnityEngine;

public class Projectile : MonoBehaviour {

    [Header("Stats Settings")]
    public int damage;
    public bool knockBack;


    [Header("Logic Settings")]
    public Rigidbody2D rigid;
    public ParticleSystem[] impacts;

    [Tooltip("How long till we add it back to the pool of projectile of this type?")]
    public float destroyAfter;

    public Sprite hitSprite;
    public Sprite normalSprite;
    public Collider2D myCollider;
    public SpriteRenderer projectileSpriteRenderer;

    [Tooltip("Make swords invisible and disable colliders shortly after launch but leaves them alive for particle effects")]
    public bool sword;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        impacts = GetComponentsInChildren<ParticleSystem>();
        myCollider = GetComponent<Collider2D>();
        projectileSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Invisible()
    {
        projectileSpriteRenderer.enabled = false;
        myCollider.enabled = false;
    }

    private void OnEnable()
    {
        if (sword)
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
        if (col.tag == "Enemy")
        {
            // grab the enemies stats
            EnemyStats targetStatsScript = col.transform.parent.GetComponent<EnemyStats>();

            // check to see if he had stats
            if (targetStatsScript != null)
            {
                // if he still has health left
                if (targetStatsScript.currentHealth > 0)
                {
                    foreach (var impact in impacts)
                    {
                        if (impact.name == "Enemy")
                        {
                            impact.Play();
                        }
                    }

                    GetComponent<SpriteRenderer>().sprite = hitSprite;
                    rigid.isKinematic = true;
                    rigid.velocity = Vector2.zero;
                    myCollider.enabled = false;
                    var script = col.transform.parent.GetComponent<EnemyAI>();
                    //script.haveAggro = true;
                    script.arrows.Add(gameObject);
                    //script.WalkToShooterPosition(PlayerController.instance.transform.position);
                    script.HyperAlert();
                    if (knockBack)
                    {
                        Vector3 dir = col.transform.position - PlayerController.instance.transform.position;
                        dir = dir.normalized;
                        script.Knockback(dir);
                    }
                    transform.parent = script.ArrowHolder;
                    targetStatsScript.TakeDamage(damage);
                }
            }
        }

        else if (col.tag == "ProjectileSurface")
        {
            SoundManager.instance.PlaySound("hit_wall");
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

    public void MakeProjectileReady()
    {
        myCollider.enabled = true;
        rigid.isKinematic = false;
        gameObject.SetActive(true);
        projectileSpriteRenderer.sprite = normalSprite;
        projectileSpriteRenderer.enabled = true;
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
