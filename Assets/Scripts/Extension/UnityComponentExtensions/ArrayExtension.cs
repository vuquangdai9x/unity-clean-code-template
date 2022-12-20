using System.Collections;
using UnityEngine;

namespace Game.Extension
{
    public static class ArrayExtension
    {
        public static T[] Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                int r = i + Random.Range(0, n - i);
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
            return array;
        }
    }
}