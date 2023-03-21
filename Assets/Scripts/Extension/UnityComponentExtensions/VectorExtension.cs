using System.Collections;
using UnityEngine;

namespace Game.Extension
{
    public static class VectorExtension
    {
        public static float AngleFromRightVector(this Vector2 vector2)
        {
            return 360 - (Mathf.Atan2(vector2.x, vector2.y) * Mathf.Rad2Deg * Mathf.Sign(vector2.x));
        }
    }
}