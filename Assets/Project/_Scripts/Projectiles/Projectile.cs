using UnityEngine;

public class Projectile : MonoBehaviour {

    public int damage;
    public Rigidbody2D rigid;
    ParticleSystem[] impacts;

    public float destroyAfter;

    public Sprite hitSprite;
    public Sprite normalSprite;
    public CircleCollider2D myCollider;
    public SpriteRenderer projectileSpriteRenderer;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        impacts = GetComponentsInChildren<ParticleSystem>();
        myCollider = GetComponent<CircleCollider2D>();
        projectileSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
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
                        if (impact.name == "BloodImpact")
                        {
                            
                            impact.Play();
                        }
                    }

                    GetComponent<SpriteRenderer>().sprite = hitSprite;
                    rigid.isKinematic = true;
                    rigid.velocity = Vector2.zero;
                    myCollider.enabled = false;
                    var script = col.transform.parent.GetComponent<EnemyAI>();
                    script.haveAggro = true;
                    script.arrows.Add(gameObject);
                    script.WalkToShooterPosition(PlayerController.instance.transform.position);
                    script.HyperAlert();
                    transform.parent = script.ArrowHolder;
                    targetStatsScript.TakeDamage(damage);
                }
            }
        }

        else if (col.tag == "ProjectileSurface")
        {
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
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
