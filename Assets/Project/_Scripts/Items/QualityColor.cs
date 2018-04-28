using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Quality {  Common, UnCommon, Rare, Epic, Legendary  }

public static class QualityColor {

    private static Dictionary<Quality, string> colors = new Dictionary<Quality, string>()
    {
        {Quality.Common, "#ffffffff" },
        {Quality.UnCommon, "#00ff00ff" },
        {Quality.Rare, "#0E6BECFF" },
        {Quality.Epic, "#A712DBFF" },
        {Quality.Legendary, "#ffffffff" }


    };

    public static Dictionary<Quality, string> MyColors
    {
        get
        {
            return colors;
        }
    }
}
