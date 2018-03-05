using UnityEngine;
using UnityEngine.SceneManagement;

public class miscellaneousTests : MonoBehaviour
{
    public GameObject monster;
    public Transform spawnPoint;

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.LogWarning("Reloading Scene");
    }

    public void SpawnEnemy()
    {
        float randomX = Random.Range(0, 10) + spawnPoint.position.x;
        float randomY = Random.Range(0, 10) + spawnPoint.position.y;

        Vector3 randomSpawn = new Vector3(randomX, randomY);
        Instantiate(monster, randomSpawn, Quaternion.identity);
        Debug.LogWarning("Spawning enemy");

    }
}
