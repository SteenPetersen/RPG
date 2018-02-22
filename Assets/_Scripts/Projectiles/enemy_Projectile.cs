using UnityEngine;

public class enemy_Projectile : MonoBehaviour {

    public int damage;
    public ParticleSystem impact;
    SpriteRenderer sprite;
    Rigidbody2D rigid;
    CircleCollider2D myCollider;


    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            Debug.Log("hit the player");

            PlayerStats stats = col.gameObject.GetComponent<PlayerStats>();

            if (stats != null)
            {
                SoundManager.instance.PlaySound("fireball_impact");
                stats.TakeDamage(damage);
                sprite.enabled = false;
                impact.Play();
            }
        }

        if (col.tag == "ProjectileSurface")
        {
            Debug.Log("hit the player");

            rigid.isKinematic = true;
            myCollider.enabled = false;
            rigid.velocity = Vector2.zero;
        }
    }

    public void MakeProjectileReady()
    {
        myCollider.enabled = true;
        rigid.isKinematic = false;
        gameObject.SetActive(true);
        sprite.enabled = true;
    }
}
