using UnityEngine;
using System.Collections;

public class AvoidPause : MonoBehaviour
{
    public ParticleSystem part;

    void Update()
    {
        if (Time.timeScale < 0.01f)
        {
            part.Simulate(Time.unscaledDeltaTime, true, false);
        }
    }
}
