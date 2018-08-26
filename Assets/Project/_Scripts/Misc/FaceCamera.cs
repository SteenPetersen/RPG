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
        if (target != null)
        {
            if (!NumberApproximation(target.rotation.x, transform.rotation.x, 0.00001f))
            {
                transform.rotation = target.rotation;

                if (DebugControl.debugOn)
                {
                    Debug.Log("Rendering: " + gameObject.name);
                }
            }
        }
    }

    public static bool NumberApproximation(float a, float b, float threshold)
    {
        return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
    }
}
