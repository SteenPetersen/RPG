using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CurrencyManager : MonoBehaviour {

    public static CurrencyManager instance;

    [SerializeField]
    Text currencyText;

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

    public static int wealth;

    public string MyCurrenyText
    {
        get
        {
            return currencyText.text;
        }

        set
        {
            currencyText.text = value;
        }
    }

    // Use this for initialization
    void Start () {
        wealth = 0;

        MyCurrenyText = wealth.ToString();
	}
	
	// Update is called once per frame
	void Update () {

        if (currencyText.text != wealth.ToString())
        {
            currencyText.text = wealth.ToString();
        }

	}
}
