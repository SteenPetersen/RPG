using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckSurroundings : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.tag == "Enemy")
        {
            Debug.Log("enemy in vicinity");
        }
    }
}
