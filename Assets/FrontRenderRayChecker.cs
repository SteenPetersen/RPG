using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontRenderRayChecker : MonoBehaviour {

    [SerializeField] Transform cam;
    [SerializeField] int renderInFrontLayer;
    [SerializeField] int renderBehindLayer;
    [SerializeField] GameObject toRender;

    [SerializeField] bool debug;

    void Start ()
    {
        cam = Camera.main.transform;
	}
	
	void Update () {

        if (Time.frameCount % 10 == 0)
        {
            RayCastCheck();
        }
    }

    void RayCastCheck()
    {
        RaycastHit hit;

        var direction = cam.position - transform.position;

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity))
        {
            toRender.layer = renderBehindLayer;

            if (debug)
            {
                Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow);
                Debug.Log("Hit something");
            }
        }
        else
        {
            toRender.layer = renderInFrontLayer;

            if (debug)
            {
                Debug.DrawRay(transform.position, direction * 1000, Color.white);
                Debug.Log("Not hitting");
            }
        }
    }
}
