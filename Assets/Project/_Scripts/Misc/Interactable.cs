using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour {

    [SerializeField] protected float radius;
    [SerializeField] protected string objectName;
    protected bool hasInteracted;
    protected Transform player;

    public float MyRadius
    {
        get { return radius; }
    }

    /// <summary>
    /// Allow other scripts to interact with this abstract class
    /// </summary>
    public virtual void Interact()
    {
        // this method is meant to be overwritten
        //Debug.Log("interacting with " + gameObject.name);
    }

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
