using UnityEngine;

public class Projectile : MonoBehaviour {

    public int damage;
    Rigidbody2D rigid;
    ParticleSystem impact;
    CircleCollider2D myCollider;

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

            EnemyStats targetStatsScript = col.gameObject.GetComponent<EnemyStats>();
            if (targetStatsScript != null)
            {
                targetStatsScript.hitByProjectile = true;
                if (targetStatsScript.currentHealth > 0)
                {
                    rigid.isKinematic = true;
                    rigid.velocity = Vector2.zero;
                    myCollider.enabled = false;
                    transform.parent = col.transform;
                    targetStatsScript.TakeDamage(damage);
                }
            }
        }

        if (col.tag == "ProjectileSurface")
        {
            rigid.isKinematic = true;
            rigid.velocity = Vector2.zero;
            impact.Play();
        }
    }
}
