using UnityEngine;

public class EnemyFaceCamera : MonoBehaviour {

    Transform cam;
    [SerializeField] float threshold;

    public static bool FastApproximately(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }

    void Start()
    {
        cam = Camera.main.transform;
        transform.rotation = cam.rotation;
    }

    void OnWillRenderObject()
    {
        if (!FastApproximately(cam.rotation.x, transform.rotation.x, 0.0001f))
        {
            transform.rotation = cam.rotation;
            //Debug.Log("calling this method");
        }
    }
}
