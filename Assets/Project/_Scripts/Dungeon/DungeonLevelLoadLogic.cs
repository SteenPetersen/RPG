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
                DungeonManager.dungeonLevel++;

                StartCoroutine(GameDetails.MyInstance.FadeOutAndLoadScene(ZoneToLoad));
                loading = true;
            }
        }
    }
}
