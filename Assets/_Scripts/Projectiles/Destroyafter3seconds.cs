using UnityEngine;

public class Destroyafter3seconds : MonoBehaviour {

    private void OnEnable()
    {
        Invoke("Destroy", 3f);
    }

    private void Destroy()
    {
        gameObject.SetActive(false);
        transform.parent = null;
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
