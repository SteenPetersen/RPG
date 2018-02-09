using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour {

    public float radius;
    public string objectName;

    [HideInInspector]
    public bool isFocus, hasInteracted = false;

    //Transform player;
    [Tooltip("avoid complications during flipping - feed the 3D selector")]
    public GameObject selector;

    #region Health
    [SerializeField]
    public CanvasGroup healthGroup;
    public Slider healthbar;

    #endregion

    [HideInInspector]
    public PlayerController playercontrol;

    public virtual void Interact()
    {
        // this method is meant to be overwritten
        // Debug.Log("interacting with " + gameObject.name);
    }

    public virtual void Follow()
    {

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void OnFocused(Transform playerTransform)
    {
        isFocus = true;
        //player = playerTransform;
        hasInteracted = false;
    }

    public void OnDeFocused()
    {
        isFocus = false;
        //player = null;
        hasInteracted = false;
    }

    public float CalculateHealth(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth;
    }




}
