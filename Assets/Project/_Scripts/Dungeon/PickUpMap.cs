using UnityEngine;
using UnityEngine.UI;

public class PickUpMap : MonoBehaviour {

    public GameObject miniMap;

    private void OnTriggerEnter2D(Collider2D coll)
    {


        if (coll.gameObject.tag == "Player")
        {
            var game = GameDetails._instance.transform;
            miniMap = game.Find("Minimap").gameObject;

            miniMap.SetActive(true);

            Destroy(gameObject);
        }
    }
}
