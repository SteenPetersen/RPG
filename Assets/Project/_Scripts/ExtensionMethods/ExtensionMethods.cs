using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ExtMethods
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Vector2 Rotate(this Vector2 vector, float degrees)
    {
         float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
         float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
         
         float vectorX = vector.x;
         float vectorY = vector.y;

         vector.x = (cos * vectorX) - (sin * vectorY);
         vector.y = (sin * vectorX) + (cos * vectorY);

         return vector;
    }




}
