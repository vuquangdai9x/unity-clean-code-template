using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DesignPattern
{
    /// <summary>
    /// This type of pool require object that done used have to return to pool explicitly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class PrefabStackPool<T> where T : MonoBehaviour
    {
        private T _prefab = null;
        private Transform _container = null;
        private Stack<T> _stack = null;

        public PrefabStackPool(T prefab, Transform container, int capacity)
        {
            _prefab = prefab;
            _container = container;
            _stack = new Stack<T>(capacity);
        }

        public void PrePool(int prePoolAmount)
        {
            for (int i = 0; i < prePoolAmount; i++)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _stack.Push(spawnedObjectComp);
            }
        }

        public T UseGameObject()
        {
            T result = null;
            if (_stack.Count > 0)
            {
                result = _stack.Pop();
                result.gameObject.SetActive(true);
            }
            else
            {
                result = GameObject.Instantiate(_prefab, _container);
                result.gameObject.SetActive(true);
            }
            return result;
        }

        public void ReturnGameObject(T gameObjectComponent)
        {
            gameObjectComponent.gameObject.SetActive(false);
            _stack.Push(gameObjectComponent);
        }
    }
}