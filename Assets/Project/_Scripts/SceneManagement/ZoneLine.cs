using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZoneLine : MonoBehaviour {

    [SerializeField] string ZoneToLoad;
    bool loading;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (StoryManager.stage < 2)
            {
                StoryManager.instance.NotifyPlayer("You are not ready yet!");
            }
            else
            {
                if (!loading)
                {
                    StartCoroutine(GameDetails.MyInstance.FadeOutAndLoadScene(ZoneToLoad));
                    GameDetails.MyInstance.Save(true);
                    loading = true;
                }
            }
        }
    }


}
