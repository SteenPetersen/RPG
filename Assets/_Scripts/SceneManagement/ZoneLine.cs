using UnityEngine;
using UnityEngine.SceneManagement;

public class ZoneLine : MonoBehaviour {

    public int ZoneToLoad;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            SceneManager.LoadSceneAsync(ZoneToLoad);
        }
    }
}
