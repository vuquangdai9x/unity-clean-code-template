using System.Collections;
using UnityEngine;

namespace Game.Randomness
{
    [System.Serializable]
    public struct RandomRangeWithStep
    {
        [SerializeField] private float min;
        [SerializeField] private float max;
        [SerializeField] private float step;
        public float GetRandomValue()
        {
            if (step > 0f)
            {
                return UnityEngine.Random.Range(Mathf.CeilToInt(min / step), Mathf.FloorToInt(max / step) + 1) * step;
            }
            else
            {
                return UnityEngine.Random.Range(min, max);
            }
        }

        public RandomRangeWithStep(float min, float max, float modulo)
        {
            this.min = min;
            this.max = max;
            this.step = modulo;
        }

        public static RandomRangeWithStep Default => new RandomRangeWithStep(1, 1, -1);
        public static RandomRangeWithStep Constant(float value) => new RandomRangeWithStep(value, value, -1);
    }
}
