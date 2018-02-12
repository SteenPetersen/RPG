using UnityEngine;

public class MeleeCollision : MonoBehaviour {

    #region Singleton

    public static MeleeCollision instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    ParticleSystem impact;
    public PlayerStats playerStat;
    PlayerController player;
    int damage;

    [HideInInspector]
    public BoxCollider2D myCollider;


    void Start () {
        player = PlayerController.instance;
        playerStat = player.GetComponent<PlayerStats>();
        myCollider = GetComponent<BoxCollider2D>();
        
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            damage = playerStat.damage.GetValue();
            EnemyStats targetStatsScript = col.gameObject.GetComponent<EnemyStats>();
            if (targetStatsScript != null)
            {
                if (targetStatsScript.currentHealth > 0)
                {
                    Debug.Log("sword collison detected");
                    //myCollider.enabled = false;
                    targetStatsScript.TakeDamage(damage);
                }
            }
        }

        if (col.tag == "ProjectileSurface")
        {
            impact.Play();
        }
    }
}
