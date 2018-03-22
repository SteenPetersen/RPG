using UnityEngine;

public class DestroyParticleEffect : MonoBehaviour {

    public float delay;

	void Start () {

        Invoke("Destroy", delay);

	}

    void Destroy()
    {
        Destroy(gameObject);
    }

}
