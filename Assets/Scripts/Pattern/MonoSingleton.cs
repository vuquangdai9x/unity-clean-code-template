using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DesignPattern
{
    /// <summary>
    /// This singleton need pre instantiated object in scene
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance => _instance;

        public static bool IsExistInstance => _instance != null;

        protected virtual void Awake()
        {
            if (IsExistInstance)
            {
                GameObject obj = this.gameObject;
                Destroy(this);
                Destroy(obj);
                return;
            }
            else
            {
                _instance = this as T;
            }
        }

        protected virtual void OnDestroy()
        {
            if (this == _instance)
            {
                _instance = null;
            }
        }
    }
}
