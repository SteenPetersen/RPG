using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour {

    void Start()
    {
        GetComponent<ParticleSystem>().randomSeed = 43;
    }
}
