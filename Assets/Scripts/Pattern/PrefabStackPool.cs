using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DaiVQScript
{
    [System.Serializable]
    public class PrefabStackPool<T> where T : Component
    {
        private T _prefab = null;
        private Transform _container = null;
        private Stack<T> _stack = null;
        private int _countMaxSpawned = 0;
        public int CountMaxSpawned => _countMaxSpawned;

        public PrefabStackPool(T prefab, Transform container, int capacity)
        {
            _prefab = prefab;
            _container = container;
            _stack = new Stack<T>(capacity);
            _countMaxSpawned = 0;
        }

        public void PrePool(int prePoolAmount)
        {
            for (int i = 0; i < prePoolAmount; i++)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _stack.Push(spawnedObjectComp);
            }
            _countMaxSpawned += prePoolAmount;
        }

        public IEnumerator IEPrePoolIterate(int prePoolAmount, int numSpawnPerFrame = 1)
        {
            for (int i = 0; i < prePoolAmount; i++)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _stack.Push(spawnedObjectComp);
                _countMaxSpawned += 1;
                if (i % numSpawnPerFrame == 0) yield return null;
            }
        }

        public void PrePoolMax(int maxPoolAmount)
        {
            if (maxPoolAmount > _countMaxSpawned)
            {
                PrePool(maxPoolAmount - _countMaxSpawned);
            }
        }

        public IEnumerator IEPrePoolMaxIterate(int maxPoolAmount, int numSpawnPerFrame = 1)
        {
            int counter = 0;
            while (_countMaxSpawned < maxPoolAmount)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _stack.Push(spawnedObjectComp);
                _countMaxSpawned += 1;

                counter += 1;
                if (counter % numSpawnPerFrame == 0) yield return null;
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
                _countMaxSpawned += 1;
            }
            return result;
        }

        /// <summary>
        /// Deactive game object and push to stack pool
        /// </summary>
        /// <param name="gameObjectComponent"></param>
        public void ReturnGameObject(T gameObjectComponent)
        {
            if (gameObjectComponent)
            {
                gameObjectComponent.gameObject.SetActive(false);
                _stack.Push(gameObjectComponent);
            }
        }

        public void DestroyPool()
        {
            while (_stack.Count > 0)
            {
                T objComp = _stack.Pop();
                if (objComp) Object.Destroy(objComp.gameObject);
            }
            _countMaxSpawned = 0;
        }
    }

    [System.Serializable]
    public class PrefabQueuePool<T> where T : Component
    {
        private T _prefab = null;
        private Transform _container = null;
        private Queue<T> _queue = null;
        private int _countMaxSpawned = 0;
        public int CountMaxSpawned => _countMaxSpawned;

        public PrefabQueuePool(T prefab, Transform container, int capacity)
        {
            _prefab = prefab;
            _container = container;
            _queue = new Queue<T>(capacity);
            _countMaxSpawned = 0;
        }

        public void PrePool(int prePoolAmount)
        {
            for (int i = 0; i < prePoolAmount; i++)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _queue.Enqueue(spawnedObjectComp);
            }
            _countMaxSpawned += prePoolAmount;
        }

        public IEnumerator IEPrePoolIterate(int prePoolAmount, int numSpawnPerFrame = 1)
        {
            for (int i = 0; i < prePoolAmount; i++)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _queue.Enqueue(spawnedObjectComp);
                _countMaxSpawned += 1;
                if (i % numSpawnPerFrame == 0) yield return null;
            }
        }

        public void PrePoolMax(int maxPoolAmount)
        {
            if (maxPoolAmount > _countMaxSpawned)
            {
                PrePool(maxPoolAmount - _countMaxSpawned);
            }
        }

        public IEnumerator IEPrePoolMaxIterate(int maxPoolAmount, int numSpawnPerFrame = 1)
        {
            int counter = 0;
            while (_countMaxSpawned < maxPoolAmount)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _queue.Enqueue(spawnedObjectComp);
                _countMaxSpawned += 1;

                counter += 1;
                if (counter % numSpawnPerFrame == 0) yield return null;
            }
        }

        public T UseGameObject()
        {
            T result = null;
            if (_queue.Count > 0)
            {
                result = _queue.Dequeue();
                result.gameObject.SetActive(true);
            }
            else
            {
                result = GameObject.Instantiate(_prefab, _container);
                result.gameObject.SetActive(true);
                _countMaxSpawned += 1;
            }
            return result;
        }

        /// <summary>
        /// Deactive game object and push to stack pool
        /// </summary>
        /// <param name="gameObjectComponent"></param>
        public void ReturnGameObject(T gameObjectComponent)
        {
            if (gameObjectComponent)
            {
                gameObjectComponent.gameObject.SetActive(false);
                _queue.Enqueue(gameObjectComponent);
            }
        }

        public void DestroyPool()
        {
            while (_queue.Count > 0)
            {
                T objComp = _queue.Dequeue();
                if (objComp) Object.Destroy(objComp.gameObject);
            }
            _countMaxSpawned = 0;
        }
    }

    [System.Serializable]
    public class AddressableQueuePool<T> where T : Component
    {
        private AssetReference _assetRef = null;
        private T _prefab = null;
        private Transform _container = null;
        private Queue<T> _queue = null;
        private int _countMaxSpawned = 0;
        public int CountMaxSpawned => _countMaxSpawned;

        public AddressableQueuePool(AssetReference assetRef, Transform container, int capacity)
        {
            _assetRef = assetRef;
            _container = container;
            _queue = new Queue<T>(capacity);
            _countMaxSpawned = 0;
        }

        public void PreloadAsset(System.Action onFinished)
        {
            AddressableUtils.LoadAssetByReference<GameObject>(_assetRef, OnLoadDoneAction);

            void OnLoadDoneAction(GameObject prefabObj)
            {
                _prefab = prefabObj.GetComponent<T>();
                onFinished?.Invoke();
            }
        }

        public void PrePool(int prePoolAmount)
        {
            if (!_prefab) return;

            for (int i = 0; i < prePoolAmount; i++)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _queue.Enqueue(spawnedObjectComp);
            }
            _countMaxSpawned += prePoolAmount;
        }

        public IEnumerator IEPrePoolIterate(int prePoolAmount, int numSpawnPerFrame = 1)
        {
            if (!_prefab) yield break;

            for (int i = 0; i < prePoolAmount; i++)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _queue.Enqueue(spawnedObjectComp);
                _countMaxSpawned += 1;
                if (i % numSpawnPerFrame == 0) yield return null;
            }
        }

        public void PrePoolMax(int maxPoolAmount)
        {
            if (!_prefab) return;

            if (maxPoolAmount > _countMaxSpawned)
            {
                PrePool(maxPoolAmount - _countMaxSpawned);
            }
        }

        public IEnumerator IEPrePoolMaxIterate(int maxPoolAmount, int numSpawnPerFrame = 1)
        {
            if (!_prefab) yield break;

            int counter = 0;
            while (_countMaxSpawned < maxPoolAmount)
            {
                T spawnedObjectComp = GameObject.Instantiate(_prefab, _container);
                spawnedObjectComp.gameObject.SetActive(false);
                _queue.Enqueue(spawnedObjectComp);
                _countMaxSpawned += 1;

                counter += 1;
                if (counter % numSpawnPerFrame == 0) yield return null;
            }
        }

        public T UseGameObject()
        {
            if (!_prefab) return null;

            T result = null;
            if (_queue.Count > 0)
            {
                result = _queue.Dequeue();
                result.gameObject.SetActive(true);
            }
            else
            {
                result = GameObject.Instantiate(_prefab, _container);
                result.gameObject.SetActive(true);
                _countMaxSpawned += 1;
            }
            return result;
        }

        /// <summary>
        /// Deactive game object and push to stack pool
        /// </summary>
        /// <param name="gameObjectComponent"></param>
        public void ReturnGameObject(T gameObjectComponent)
        {
            if (gameObjectComponent)
            {
                gameObjectComponent.gameObject.SetActive(false);
                _queue.Enqueue(gameObjectComponent);
            }
        }

        public void DestroyPool()
        {
            while (_queue.Count > 0)
            {
                T objComp = _queue.Dequeue();
                if (objComp) Object.Destroy(objComp.gameObject);
            }
            _countMaxSpawned = 0;
        }
    }
}