using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

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

    public Transform lookAt;
    public float cameraRot;
    public Transform measurementTransform;
    public Camera mainCamera;
    public int fieldOfViewBase = 48;
    public int fieldOfViewDungeon = 40;
    PlayerController player;

    [SerializeField] Vector3 cameraOffset;


    [SerializeField] Light dayLight;
    [SerializeField] Camera[] cameras;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += ChangingCameraSpecs;
    }

    private void ChangingCameraSpecs(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.EndsWith("_indoor"))
        {
            dayLight.intensity = 1;
            mainCamera.fieldOfView = 48;
            mainCamera.backgroundColor = Color.black;

            if (scene.name == "StartArea")
            {

            }
        }

        else if (scene.name.EndsWith("_indoor"))
        {
            dayLight.intensity = 0.43f;
            mainCamera.fieldOfView = 40;
            mainCamera.backgroundColor = Color.black;
        }

        foreach (Camera cam in cameras)
        {
            cam.fieldOfView = mainCamera.fieldOfView;
            cam.backgroundColor = mainCamera.backgroundColor;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        mainCamera.transparencySortMode = TransparencySortMode.Orthographic;
        player = PlayerController.instance;
    }

    private void Update()
    {
        if (lookAt == null)
        {
            lookAt = player.gameObject.transform;
        }


        if (transform.position != lookAt.position + cameraOffset)
        {
            transform.position = lookAt.position + cameraOffset;
        }
    }

    private void FixedUpdate()
    {
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (GameDetails.instance.paused || player.Portal)
            return;

        if (Input.GetKey(KeybindManager.instance.CameraBinds["CAMERACCW"]))
        {
            transform.Rotate(0, 0, cameraRot * Time.deltaTime);
        }
        if (Input.GetKey(KeybindManager.instance.CameraBinds["CAMERACW"]))
        {
            transform.Rotate(0, 0, -cameraRot * Time.deltaTime);
        }
    }

    public void SetHomeRotation()
    {
        transform.rotation = Quaternion.identity;
    }

    void OnDisable()
    {
        // unsubscribe from the scenemanager.
        SceneManager.sceneLoaded -= ChangingCameraSpecs;
    }

    #region renderOrderAxis
    //public void DetermineRenderOrderAxis()
    //{
    //    if (n >= 0 && n < 45)
    //    {
    //        renderAxisCase = 1;

    //        GraphicsSettings.transparencySortAxis = new Vector3(0, 1, 0);
    //    }
    //    else if (n >= 45 && n < 90)
    //    {
    //        renderAxisCase = 2;

    //        GraphicsSettings.transparencySortAxis = new Vector3(0.5f, 0.5f, 0);
    //    }
    //    else if (n >= 90 && n < 135)
    //    {
    //        renderAxisCase = 3;

    //        GraphicsSettings.transparencySortAxis = new Vector3(1, -0.5f, 0);
    //    }
    //    else if (n >= 135 && n < 175)
    //    {
    //        renderAxisCase = 4;

    //        GraphicsSettings.transparencySortAxis = new Vector3(0.5f, -1, 0);
    //    }
    //    else if (n >= 175 && n < 225)
    //    {
    //        renderAxisCase = 5;

    //        GraphicsSettings.transparencySortAxis = new Vector3(0, -1, 0);
    //    }
    //    else if (n >= 225 && n < 270)
    //    {
    //        renderAxisCase = 6;

    //        GraphicsSettings.transparencySortAxis = new Vector3(-0.5f, -0.5f, 0);
    //    }
    //    else if (n >= 270 && n < 315)
    //    {
    //        renderAxisCase = 7;

    //        GraphicsSettings.transparencySortAxis = new Vector3(-1, 0.5f, 0);
    //    }
    //    else if (n >= 315 && n < 360)
    //    {
    //        renderAxisCase = 8;

    //        GraphicsSettings.transparencySortAxis = new Vector3(-0.5f, 1, 0);
    //    }
    //}

    #endregion renderOrderAxis
}
