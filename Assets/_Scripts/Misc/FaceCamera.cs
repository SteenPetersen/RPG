using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

    public Transform target;

    void Start()
    {
        target = GameObject.Find("MainCamera").GetComponent<Transform>();
    }

    void Update()
    {

        if (target != null)

            //transform.eulerAngles = new Vector3(target.rotation.x, 0, 0);

        this.transform.rotation = Camera.main.transform.rotation;
    }
}
