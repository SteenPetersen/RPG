using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    public Camera cam;
    public int fieldOfViewBase = 48;
    public int fieldOfViewDungeon = 40;
    PlayerController player;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        cam.transparencySortMode = TransparencySortMode.Orthographic;
        player = PlayerController.instance;
    }

    private void Update()
    {
        if (lookAt == null)
        {
            lookAt = player.gameObject;
        }
    }

    private void FixedUpdate()
    {
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (GameDetails._instance.paused || player.Portal)
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
