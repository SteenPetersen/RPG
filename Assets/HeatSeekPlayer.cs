using UnityEngine;

public class HeatSeekPlayer : MonoBehaviour {

    Transform player;
    public Transform boss;
    public float speed;
    public int damage;

    public float destroyAfter;
    public float hitTargets;

	void Start () {

        player = PlayerController.instance.transform;
        boss = GameObject.Find("ImpGiant").transform;
        Destroy(gameObject, destroyAfter);
        Invoke("Boom", hitTargets);
	}
	
	void Update () {

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.position, step);

    }

    void Boom()
    {
        //create layer masks for the player and the obstacles ending a finalmask combining both
        int playerLayer = 10;
        int enemyLayer = 11;
        var playerlayerMask = 1 << playerLayer;
        var enemyLayerMask = 1 << enemyLayer;
        var finalMask = playerlayerMask | enemyLayerMask;

        RaycastHit2D hitPlayer = Physics2D.Raycast(transform.position, (player.position - transform.position), 1f, playerlayerMask);

        RaycastHit2D hitBoss = Physics2D.Raycast(transform.position, (boss.position - transform.position), 1f, enemyLayerMask);





        if (hitPlayer.collider != null)
        {
            Debug.Log("hitPlayer hit: " + hitPlayer.collider.name);

            var playerStat = hitPlayer.collider.gameObject.transform.root.GetComponent<PlayerStats>();
            playerStat.TakeDamage(damage);
            return;
        }
        if (hitBoss.collider != null)
        {
            if (hitBoss.collider.name != "ImpGiant" || hitBoss.collider.name != "ImpGiant(Clone)")
                return;

            Debug.Log("hitBoss hit: " + hitBoss.collider.name);

            var enemyStat = hitBoss.collider.gameObject.transform.root.GetComponent<EnemyStats>();
            enemyStat.TakeDamage(damage);
            return;
        }
    }
}
