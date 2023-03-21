using UnityEngine;

namespace Game.Attributes
{
    /// <summary>
    /// This attribute is to show min and max value of the curve
    /// </summary>
    public class ValueCurveAttribute : PropertyAttribute
    {
        //public Rect range;
        //public float minKey => range.xMin;
        //public float maxKey => range.xMax;
        //public float minValue => range.yMin;
        //public float maxValue => range.yMax;

        //public ValueCurveAttribute(float minKey = float.MinValue, float maxKey = float.MaxValue, float minValue = float.MinValue, float maxValue = float.MaxValue)
        //{
        //    range = new Rect(minKey, minValue, maxKey - minKey, maxValue - minValue);
        //}
        public ValueCurveAttribute()
        {
        }
    }
}