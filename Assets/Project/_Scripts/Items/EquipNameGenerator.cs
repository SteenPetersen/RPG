using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipNameGenerator : MonoBehaviour {

    public static EquipNameGenerator _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
    }

    public List<string> swordVerb = new List<string>();
    public List<string> swordNoun = new List<string>();
    public List<string> swordTitle = new List<string>();
    public List<string> swordUnique = new List<string>();

    public List<string> bowVerb = new List<string>();
    public List<string> bowNoun = new List<string>();
    public List<string> bowTitle = new List<string>();
    public List<string> bowUnique = new List<string>();

    public List<string> shieldVerb = new List<string>();
    public List<string> shieldNoun = new List<string>();
    public List<string> shieldTitle = new List<string>();
    public List<string> shieldUnique = new List<string>();

    public List<string> LeatherVerb = new List<string>();
    public List<string> metalVerb = new List<string>();
    public List<string> magicalVerb = new List<string>();

    public List<string> helmNoun = new List<string>();
    public List<string> chestNoun = new List<string>();
    public List<string> legNoun = new List<string>();
    public List<string> footNoun = new List<string>();
    public List<string> handNoun = new List<string>();
    public List<string> shoulderNoun = new List<string>();

    public List<string> armorTitle = new List<string>();
    public List<string> armorUnique = new List<string>();

    /// <summary>
    /// Creates a sword name based on the quality of the item
    /// </summary>
    /// <param name="q">Quality to base the name on</param>
    /// <returns></returns>
    public string GetSwordName(Quality q)
    {
        string generatedName = "";

        float x = UnityEngine.Random.Range(0.0f, 100.0f);

        if ((int)q > 0)
        {
            if (x > 60.0f)
            {
                int v = UnityEngine.Random.Range(0, swordVerb.Count);
                generatedName = generatedName + swordVerb[v] + " ";
            }
        }

        int n = UnityEngine.Random.Range(0, swordNoun.Count);

        generatedName = generatedName + swordNoun[n];

        if ((int)q == 3)
        {
            x = UnityEngine.Random.Range(0.0f, 100.0f);

            if (x > 50)
            {
                int t = UnityEngine.Random.Range(0, swordTitle.Count);
                generatedName = generatedName + " " + swordTitle[t];
            }
        }

        if ((int)q == 4)
        {
            int u = UnityEngine.Random.Range(0, swordUnique.Count);
            generatedName = swordUnique[u];
        }

        return generatedName;
    }

    /// <summary>
    /// Creates a sword name based on the quality of the item
    /// </summary>
    /// <param name="q">Quality to base the name on</param>
    /// <returns></returns>
    public string GetBowName(Quality q)
    {
        string generatedName = "";

        float x = UnityEngine.Random.Range(0.0f, 100.0f);

        if ((int)q > 0)
        {
            if (x > 60.0f)
            {
                int v = UnityEngine.Random.Range(0, bowVerb.Count);
                generatedName = generatedName + bowVerb[v] + " ";
            }
        }

        int n = UnityEngine.Random.Range(0, bowNoun.Count);

        generatedName = generatedName + bowNoun[n];

        if ((int)q == 3)
        {
            x = UnityEngine.Random.Range(0.0f, 100.0f);

            if (x > 50)
            {
                int t = UnityEngine.Random.Range(0, bowTitle.Count);
                generatedName = generatedName + " " + bowTitle[t];
            }
        }

        if ((int)q == 4)
        {
            int u = UnityEngine.Random.Range(0, bowUnique.Count);
            generatedName = bowUnique[u];
        }

        return generatedName;
    }

    /// <summary>
    /// Creates a sword name based on the quality of the item
    /// </summary>
    /// <param name="q">Quality to base the name on</param>
    /// <returns></returns>
    public string GetShieldName(Quality q)
    {
        string generatedName = "";

        float x = UnityEngine.Random.Range(0.0f, 100.0f);

        if ((int)q > 0)
        {
            if (x > 60.0f)
            {
                int v = UnityEngine.Random.Range(0, shieldVerb.Count);
                generatedName = generatedName + shieldVerb[v] + " ";
            }
        }

        int n = UnityEngine.Random.Range(0, shieldNoun.Count);

        generatedName = generatedName + shieldNoun[n];

        if ((int)q == 3)
        {
            x = UnityEngine.Random.Range(0.0f, 100.0f);

            if (x > 50)
            {
                int t = UnityEngine.Random.Range(0, shieldTitle.Count);
                generatedName = generatedName + " " + shieldTitle[t];
            }
        }

        if ((int)q == 4)
        {
            int u = UnityEngine.Random.Range(0, shieldUnique.Count);
            generatedName = shieldUnique[u];
        }

        return generatedName;
    }

    /// <summary>
    /// Creates an armor name based on the quality and material of the item
    /// </summary>
    /// <param name="q">Quality to base the name on</param>
    /// <returns></returns>
    public string GetArmorName(Quality q, int material, EquipmentSlot slot)
    {
        string generatedName = "";

        float x = UnityEngine.Random.Range(0.0f, 100.0f);

        if ((int)q > 0)
        {
            if (x > 60.0f)
            {
                if (material == 0)
                {
                    int v = UnityEngine.Random.Range(0, LeatherVerb.Count);
                    generatedName = generatedName + LeatherVerb[v] + " ";
                }
                else if (material == 1)
                {
                    int v = UnityEngine.Random.Range(0, metalVerb.Count);
                    generatedName = generatedName + metalVerb[v] + " ";
                }
                else if (material == 2)
                {
                    int v = UnityEngine.Random.Range(0, magicalVerb.Count);
                    generatedName = generatedName + magicalVerb[v] + " ";
                }

            }
        }

        if ((int)slot == 0)
        {
            int n = UnityEngine.Random.Range(0, helmNoun.Count);

            generatedName = generatedName + helmNoun[n];
        }

        else if ((int)slot == 1)
        {
            int n = UnityEngine.Random.Range(0, chestNoun.Count);

            generatedName = generatedName + chestNoun[n];
        }

        else if ((int)slot == 2)
        {
            int n = UnityEngine.Random.Range(0, legNoun.Count);

            generatedName = generatedName + legNoun[n];
        }

        else if ((int)slot == 5 || ((int)slot == 6))
        {
            int n = UnityEngine.Random.Range(0, footNoun.Count);

            generatedName = generatedName + footNoun[n];
        }

        else if ((int)slot == 7 || ((int)slot == 8))
        {
            int n = UnityEngine.Random.Range(0, handNoun.Count);

            generatedName = generatedName + handNoun[n];
        }

        else if ((int)slot == 9)
        {
            int n = UnityEngine.Random.Range(0, shoulderNoun.Count);

            generatedName = generatedName + shoulderNoun[n];
        }



        if ((int)q == 3)
        {
            x = UnityEngine.Random.Range(0.0f, 100.0f);

            if (x > 50)
            {
                int t = UnityEngine.Random.Range(0, armorTitle.Count);
                generatedName = generatedName + " " + armorTitle[t];
            }
        }

        if ((int)q == 4)
        {
            int u = UnityEngine.Random.Range(0, armorUnique.Count);
            generatedName = armorUnique[u];
        }

        return generatedName;

    }
}
