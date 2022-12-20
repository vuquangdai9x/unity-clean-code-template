using System.Collections;
using UnityEngine;

namespace Game.Randomness
{
    public static class RandomUtility
    {
        #region Primitive random
        public static int GetRandomInt => UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        public static int GetInt(int data, int seed)
        {
            return (int)Klak.Math.XXHash.GetHash(data, seed);
        }

        public static bool GetRandomBool(int data, int seed)
        {
            return GetInt(data, seed) % 2 == 0;
        }

        public static float GetValue01(int data, int seed)
        {
            return Klak.Math.XXHash.GetValue01(data, seed);
        }

        public static int GetRange(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        public static float GetRange(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static int GetRange(int min, int max, int data, int seed)
        {
            return Klak.Math.XXHash.GetRange(data, min, max, seed);
        }

        public static float GetRange(float min, float max, int data, int seed)
        {
            return Klak.Math.XXHash.GetRange(data, min, max, seed);
        }
        #endregion

        #region Random with weight
        public static int GetWeightedRandomIndex(int[] weights)
        {
            float sumWeight = 0;
            for (int i = 0; i < weights.Length; i++) sumWeight += weights[i];

            float randomWeight = sumWeight * UnityEngine.Random.value;

            float accumulatedWeight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] == 0) continue;
                accumulatedWeight += weights[i];
                if (accumulatedWeight > randomWeight) return i;
            }

            return -1;
        }

        public static int GetWeightedRandomIndex(int[] weights, int data, int seed)
        {
            float sumWeight = 0;
            for (int i = 0; i < weights.Length; i++) sumWeight += weights[i];

            float randomWeight = sumWeight * Klak.Math.XXHash.GetValue01(data, seed);

            float accumulatedWeight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] == 0) continue;
                accumulatedWeight += weights[i];
                if (accumulatedWeight > randomWeight) return i;
            }

            return -1;
        }

        public static int[] GetWeightedRandomListIndexs(int amount, int[] weights, int data, int seed)
        {
            float sumWeight = 0;
            for (int i = 0; i < weights.Length; i++) sumWeight += weights[i];

            int[] result = new int[amount];
            for (int resultIndex = 0; resultIndex < amount; resultIndex++)
            {
                float randomWeight = sumWeight * Klak.Math.XXHash.GetValue01(data + resultIndex, seed);
                float accumulatedWeight = 0;
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights[i] == 0) continue;
                    accumulatedWeight += weights[i];
                    if (accumulatedWeight > randomWeight)
                    {
                        result[resultIndex] = i;
                        break;
                    }
                }
            }

            return result;
        }

        public static int GetWeightedRandomIndex(float[] weights)
        {
            float sumWeight = 0;
            for (int i = 0; i < weights.Length; i++) sumWeight += weights[i];

            float randomWeight = sumWeight * UnityEngine.Random.value;

            float accumulatedWeight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] == 0f) continue;
                accumulatedWeight += weights[i];
                if (accumulatedWeight > randomWeight) return i;
            }

            return -1;
        }

        public static int GetWeightedRandomIndex(float[] weights, int data, int seed)
        {
            float sumWeight = 0;
            for (int i = 0; i < weights.Length; i++) sumWeight += weights[i];

            float randomWeight = sumWeight * Klak.Math.XXHash.GetValue01(data, seed);

            float accumulatedWeight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] == 0f) continue;
                accumulatedWeight += weights[i];
                if (accumulatedWeight > randomWeight) return i;
            }

            return -1;
        }

        public static int[] GetWeightedRandomListIndexs(int amount, float[] weights, int data, int seed)
        {
            float sumWeight = 0;
            for (int i = 0; i < weights.Length; i++) sumWeight += weights[i];

            int[] result = new int[amount];
            for (int resultIndex = 0; resultIndex < amount; resultIndex++)
            {
                float randomWeight = sumWeight * Klak.Math.XXHash.GetValue01(data + resultIndex, seed);
                float accumulatedWeight = 0;
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights[i] == 0) continue;
                    accumulatedWeight += weights[i];
                    if (accumulatedWeight > randomWeight)
                    {
                        result[resultIndex] = i;
                        break;
                    }
                }
            }

            return result;
        }

        public static int[] GetShuffledIndexArray(int length, int data, int seed)
        {
            // shuffer indexs - O(n)
            int[] indexArray = new int[length];
            for (int i = 0; i < length; i++) indexArray[i] = i;
            int randomPickIndex, swapValue;
            for (int pickCount = 0; pickCount < length - 1; pickCount++)
            {
                randomPickIndex = Klak.Math.XXHash.GetRange(data + pickCount, pickCount, length, seed);
                if (randomPickIndex != pickCount)
                {
                    swapValue = indexArray[pickCount];
                    indexArray[pickCount] = indexArray[randomPickIndex];
                    indexArray[randomPickIndex] = swapValue;
                }
            }
            return indexArray;
        }

        public static int[] GetShuffledIndexArray(int length)
        {
            // shuffer indexs - O(n)
            int[] indexArray = new int[length];
            for (int i = 0; i < length; i++) indexArray[i] = i;
            int randomPickIndex, swapValue;
            for (int pickCount = 0; pickCount < length - 1; pickCount++)
            {
                randomPickIndex = UnityEngine.Random.Range(pickCount, length);
                if (randomPickIndex != pickCount)
                {
                    swapValue = indexArray[pickCount];
                    indexArray[pickCount] = indexArray[randomPickIndex];
                    indexArray[randomPickIndex] = swapValue;
                }
            }
            return indexArray;
        }
        #endregion
    }
}