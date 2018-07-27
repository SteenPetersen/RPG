using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Quality {  Common, UnCommon, Rare, Epic, Legendary  }

public static class QualityColor {

    private static Dictionary<Quality, string> colors = new Dictionary<Quality, string>()
    {
        {Quality.Common, "#cecece" },
        {Quality.UnCommon, "#00ff00ff" },
        {Quality.Rare, "#7b87ff" },
        {Quality.Epic, "#9e7afa" },
        {Quality.Legendary, "#af1717" }


    };

    public static Dictionary<Quality, string> MyColors
    {
        get
        {
            return colors;
        }
    }
}
