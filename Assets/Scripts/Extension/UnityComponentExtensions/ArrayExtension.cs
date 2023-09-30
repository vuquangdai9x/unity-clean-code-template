using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript.Utilities
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

        public static T GetRandomElement<T>(this T[] array)
        {
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Create an array which value = index, ranged from 0 to n-1
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int[] ArrayRange(int n)
        {
            int[] result = new int[n];
            for (int i = 0; i < n; i++) result[i] = i;
            return result;
        }

        public static T GetElementClamp<T>(this T[] array, int index) => array[Mathf.Clamp(index, 0, array.Length - 1)];

        /// <summary>
        /// Quick remove an element from list, by swap index with the last element.
        /// Use with !!CAUTION!! because it modify the last element and the element at index of list.
        /// </summary>
        public static void RemoveBySwapBack<T>(this List<T> list, int index)
        {
            list[index] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }
    }
}