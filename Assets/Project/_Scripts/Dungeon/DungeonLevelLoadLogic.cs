using UnityEngine;


public class DungeonLevelLoadLogic : MonoBehaviour {

    [SerializeField] int ZoneToLoad;
    bool loading;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (!loading)
            {
                // add to statistics
                GameDetails.dungeonFloorsExplored++;

                // set the camera correctly so we can know the outset of the player when zoning in if not it can be hard to determine how dialogue looks.
                CameraController.instance.transform.rotation = Quaternion.Euler(0, 0, 0);

                StartCoroutine(GameDetails.Instance.FadeOutAndLoadScene(ZoneToLoad));
                loading = true;
            }
        }
    }
}
