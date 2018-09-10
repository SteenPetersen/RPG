using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretRoom : MonoBehaviour {

    public List<GameObject> coverTiles = new List<GameObject>();

    [SerializeField] int amountOfCoverTiles;
    [SerializeField] float timeBetweenFades;
    public bool faded;
    public bool fading;
    [SerializeField] ParticleSystem glow;


    void Update()
    {
        if (amountOfCoverTiles < coverTiles.Count)
        {
            amountOfCoverTiles = coverTiles.Count;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (!faded)
            {
                glow.Play();
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            glow.Stop();
        }
    }

    public IEnumerator FadeCover()
    {
        glow.Stop();
        fading = true;
        coverTiles.Shuffle();

        while (!faded)
        {
            foreach (GameObject cover in coverTiles)
            {
                cover.AddComponent<Fade>();
                yield return new WaitForSeconds(timeBetweenFades);
            }

            faded = true;
        }

        yield return null;

    }

}
