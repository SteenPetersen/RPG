using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DisactivateByDistance : MonoBehaviour {

    DrawDistanceActivator activatorScript;

	void Start () {

        activatorScript = GameObject.Find("CullingObject").GetComponent<DrawDistanceActivator>();

        activatorScript.cullableObjects.Add(new CullableObject { obj = this.gameObject, objPos = transform.position });
	}

}
