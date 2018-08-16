using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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

    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Debug.Log("!!!");

        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }

    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        Debug.Log("!!");

        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
    }




}
