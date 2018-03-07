using UnityEngine;

public class enemy_Projectile : MonoBehaviour {

    public int damage;
    public ParticleSystem impact, particles;
    SpriteRenderer sprite;
    Rigidbody2D rigid;
    Collider2D myCollider;

    

    public float destroyAfter;


    private void OnEnable()
    {
        Invoke("Destroy", destroyAfter);
    }

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
    }

    private void Destroy()
    {
        sprite.enabled = false;
        transform.parent = null;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            //Debug.Log("hit the player");

            PlayerStats stats = col.gameObject.GetComponent<PlayerStats>();

            if (stats != null)
            {
                SoundManager.instance.PlaySound("fireball_impact");
                stats.TakeDamage(damage);
                sprite.enabled = false;
                impact.Play();
                if (particles != null)
                {
                    particles.Stop();
                }
            }
        }

        if (col.tag == "ProjectileSurface")
        {
            //Debug.Log("hit the wall");

            rigid.isKinematic = true;
            myCollider.enabled = false;
            rigid.velocity = Vector2.zero;
            if (particles != null)
            {
                particles.Stop();
            }
        }
    }

    public void MakeProjectileReady()
    {
        myCollider.enabled = true;
        rigid.isKinematic = false;
        gameObject.SetActive(true);
        sprite.enabled = true;
    }


    private void OnDisable()
    {
        CancelInvoke();
    }
}
