using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    #region Singleton

    public static CameraController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    public GameObject lookAt;
    public float cameraRot;
    public Transform measurementTransform;

    public float test;

    Vector3 offSet;

    private void Start()
    {
        offSet = new Vector3(0f, 0f, -10f);
    }
    private void Update()
    {

        if (lookAt == null)
        {
            lookAt = GameObject.Find("Player");
        }
    }

    private void FixedUpdate()
    {
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (Input.GetKey("q"))
        {
            transform.Rotate(0, 0, cameraRot * Time.deltaTime);
        }
        if (Input.GetKey("e"))
        {
            transform.Rotate(0, 0, -cameraRot * Time.deltaTime);
        }
        if (lookAt != null)
        {
            Vector3 desiredPosition = lookAt.transform.position + offSet;
            transform.position = desiredPosition;
        }
    }
}
