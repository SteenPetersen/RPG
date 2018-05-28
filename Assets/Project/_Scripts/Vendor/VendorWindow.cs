using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VendorWindow : MonoBehaviour {

    public static VendorWindow instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }

    }

    [SerializeField]
    Vendor vendor;
    [SerializeField]
    Text title;

    public Vendor MyVendor
    {
        get
        {
            return vendor;
        }

        set
        {
            vendor = value;
        }
    }

    public string MyTitle
    {
        get
        {
            return title.text;
        }

        set
        {
            title.text = value;
        }
    }

    public void CloseWindow()
    {
        vendor.OpenClose();
    }
}
