using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoatMovement : MonoBehaviour {

    [SerializeField] Transform target;
    [SerializeField] float speed;
    public bool readyToSail;

    [SerializeField] GameObject[] visibleObjects;

    void Update()
    {
        if (readyToSail)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        }

    }

    private void OnEnable()
    {
        // subscribe to notice is a scene is loaded.
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine("CheckForReylith");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    private void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }



    IEnumerator CheckForReylith()
    {
        //Debug.Log("Startinmg search for reylith");
        yield return new WaitForSeconds(1.5f);

        GameObject isReylithInScene = GameObject.Find("Reylith(Clone)");



        if (isReylithInScene != null)
        {
            foreach (var visibleObj in visibleObjects)
            {
                visibleObj.SetActive(true);
            }
            StopCoroutine("CheckForReylith");
        }
        else
        {
            Destroy(gameObject);
        }



    }
}
