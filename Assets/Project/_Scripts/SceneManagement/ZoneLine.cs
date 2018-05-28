using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZoneLine : MonoBehaviour {

    public int ZoneToLoad;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (StoryManager.quest1 < 3)
            {
                StoryManager.instance.NotifyPlayer("You are not ready yet!");
            }
            else
            {
                StartCoroutine(FadeOUt());
            }
        }
    }

    IEnumerator FadeOUt()
    {
        while (GameDetails.instance.fadeToBlack.color.a <= 0.98)
        {
            GameDetails.instance.fadeToBlack.enabled = true;
            GameDetails.instance.fadeToBlack.color = new Color(0, 0, 0, GameDetails.instance.fadeSpeed);
            GameDetails.instance.fadeSpeed += 0.02f;
            yield return new WaitForSeconds(0.01f);
        }

        SceneManager.LoadSceneAsync(ZoneToLoad);
        yield return null;
    }
}
