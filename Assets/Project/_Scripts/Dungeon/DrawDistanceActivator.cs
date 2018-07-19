using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DrawDistanceActivator : MonoBehaviour {

    public static DrawDistanceActivator instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [SerializeField] float DistanceFromPlayer;
    [SerializeField] GameObject player;

    // objects that can be culled when out of view
    public List<CullableObject> cullableObjects; 


	void Start ()
    {
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cullableObjects = new List<CullableObject>();
        player = GameObject.Find("Player");
        StopCoroutine("Check");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public IEnumerator Check()
    {
        yield return new WaitForSeconds(0.5f);

        List<CullableObject> objectsToRemove = new List<CullableObject>();

        if (cullableObjects.Count > 0)
        {
            foreach (CullableObject currentObject in cullableObjects)
            {

                if (Vector3.Distance(player.transform.position, currentObject.objPos) > DistanceFromPlayer)
                {
                    if (currentObject.obj == null)
                    {
                        objectsToRemove.Add(currentObject);
                    }
                    else
                    {
                        currentObject.obj.SetActive(false);
                    }

                }
                else
                {
                    if (currentObject.obj == null)
                    {
                        objectsToRemove.Add(currentObject);
                    }
                    else
                    {
                        currentObject.obj.SetActive(true);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.01f);

        if (objectsToRemove.Count > 0)
        {
            foreach (CullableObject objectToRemove in objectsToRemove)
            {
                cullableObjects.Remove(objectToRemove);
            }
        }

        yield return new WaitForSeconds(0.01f);


        StartCoroutine("Check");

    }
}

public class CullableObject
{
    public GameObject obj;
    public Vector3 objPos;
}
