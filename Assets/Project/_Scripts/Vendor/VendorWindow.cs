using System.Collections.Generic;
using TMPro;
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

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] VendorButton[] vendorButtons;

    [SerializeField] Vendor vendor;
    [SerializeField] TextMeshProUGUI title;
    public Button nextButton;
    public Button prevButton;
    public TextMeshProUGUI amountOfPagesText;

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

    public static bool IsOpen { get; set; } /// Makes more sense to have it here then on the vendor

    List<List<VendorItem>> pages = new List<List<VendorItem>>();

    int pageIndex;

    public void CreatePages(List<VendorItem> items)
    {
        pages.Clear();
        ClearButtons();

        List<VendorItem> page = new List<VendorItem>();

        for (int i = 0; i < items.Count; i++)
        {
            page.Add(items[i]);

            if (page.Count == 10 || i == items.Count - 1)
            {
                pages.Add(page);
                page = new List<VendorItem>();
            }
        }

        AddItems();
    }

    public void AddItems()
    {
        amountOfPagesText.text = pageIndex + 1 + "/" + pages.Count;

        if (pages.Count > 0)
        {
            for (int i = 0; i < pages[pageIndex].Count; i++)
            {
                if (pages[pageIndex][i] != null)
                {
                    vendorButtons[i].AddItem(pages[pageIndex][i]);
                }
            }
        }
    }

    public void OpenWindow()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public void CloseWindow()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        MyVendor.StopInteracting();
    }

    public void NextPage()
    {
        if (pageIndex < pages.Count - 1)
        {
            pageIndex++;
            ClearButtons();
            AddItems();
        }
    }

    public void PreviousPage()
    {
        if (pageIndex > 0)
        {
            pageIndex--;
            ClearButtons();
            AddItems();
        }
    }

    void ClearButtons()
    {
        foreach (var button in vendorButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
