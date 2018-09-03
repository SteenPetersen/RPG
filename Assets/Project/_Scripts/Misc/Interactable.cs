using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour {

    [SerializeField] protected float radius;
    protected bool hasInteracted;

    public bool drawGizmos;

    PlayerController pc;

    [Tooltip("Should this interactable stop the player from shooting and hitting while mouse is over them")]
    [SerializeField] bool stopHitAnimationOnMouseOver;

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
        OnMouseExit();
    }

    protected void OnDrawGizmosSelected()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    public virtual void OnMouseEnter()
    {
        if (stopHitAnimationOnMouseOver)
        {
            if (pc == null)
            {
                pc = PlayerController.instance;
            }

            pc.mouseOverInteractable = true;
        }
    }

    public virtual void OnMouseOver()
    {
        /// Nothing here yet
    }

    public virtual void OnMouseExit()
    {
        if (stopHitAnimationOnMouseOver)
        {
            if (pc == null)
            {
                pc = PlayerController.instance;
            }

            pc.mouseOverInteractable = false;
        }


    }

}
