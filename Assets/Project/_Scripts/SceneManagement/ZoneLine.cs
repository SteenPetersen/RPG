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
            if (StoryManager.questLine < 2)
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
        while (GameDetails._instance.fadeToBlack.color.a <= 0.98)
        {
            GameDetails._instance.fadeToBlack.enabled = true;
            GameDetails._instance.fadeToBlack.color = new Color(0, 0, 0, GameDetails._instance.fadeSpeed);
            GameDetails._instance.fadeSpeed += 0.02f;
            yield return new WaitForSeconds(0.01f);
        }

        SceneManager.LoadSceneAsync(ZoneToLoad);
        yield return null;
    }
}
