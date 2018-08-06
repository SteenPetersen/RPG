using UnityEngine;
using UnityEngine.Tilemaps;

public class OnGrass : MonoBehaviour {


    [SerializeField] TilemapCollider2D grass;
    [SerializeField] PlayerController player;

    void Start()
    {
        player = PlayerController.instance;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            //Debug.Log("enter " + col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
            player.sandTrail.Stop();
            player.footPrints.Play();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            //Debug.Log("Exit" + col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
            player.sandTrail.Play();
            player.footPrints.Stop();
        }
    }

}
