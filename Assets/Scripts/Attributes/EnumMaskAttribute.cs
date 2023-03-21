using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Attributes
{
    public class EnumMaskAttribute : PropertyAttribute
    {
        public System.Type enumType;
        public int numCols = 4;
        public bool isShowList = false;
        public float buttonHeight = 25f;

        public EnumMaskAttribute(System.Type enumType, float buttonHeight = 25f, bool isShowList = false, int numCols = 5)
        {
            this.enumType = enumType;
            this.isShowList = isShowList;
            this.numCols = numCols;
            this.buttonHeight = buttonHeight;
        }
    }

    public static class EnumMaskUtility
    {
        public static List<T> GetEnumValues<T>(int mask) where T : System.Enum
        {
            T[] allValues = (T[])System.Enum.GetValues(typeof(T));
            List<T> result = new List<T>();
            for (int i = 0; i < allValues.Length && i < 32; i++)
            {
                if (((mask >> i) & 0x01) == 1)
                {
                    result.Add(allValues[i]);
                }
            }
            return result;
        }

        public static void ForeachEnum<T>(int mask, System.Action<T, bool> onIterate) where T : System.Enum
        {
            T[] allValues = (T[])System.Enum.GetValues(typeof(T));
            for (int i = 0; i < allValues.Length && i < 32; i++)
            {
                onIterate(
                    allValues[i],
                    ((mask >> i) & 0x01) == 1
                    );
            }
        }

        public static T GetRandomEnum<T>(int mask, T defaultValue) where T : System.Enum
        {
            T[] allValues = (T[])System.Enum.GetValues(typeof(T));
            List<T> result = new List<T>();
            for (int i = 0; i < allValues.Length && i < 32; i++)
            {
                if (((mask >> i) & 0x01) == 1)
                {
                    result.Add(allValues[i]);
                }
            }

            if (result.Count > 0)
            {
                return result[UnityEngine.Random.Range(0, result.Count)];
            }
            else
            {
                return defaultValue;
            }
        }
    }
}