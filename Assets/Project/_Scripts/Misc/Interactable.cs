using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour {

    [SerializeField] protected float radius;
    protected bool hasInteracted;
    protected Transform player;

    [SerializeField] bool drawGizmos;

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
    }

    protected void OnDrawGizmosSelected()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

}
