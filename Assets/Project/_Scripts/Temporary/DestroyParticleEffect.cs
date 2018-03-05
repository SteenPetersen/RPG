using UnityEngine;

public class DestroyParticleEffect : MonoBehaviour {

	void Start () {

        Invoke("Destroy", 1.5f);

	}

    void Destroy()
    {
        Destroy(gameObject);
    }

}
