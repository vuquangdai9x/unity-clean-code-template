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

        public static T[] Shuffle<T>(this T[] array, int seed, int data = 0)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                int r = i + Randomness.RandomUtility.GetRange(0, n - i, seed, data);
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

        public static T GetRandomElement<T>(this T[] array, int seed, int data = 0)
        {
            return array[Randomness.RandomUtility.GetRange(0, array.Length, seed, data)];
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
    }
}