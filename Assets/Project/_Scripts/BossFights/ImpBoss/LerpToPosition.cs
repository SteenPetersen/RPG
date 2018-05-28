using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpToPosition : MonoBehaviour {

    [SerializeField] float speed = .5f;
    [SerializeField] int damage;
    [SerializeField] float range;

    Vector2 start;
    Vector2 des;
    float fraction = 0;
    bool initialized, obstacleInTheWay;
    float timer;
    SpriteRenderer mySprite;
    ParticleSystem sparks;

    void Start () {

        start = new Vector2(transform.position.x, transform.position.y);
        timer = UnityEngine.Random.Range(2, 5);
        mySprite = transform.Find("BossBomb").GetComponent<SpriteRenderer>();
        sparks = transform.Find("Spark").GetComponent<ParticleSystem>();

        Destroy(gameObject, 5.5f);
    }
	
	void Update () {

        if (initialized)
        {
            timer -= Time.deltaTime;

            if (fraction < 1 && !obstacleInTheWay)
            {
                CheckForObstacles();
                fraction += Time.deltaTime * speed;
                transform.position = Vector2.Lerp(start, des, fraction);
            }

            if (timer < 0)
            {
                Explode();
            }
        }
    }

    /// <summary>
    /// Set the destination at which this bomb will stop
    /// </summary>
    /// <param name="destination"></param>
    public void SetDestination(Vector2 destination)
    {
        des = destination;
        initialized = true;
    }

    /// <summary>
    /// when timer expires explode the bomb
    /// </summary>
    void Explode()
    {
        mySprite.enabled = false;
        sparks.Stop();

        Instantiate(ParticleSystemHolder.instance.bombExplosion, transform.position, Quaternion.identity);
        SoundManager.instance.PlayCombatSound("bomb");
;
        // check if damage has been done to player and apply said damage
        int playerLayer = 10;
        var playerlayerMask = 1 << playerLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, PlayerController.instance.gameObject.transform.position - transform.position, range, playerlayerMask);

        if (hit.transform != null)
        {
            if (hit.collider.name == "Player")
            {
                if (hit.collider.transform.root.GetComponent<PlayerStats>() != null)
                {
                    var script = hit.collider.transform.root.GetComponent<PlayerStats>();
                    script.TakeDamage(damage);
                }
            }
        }

        initialized = false;
    }

    /// <summary>
    /// Checks to see if the bomb is near any obstacles
    /// </summary>
    /// <returns></returns>
    void CheckForObstacles()
    {
        int obstacleLayer = 13;
        var obstacleLayerMask = 1 << obstacleLayer;

        RaycastHit2D hit1 = Physics2D.Raycast(transform.position, Vector2.up, 0.7f, obstacleLayerMask);
        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, Vector2.down, 0.7f, obstacleLayerMask);
        RaycastHit2D hit3 = Physics2D.Raycast(transform.position, Vector2.right, 0.7f, obstacleLayerMask);
        RaycastHit2D hit4 = Physics2D.Raycast(transform.position, Vector2.left, 0.7f, obstacleLayerMask);

        if (hit1.collider == null && 
            hit2.collider == null && 
            hit3.collider == null && 
            hit4.collider == null)
        {
            obstacleInTheWay = false;
            return;
        }

        obstacleInTheWay = true;
    }

    /// <summary>
    /// Draws a gizmo to show the range in which this bomb will hit
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // set the color of the gizmo
        Gizmos.color = Color.yellow;
        // set the size of the gizmo ( this one relates to the range at which the player will consider enemies to be "in range")
        Gizmos.DrawWireSphere(transform.position, range);
    } // commented
}

