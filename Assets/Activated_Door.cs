using UnityEngine;

public class Activated_Door : MonoBehaviour {

    public bool activated;
    [SerializeField] bool open;
    Animator anim;

	void Start ()
    {
        anim = GetComponentInParent<Animator>();
	}
	
	void Update ()
    {
        if (activated && !open)
        {
            anim.SetTrigger("open");
            SoundManager.instance.PlayEnvironmentSound("stone_wall_open");
            open = true;
        }
	}
}
