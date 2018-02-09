using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakBrick : MonoBehaviour {

    public int hp = 3;
    public GameObject breakParticles;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (hp <= 0)
        {
            Break();
        }
		
	}

    public void Damage()
    {
        hp -= 1;
    }

    void Break()
    {

        Instantiate(breakParticles, gameObject.transform.position, Quaternion.Euler(-180,0,0));
        Destroy(gameObject);
    }
}
