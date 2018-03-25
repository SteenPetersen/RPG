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

    public void PlayImpactEffect(string clip, Vector2 pos)
    {
        if (clip.StartsWith("Imp") && clip.EndsWith("_impact"))
        {
            var effect = Instantiate(ImpBlood, pos, Quaternion.identity);
        }

    }

}
