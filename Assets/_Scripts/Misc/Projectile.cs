using UnityEngine;

public class Projectile : MonoBehaviour {

    public int damage;
    Rigidbody2D rigid;
    ParticleSystem impact;
    public CircleCollider2D myCollider;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        impact = GetComponentInChildren<ParticleSystem>();
        myCollider = GetComponent<CircleCollider2D>();
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
            impact.Play();
        }
    }
}
