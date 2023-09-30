using System.Collections;
using UnityEngine;

namespace DaiVQScript.Utilities
{
    public static class RandomUtility
    {
        public static int GetRandomInt => UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        public static int GetHash(int data, int seed)
        {
            return (int)Klak.Math.XXHash.GetHash(data, seed);
        }

        public static bool GetRandomBool(int data, int seed)
        {
            return GetHash(data, seed) % 2 == 0;
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
            // shuffle indexs - O(n)
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

        ///// <summary>
        ///// Complexity: O(n)
        ///// </summary>
        //public static int[] GetSegmentalShuffledIndexArray(int length, int segmentLength, bool isUseSecondLoop = true)
        //{
        //    int[] indexArray = new int[length];
        //    for (int i = 0; i < length; i++) indexArray[i] = i;

        //    int numFullSegments = length / segmentLength;
        //    int remainderSegments = length % segmentLength;

        //    int index, randomPickIndex, tmpSwapValue;

        //    for (int fullSegmentIndex = 0; fullSegmentIndex < numFullSegments; fullSegmentIndex++)
        //    {
        //        for (int pickCount = 0; pickCount < segmentLength; pickCount++)
        //        {
        //            index = fullSegmentIndex * segmentLength + pickCount;
        //            randomPickIndex = fullSegmentIndex * segmentLength + UnityEngine.Random.Range(pickCount, segmentLength);
        //            if (randomPickIndex != index)
        //            {
        //                tmpSwapValue = indexArray[index];
        //                indexArray[index] = indexArray[randomPickIndex];
        //                indexArray[randomPickIndex] = tmpSwapValue;
        //            }
        //        }
        //    }

        //    for (int pickCount = 0; pickCount < remainderSegments; pickCount++)
        //    {
        //        index = length - remainderSegments + pickCount;
        //        randomPickIndex = length - remainderSegments + UnityEngine.Random.Range(pickCount, segmentLength);
        //        if (randomPickIndex != index)
        //        {
        //            tmpSwapValue = indexArray[index];
        //            indexArray[index] = indexArray[randomPickIndex];
        //            indexArray[randomPickIndex] = tmpSwapValue;
        //        }
        //    }

        //    int offset = segmentLength / 2;
        //    if (isUseSecondLoop)
        //    {
        //        numFullSegments = (length - offset) / segmentLength;
        //        remainderSegments = (length - offset) - segmentLength * numFullSegments;

        //        for (int fullSegmentIndex = 0; fullSegmentIndex < numFullSegments; fullSegmentIndex++)
        //        {
        //            for (int pickCount = 0; pickCount < segmentLength; pickCount++)
        //            {
        //                index = offset + fullSegmentIndex * segmentLength + pickCount;
        //                randomPickIndex = offset + fullSegmentIndex * segmentLength + UnityEngine.Random.Range(pickCount, segmentLength);
        //                if (randomPickIndex != index)
        //                {
        //                    tmpSwapValue = indexArray[index];
        //                    indexArray[index] = indexArray[randomPickIndex];
        //                    indexArray[randomPickIndex] = tmpSwapValue;
        //                }
        //            }
        //        }

        //        for (int pickCount = 0; pickCount < remainderSegments; pickCount++)
        //        {
        //            index = offset + segmentLength - remainderSegments + pickCount;
        //            randomPickIndex = offset + segmentLength - remainderSegments + UnityEngine.Random.Range(pickCount, segmentLength);
        //            if (randomPickIndex != index)
        //            {
        //                tmpSwapValue = indexArray[index];
        //                indexArray[index] = indexArray[randomPickIndex];
        //                indexArray[randomPickIndex] = tmpSwapValue;
        //            }
        //        }
        //    }

        //    return indexArray;
        //}

        /// <summary>
        /// Complexity: O(n*n) : iterating to generate displacement arrays, then bubble sort
        /// </summary>
        public static int[] GetDisplacementalShuffledIndexArray(int length, float displacementStrength, float randomFrequency = 1)
        {
            // shuffle indexs - O(n)
            int[] indexArray = new int[length];
            float[] displacedValueArrays = new float[length];
            for (int i = 0; i < length; i++)
            {
                indexArray[i] = i;
                displacedValueArrays[i] = i + displacementStrength * Mathf.PerlinNoise(i * randomFrequency, 0f);
            }

            int tmpIndex;
            float tmpDisplacedValue;
            for (int i = length - 1; i > 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (displacedValueArrays[i] < displacedValueArrays[j])
                    {
                        tmpIndex = indexArray[i];
                        indexArray[i] = indexArray[j];
                        indexArray[j] = tmpIndex;

                        tmpDisplacedValue = displacedValueArrays[i];
                        displacedValueArrays[i] = displacedValueArrays[j];
                        displacedValueArrays[j] = tmpDisplacedValue;
                    }
                }
            }

            return indexArray;
        }

        #region Position
        public static Vector2[] GetRandomPositionsInDividedAreaNormalized(Vector2Int division, Vector2 randomness, int numPoints)
        {
            Vector2[] positions = new Vector2[numPoints];

            int[] shuffledAreaIndex = GetShuffledIndexArray(division.x * division.y);

            for (int i = 0; i < numPoints; i++)
            {
                int areaIndex = shuffledAreaIndex[i];
                int areaX = areaIndex % division.x;
                int areaY = areaIndex / division.x;

                positions[i].x = (areaX + 0.5f + 0.5f * Random.Range(-randomness.x, randomness.x)) / division.x;
                positions[i].y = (areaY + 0.5f + 0.5f * Random.Range(-randomness.y, randomness.y)) / division.y;
            }

            return positions;
        }

        public static Vector2[] GetRandomPositionsInDividedArea(Rect area, Vector2Int division, Vector2 randomness, int numPoints)
        {
            Vector2[] positions = GetRandomPositionsInDividedAreaNormalized(division, randomness, numPoints);
            for (int i = 0; i < numPoints; i++)
            {
                positions[i].x = area.xMin + area.width * positions[i].x;
                positions[i].y = area.yMin + area.height * positions[i].y;
            }
            return positions;
        }
        #endregion

        #region Range & Randomness by index
        /// <summary>
        /// Return a value ranged from 0 to 1
        /// </summary>
        public static float GetRandomProgressNormByIndex(int index, int total, float randomness)
        {
            return (index + 0.5f + randomness * UnityEngine.Random.Range(-0.5f, 0.5f)) / total;
        }

        public static float GetRandomValueInsideRangeByIndex(int index, int total, float min, float max, float randomness)
        {
            return Mathf.LerpUnclamped(min, max, (index + 0.5f + randomness * UnityEngine.Random.Range(-0.5f, 0.5f)) / total);
        }

        public static float GetRandomValueInsideRangeByIndex(int index, int total, Vector2 range, float randomness)
        {
            return Mathf.LerpUnclamped(range.x, range.y, (index + 0.5f + randomness * UnityEngine.Random.Range(-0.5f, 0.5f)) / total);
        }

        public static float GetRandomAngleInsideRangeByIndex(int index, int total, float angleRange, float randomness)
        {
            return angleRange * ((index + 0.5f + randomness * UnityEngine.Random.Range(-0.5f, 0.5f)) / total - 0.5f);
        }
        #endregion

        #region Simple Random
        public static float GetRandomWithVariant(float baseValue, float variant) => baseValue + UnityEngine.Random.Range(-variant, +variant);
        public static int GetRandomWithVariant(int baseValue, int variant) => baseValue + UnityEngine.Random.Range(-variant, +variant + 1);
        #endregion
    }
}