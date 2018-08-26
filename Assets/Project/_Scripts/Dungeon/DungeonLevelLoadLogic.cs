using UnityEngine;


public class DungeonLevelLoadLogic : MonoBehaviour {

    [SerializeField] string ZoneToLoad;
    bool loading;
    [SerializeField] string[] loadingTexts;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (!loading)
            {
                // add to statistics
                GameDetails.dungeonFloorsExplored++;
                DungeonManager.dungeonLevel++;

                int rnd = UnityEngine.Random.Range(0, loadingTexts.Length);

                StartCoroutine(GameDetails.MyInstance.FadeOutAndLoadScene(ZoneToLoad, loadingTexts[rnd]));
                loading = true;
            }
        }
    }
}
