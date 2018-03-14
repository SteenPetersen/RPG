using UnityEngine;

public class ExplosionPlayerProximityChecker : MonoBehaviour {

    public int damage;
    public float range;

	void Start () {
        RaycastPlayer();
        Destroy(gameObject, 3f);
    }


    void RaycastPlayer()
    {
        //create layer masks for the player
        int playerLayer = 10;
        var playerlayerMask = 1 << playerLayer;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, PlayerController.instance.gameObject.transform.position - transform.position, range, playerlayerMask);

        var drawdirection = (PlayerController.instance.gameObject.transform.position - transform.position).normalized;
        Debug.DrawRay(transform.position, (PlayerController.instance.gameDetails.transform.position - transform.position), Color.grey);
        Debug.DrawLine(transform.position, (drawdirection * range) + transform.position, Color.yellow, 1f);
        if (hit.transform != null)
        {
            if (hit.collider.name == "Player")
            {
                Debug.Log("Hit the player");
                if (hit.collider.transform.root.GetComponent<PlayerStats>() != null)
                {
                    var script = hit.collider.transform.root.GetComponent<PlayerStats>();
                    script.TakeDamage(damage);
                }
            }
        }
    }
}
