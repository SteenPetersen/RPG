using UnityEngine;

public class FollowLight : MonoBehaviour {

    [SerializeField] bool player;
    [SerializeField] Transform target;

    public Transform Target
    {
        get
        {
            return target;
        }

        set
        {
            target = value;
        }
    }

    void Start()
    {
        if (player)
        {
            Target = PlayerController.instance.gameObject.transform;
        }
    }

    void Update ()
    {
        if (target != null)
        {
            transform.position = Target.position;

        }
    }

}
