using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraLight : MonoBehaviour {

    public Light miniMapLight;


    private void OnPreCull()
    {
        miniMapLight.enabled = true;
    }
    private void OnPreRender()
    {
        miniMapLight.enabled = false;
    }

    private void OnPostRender()
    {
        miniMapLight.enabled = false;
    }
}
