using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBrick : MonoBehaviour {

    [SerializeField] bool hit;
    [SerializeField] float speed;
    [SerializeField] MeshRenderer rend;
    [SerializeField] Material hitMaterial;
    [SerializeField] ParticleSystem light;

    void Update()
    {
        if (!hit)
        {
            transform.parent.position = new Vector3(PingPong(Time.time * speed, 19.50f, 34f), transform.position.y, 0);
        }
    }

    public void Impact()
    {
        if (!TutorialManager.instance.MyOverTheLine)
        {
            if (StoryManager.tutorialStage == 1 && !hit)
            {
                hit = true;
                rend.material = hitMaterial;
                light.Play();
                TutorialManager.instance.TargetBrickCounter();
                SoundManager.instance.PlayEnvironmentSound("ignite_target");
            }
        }
    }

    //function to change the default starting value of (0, 0, 0) to any value
    float PingPong(float t, float minLength, float maxLength)
    {
        return Mathf.PingPong(t, maxLength - minLength) + minLength;
    }
}
