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

    public ParticleSystem impBlood;
    public ParticleSystem redBlood;
    public ParticleSystem bombExplosion;
    public ParticleSystem bossOneFireShield;
    [SerializeField] GameObject townPortal;
    [SerializeField] GameObject townPortalEnter;

    public GameObject ChargedBowShot;

    public GameObject[] stunWords;
    public GameObject[] critWords;


    /// <summary>
    /// Plays an impact effect of a certain name at a certain location
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="pos"></param>
    public void PlayImpactEffect(string clip, Vector2 pos)
    {
        if (clip.StartsWith("Imp") && clip.EndsWith("_impact"))
        {
            Instantiate(impBlood, pos, Quaternion.identity);
        }

        else if (clip == "player")
        {
            Instantiate(redBlood, pos, Quaternion.identity);
        }
    }

    public void PlaySpellEffect(Vector2 pos, string itemOrEffectName)
    {
        if (itemOrEffectName.EndsWith("(Clone)"))
        {
            itemOrEffectName = itemOrEffectName.Remove(itemOrEffectName.Length - 7);
        }

        switch (itemOrEffectName)
        {
            case "Town Portal Book":
                Instantiate(townPortal, pos, Quaternion.identity);
                break;
            case "Town Portal":
                Instantiate(townPortalEnter, pos, Quaternion.identity);
                break;

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
