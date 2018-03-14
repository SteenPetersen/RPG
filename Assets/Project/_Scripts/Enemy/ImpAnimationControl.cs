using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpAnimationControl : MonoBehaviour {

    EnemyAI ai;
    public ParticleSystem particles;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            GetComponent<Animator>().SetTrigger("walk");
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            GetComponent<Animator>().SetTrigger("run");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            GetComponent<Animator>().SetTrigger("explode");
        }
    }

    private void Awake()
    {
        ai = transform.parent.gameObject.GetComponent<EnemyAI>();
    }

    public void StrikeComplete()
    {
        ai.OnStrikeComplete();
    }

    public void CastComplete()
    {
        ai.OnEnemyCastComplete();
    }

    public void startParticles()
    {
        particles.Play();
    }

    public void stopParticles()
    {
        particles.Stop();
    }

    public void CastFireCircle()
    {
        ai.CastFireCircle();
    }

    public void Explode()
    {
        ai.OnExplode();
    }
}
