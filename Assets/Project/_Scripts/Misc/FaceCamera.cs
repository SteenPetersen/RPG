using UnityEngine;

public class FaceCamera : MonoBehaviour {

    [SerializeField] Transform target;

    void Start()
    {
        target = Camera.main.transform;
    }

    void Update()
    {
        transform.rotation = target.rotation;
    }


    void OnWillRenderObject()
    {
        if (!EnemyFaceCamera.FastApproximately(target.rotation.x, transform.rotation.x, 0.00001f))
        {
            transform.rotation = target.rotation;

            if (DebugControl.debugOn)
            {
                Debug.Log("rendering Player");
            }
        }
    }
}
