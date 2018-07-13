using UnityEngine;

public class BoatMovement : MonoBehaviour {

    [SerializeField] Transform target;
    [SerializeField] float speed;
    public bool readyToSail;

    void Update()
    {
        if (readyToSail)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        }

    }
}
