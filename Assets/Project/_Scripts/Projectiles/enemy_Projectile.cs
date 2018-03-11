using UnityEngine;

public class enemy_Projectile : MonoBehaviour {

    public int damage;
    public ParticleSystem impact;
    public ParticleSystem wallImpact;
    public ParticleSystem particles;
    SpriteRenderer sprite;
    Rigidbody2D rigid;
    Collider2D myCollider;

    Transform projectileHolder;

    public float destroyAfter;


    private void OnEnable()
    {
        Invoke("Destroy", destroyAfter);
    }

    private void Awake()
    {
        projectileHolder = GameObject.Find("ProjectileHolder").transform;
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
    }

    private void Destroy()
    {
        sprite.enabled = false;
        transform.SetParent(projectileHolder);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            //Debug.Log("hit the player");

            PlayerStats stats = col.gameObject.GetComponent<PlayerStats>();

            if (stats != null)
            {
                SoundManager.instance.PlayCombatSound(gameObject.name + "_impact");
                stats.TakeDamage(damage);
                sprite.enabled = false;
                myCollider.enabled = false;
                rigid.velocity = Vector2.zero;
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
            SoundManager.instance.PlayCombatSound(gameObject.name + "_wallimpact");

            //rigid.isKinematic = true;
            myCollider.enabled = false;
            rigid.velocity = Vector2.zero;
            if (wallImpact != null)
            {
                wallImpact.Play();
            }

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
        impact = transform.Find("impact").GetComponent<ParticleSystem>();
        particles = transform.Find("particles").GetComponent<ParticleSystem>();
        wallImpact = transform.Find("wallimpact").GetComponent<ParticleSystem>();
    }


    private void OnDisable()
    {
        CancelInvoke();
    }
}
