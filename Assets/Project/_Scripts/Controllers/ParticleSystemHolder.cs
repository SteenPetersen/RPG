using UnityEngine;

public class ParticleSystemHolder : MonoBehaviour {

    public static ParticleSystemHolder instance;

    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public ParticleSystem ImpBlood;
    public ParticleSystem bombExplosion;
    public GameObject[] stunWords;
    public GameObject[] critWords;



    public void PlayImpactEffect(string clip, Vector2 pos)
    {
        if (clip.StartsWith("Imp") && clip.EndsWith("_impact"))
        {
            Instantiate(ImpBlood, pos, Quaternion.identity);
        }
    }

    public GameObject PlayerStunnedEffect()
    {
        int rnd = UnityEngine.Random.Range(0, stunWords.Length);

        switch (rnd)
        {
            case 0:
                return stunWords[0];

            case 1:
                return stunWords[1];

            case 2:
                return stunWords[2];
        }

        return stunWords[0];

    }

    public GameObject CritWord()
    {
        int rnd = UnityEngine.Random.Range(0, stunWords.Length);

        Debug.Log(rnd);

        switch (rnd)
        {
            case 0:
                return critWords[0];

            case 1:
                return critWords[1];

            case 2:
                return critWords[2];
        }

        return null;

    }

}
