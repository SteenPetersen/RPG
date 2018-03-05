using UnityEngine;

public class WavesThatFollow : MonoBehaviour {

    public Transform top;
    public Transform bottom;

    public float speed = 1.0F;

    Vector3 destination;

    void Start () {


    }
	
	void Update ()
    {
        MoveDown();
    }

    private void MoveDown()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, destination, step);
        if (Mathf.Round(transform.position.y) == Mathf.Round(destination.y))
        {
            if (Mathf.Round(transform.position.y) == Mathf.Round(top.position.y))
            {
                destination = bottom.position;
            }
            else if (Mathf.Round(transform.position.y) == Mathf.Round(bottom.position.y))
            {
                destination = top.position;
            }
        }
    }
}
