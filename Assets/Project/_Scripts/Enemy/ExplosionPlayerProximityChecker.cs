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

        int destructableLayer = 19;
        var destructableLayerMask = 1 << destructableLayer;

        Collider2D[] destructableColliders = Physics2D.OverlapCircleAll(transform.position, range, destructableLayerMask);

        if (destructableColliders.Length > 0)
        {
            foreach (Collider2D target in destructableColliders)
            {
                if (target.gameObject.GetComponent<Destructable>() != null)
                {
                    target.gameObject.GetComponent<Destructable>().InstantDestroy();
                }
            }
        }
    }
}
