using UnityEngine;

public class DestroyOnCollision : MonoBehaviour {


    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "ProjectileSurface")
        {
            Destroy(gameObject);
        }
    }

}
