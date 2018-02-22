using UnityEngine;

public class Projectile : MonoBehaviour {

    public int damage;
    public Rigidbody2D rigid;
    ParticleSystem[] impacts;

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

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            // grab the enemies stats
            EnemyStats targetStatsScript = col.gameObject.GetComponent<EnemyStats>();

            // check to see if he had stats
            if (targetStatsScript != null)
            {
                // let the script know it has been hit by a projectile (useful for ranged aggro)
                targetStatsScript.hitByProjectile = true;

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
                    col.gameObject.GetComponent<EnemyController>().arrows.Add(gameObject);
                    transform.parent = col.transform.Find("Logic");
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
}
