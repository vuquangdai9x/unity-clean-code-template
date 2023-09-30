using Game.Attributes;
using System.Collections;
using UnityEngine;

namespace Assets.Samples.Scripts
{
    public class AttributeSample : MonoBehaviour
    {
        public enum SampleEnum
        {
            FOO,
            BAR,
            XXX
        }
        [SerializeField, EnumMask(typeof(SampleEnum))] private int mask = 0;
        [SerializeField, ValueCurve] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}