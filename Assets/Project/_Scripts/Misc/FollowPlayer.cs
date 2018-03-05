using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    GameObject player;

	void Start () {
        player = GameObject.Find("Player");
	}
	
	void Update () {

        if (transform.position != player.transform.position)
        {
            transform.position = player.transform.position;
        }
        if (transform.rotation != player.transform.rotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, player.transform.localEulerAngles.z);
        }
    }
}
