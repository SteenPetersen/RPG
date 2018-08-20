using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class ExtMethods
{
    private static System.Random rng = new System.Random();

    /// <summary>
    /// Allows a List<T> to be shuffled so the elements in said list and positioned
    /// in a random order
    /// </summary>
    /// <typeparam name="T">The List to be shuffled</typeparam>
    /// <param name="list">Name of the list</param>
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

    /// <summary>
    /// A simple 
    /// </summary>
    /// <param name="vector">The vector that is to be changed</param>
    /// <param name="degrees">By how many degrees to we wish to rotate the direction</param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates a sinus curve parabola arc, insert a start Vector3, and end Vector3,
    /// a height for the jump of the Object and how long it should take to finish.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="height"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }


    /// <summary>
    /// Creates a sinus curve parabola arc, insert a start Vector2, and end Vector2,
    /// a height for the jump of the Object and how long it should take to finish.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="height"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
    }
}
