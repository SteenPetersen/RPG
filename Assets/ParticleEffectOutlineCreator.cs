using UnityEngine;

public class ParticleEffectOutlineCreator : MonoBehaviour {

    [SerializeField] uint seed;
    [SerializeField] bool playOnAwake;
    [SerializeField] ParticleSystem p;


    void Start ()
    {
        p = this.GetComponent<ParticleSystem>();
        p.Stop();
        if (p.isStopped)
        {
            p.randomSeed = seed;
        }
        if (playOnAwake)
        {
            p.Play();
        }
    }

    public void Go()
    {
        if (p == null)
        {
            p = this.GetComponent<ParticleSystem>();
        }
        p.Stop();
        p.randomSeed = seed;
        if (playOnAwake)
        {
            p.Play();
        }
    }


}
