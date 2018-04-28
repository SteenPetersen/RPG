﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLight : MonoBehaviour {

    public Transform target;


    private void Start()
    {
        target = PlayerController.instance.gameObject.transform;
    }

    // Update is called once per frame
    void Update () {

        transform.position = target.position;

	}

}