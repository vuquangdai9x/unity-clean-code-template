using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.DesignPattern
{
    /// <summary>
    /// This kind of pool is convenient because of not required return object after finished using, with trade-off is some performance.
    /// It iterate through an array to detect which object is inactive then return them.
    /// For more efficient, use stack pool or queue pool instead.
    /// </summary>
    public class ObjectPoolDictArray : MonoSingleton<ObjectPoolDictArray>
    {
        private List<int> _pooledKeyList = new List<int>();
        private Dictionary<int, List<GameObject>> _pooledGoDic = new Dictionary<int, List<GameObject>>();

        /// <summary>
        /// Cache transform.
        /// </summary>
        protected Transform _Transform
        {
            get
            {
                if (_transform == null)
                {
                    _transform = this.transform;
                }
                return _transform;
            }
        }
        private Transform _transform = null;

        #region PrePooling
        // use prepool for better performance

        /// <summary>
        /// Init when scene start, so it doesn't take too much time when create object while playing
        /// </summary>
        public void InitGameObjects(GameObject prefab, int amount)
        {
            //PrePooling(prefab, amount, -1, null);
            if (prefab == null)
            {
                return;
            }

            int key = prefab.GetInstanceID();

            if (_pooledKeyList.Contains(key) == false && _pooledGoDic.ContainsKey(key) == false)
            {
                _pooledKeyList.Add(key);
                _pooledGoDic.Add(key, new List<GameObject>());
            }

            List<GameObject> goList = _pooledGoDic[key];
            Transform _transform = transform;

            for (int i = 0; i < amount; i++)
            {
                GameObject go = (GameObject)Instantiate(prefab, _transform);
                goList.Add(go);
                go.SetActive(false);
            }
        }

        private Dictionary<int, Coroutine> _dictCorPrePooling = new Dictionary<int, Coroutine>();
        /// <summary>
        /// Set amountSpawnEachFrame=-1 for instant pre-pooling
        /// </summary>
        public void PrePooling(GameObject prefab, int amount, int amountSpawnEachFrame, System.Action onEnd = null)
        {
            if (prefab == null || amount <= 0)
            {
                onEnd?.Invoke();
                return;
            }

            int key = prefab.GetInstanceID();

            List<GameObject> listObject = null;
            if (_pooledGoDic.TryGetValue(key, out listObject))
            {
                listObject.Capacity = Mathf.Max(listObject.Capacity, listObject.Count + amount);
            }
            else
            {
                listObject = new List<GameObject>(amount);
                _pooledKeyList.Add(key);
                _pooledGoDic.Add(key, listObject);
            }

            if (amountSpawnEachFrame > 0)
            {
                // iterated spawn
                if (_dictCorPrePooling.ContainsKey(key))
                {
                    Coroutine prevCor = _dictCorPrePooling[key];
                    if (prevCor != null) StopCoroutine(prevCor);
                    _dictCorPrePooling[key] = StartCoroutine(IEPrePooling(prefab, amount, amountSpawnEachFrame, onEnd));
                }
                else
                {
                    _dictCorPrePooling.Add(key, StartCoroutine(IEPrePooling(prefab, amount, amountSpawnEachFrame, onEnd)));
                }
            }
            else
            {
                // instant spawn
                InstantPrePooling(prefab, amount);
                onEnd?.Invoke();
            }
        }
        public void PrePoolingMax(GameObject prefab, int maxAmount, int amountSpawnEachFrame, System.Action onEnd = null)
        {
            int currentAmount = CountSpawnedObjects(prefab);
            PrePooling(prefab, maxAmount - currentAmount, amountSpawnEachFrame, onEnd);
        }
        private IEnumerator IEPrePooling(GameObject prefab, int amount, int amountSpawnEachFrame, System.Action onEnd = null)
        {
            int numSteps = amount / amountSpawnEachFrame;
            for (int i = 0; i < numSteps; i++)
            {
                InstantPrePooling(prefab, amountSpawnEachFrame);
                yield return null;
            }
            int mod = amount % amountSpawnEachFrame;
            if (mod > 0) InstantPrePooling(prefab, mod);
            onEnd?.Invoke();
        }
        private void InstantPrePooling(GameObject prefab, int amount)
        {
            int key = prefab.GetInstanceID();
            List<GameObject> goList = _pooledGoDic[key];
            GameObject go = null;
            for (int i = 0; i < amount; i++)
            {
                go = (GameObject)Instantiate(prefab, _Transform);
                go.SetActive(false);
                goList.Add(go);
            }
        }
        #endregion

        #region Get One
        /// <summary>
        /// Get GameObject from object pool or instantiate.
        /// </summary>
        public GameObject GetGameObject(GameObject prefab, Vector3 position, Quaternion rotation, bool forceInstantiate = false)
        {
            if (prefab == null)
            {
                return null;
            }

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                GameObject objectEditor = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                objectEditor.transform.position = position;
                objectEditor.transform.rotation = rotation;
                return objectEditor;
            }
#endif

            int key = prefab.GetInstanceID();

            if (_pooledKeyList.Contains(key) == false && _pooledGoDic.ContainsKey(key) == false)
            {
                _pooledKeyList.Add(key);
                _pooledGoDic.Add(key, new List<GameObject>());
            }

            List<GameObject> goList = _pooledGoDic[key];
            GameObject go = null;

            if (forceInstantiate == false)
            {
                for (int i = goList.Count - 1; i >= 0; i--)
                {
                    go = goList[i];
                    if (go == null)
                    {
                        goList.Remove(go);
                        continue;
                    }
                    if (go.activeSelf == false)
                    {
                        // Found free GameObject in object pool.
                        Transform goTransform = go.transform;
                        goTransform.position = position;
                        goTransform.rotation = rotation;
                        go.SetActive(true);
                        return go;
                    }
                }
            }

            // Instantiate because there is no free GameObject in object pool.
            go = (GameObject)Instantiate(prefab, position, rotation);
            go.transform.SetParent(transform);
            goList.Add(go);

            return go;
        }
        public GameObject GetGameObject(GameObject prefab, bool forceInstantiate = false)
        {
            if (prefab == null)
            {
                return null;
            }


#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                GameObject objectEditor = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                return objectEditor;
            }
#endif

            int key = prefab.GetInstanceID();

            if (_pooledKeyList.Contains(key) == false && _pooledGoDic.ContainsKey(key) == false)
            {
                _pooledKeyList.Add(key);
                _pooledGoDic.Add(key, new List<GameObject>());
            }

            List<GameObject> goList = _pooledGoDic[key];
            GameObject go = null;

            if (forceInstantiate == false)
            {
                for (int i = goList.Count - 1; i >= 0; i--)
                {
                    go = goList[i];
                    if (go == null)
                    {
                        goList.Remove(go);
                        continue;
                    }
                    if (go.activeSelf == false)
                    {
                        // Found free GameObject in object pool.
                        Transform goTransform = go.transform;
                        go.SetActive(true);
                        return go;
                    }
                }
            }

            // Instantiate because there is no free GameObject in object pool.
            go = (GameObject)Instantiate(prefab);
            go.transform.SetParent(transform);
            goList.Add(go);

            return go;
        }

        public T GetGameObject<T>(T prefabComponent, Vector3 position, Quaternion rotation, bool forceInstantiate = false) where T : MonoBehaviour
        {
            if (prefabComponent == null) return null;
            GameObject go = GetGameObject(prefabComponent.gameObject, position, rotation, forceInstantiate);
            return go.GetComponent<T>();
        }

        public ParticleSystem GetParticle(ParticleSystem particle, Vector3 position, Quaternion rotation, bool forceInstantiate = false)
        {
            if (particle == null) return null;
            GameObject go = GetGameObject(particle.gameObject, position, rotation, forceInstantiate);
            return go.GetComponent<ParticleSystem>();
        }

        public ParticleSystem GetParticle(ParticleSystem particle, Vector3 position)
        {
            if (particle == null) return null;
            GameObject go = GetGameObject(particle.gameObject, position, particle.transform.rotation);
            return go.GetComponent<ParticleSystem>();
        }

        /// <summary>
        /// To use this, particle must set main.StopAction = Disable
        /// </summary>
        public void PlayParticleOnce(ParticleSystem particle, Vector3 position, float scale = -1)
        {
            if (particle == null) return;
            GameObject go = GetGameObject(particle.gameObject, position, particle.transform.rotation);
            if (scale > 0f)
            {
                go.transform.localScale = particle.transform.localScale * scale;
            }
            go.GetComponent<ParticleSystem>().Play();
        }

        /// <summary>
        /// To use this, particle must set main.StopAction = Disable
        /// </summary>
        public void PlayParticleOnce(ParticleSystem particle, Vector3 position, Quaternion rotation, float scale = -1)
        {
            if (particle == null) return;
            GameObject go = GetGameObject(particle.gameObject, position, rotation);
            if (scale > 0f)
            {
                go.transform.localScale = particle.transform.localScale * scale;
            }
            go.GetComponent<ParticleSystem>().Play();
        }

        public LineRenderer GetLine(LineRenderer line, Vector3 position, Quaternion rotation, bool forceInstantiate = false)
        {
            if (line == null) return null;
            GameObject go = GetGameObject(line.gameObject, position, rotation, forceInstantiate);
            return go.GetComponent<LineRenderer>();
        }

        public T GetGameObject<T>(T prefabComponent, bool forceInstantiate = false) where T : MonoBehaviour
        {
            if (prefabComponent == null) return null;
            GameObject go = GetGameObject(prefabComponent.gameObject, forceInstantiate);
            return go.GetComponent<T>();
        }

        public ParticleSystem GetParticle(ParticleSystem particle, bool forceInstantiate = false)
        {
            if (particle == null) return null;
            GameObject go = GetGameObject(particle.gameObject, forceInstantiate);
            return go.GetComponent<ParticleSystem>();
        }

        public LineRenderer GetLine(LineRenderer line, bool forceInstantiate = false)
        {
            if (line == null) return null;
            GameObject go = GetGameObject(line.gameObject, forceInstantiate);
            return go.GetComponent<LineRenderer>();
        }
        #endregion

        #region Get Many
        public List<GameObject> GetGameObjects(GameObject prefab, int amount, bool forceInstantiate = false)
        {
            if (prefab == null)
            {
                return null;
            }

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                List<GameObject> objectsEditor = new List<GameObject>();
                for (int i = 0; i < amount; i++)
                {
                    objectsEditor.Add((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab));
                }
                return objectsEditor;
            }
#endif

            int key = prefab.GetInstanceID();

            if (_pooledKeyList.Contains(key) == false && _pooledGoDic.ContainsKey(key) == false)
            {
                _pooledKeyList.Add(key);
                _pooledGoDic.Add(key, new List<GameObject>());
            }

            List<GameObject> goList = _pooledGoDic[key];
            List<GameObject> listReadyGameObject = new List<GameObject>();

            GameObject go = null;

            if (!forceInstantiate)
            {
                for (int i = goList.Count - 1; i >= 0 && listReadyGameObject.Count < amount; i--)
                {
                    go = goList[i];
                    if (go == null)
                    {
                        goList.Remove(go);
                        continue;
                    }
                    if (go.activeSelf == false)
                    {
                        // Found free GameObject in object pool.
                        go.SetActive(true);
                        listReadyGameObject.Add(go);
                    }
                }
            }

            // extend list capacity
            goList.Capacity = Mathf.Max(goList.Capacity, goList.Count + (amount - listReadyGameObject.Count));

            while (listReadyGameObject.Count < amount)
            {
                // Instantiate because there is no free GameObject in object pool.
                go = (GameObject)Instantiate(prefab);
                go.SetActive(true);
                go.transform.SetParent(transform);
                goList.Add(go);

                listReadyGameObject.Add(go);
            }

            return listReadyGameObject;
        }
        public List<GameObject> GetGameObjects(GameObject prefab, int amount, Vector3 position, Quaternion rotation, bool forceInstantiate = false)
        {
            if (prefab == null)
            {
                return null;
            }

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                List<GameObject> objectsEditor = new List<GameObject>();
                for (int i = 0; i < amount; i++)
                {
                    objectsEditor.Add((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab));
                }
                return objectsEditor;
            }
#endif

            int key = prefab.GetInstanceID();

            if (_pooledKeyList.Contains(key) == false && _pooledGoDic.ContainsKey(key) == false)
            {
                _pooledKeyList.Add(key);
                _pooledGoDic.Add(key, new List<GameObject>());
            }

            List<GameObject> goList = _pooledGoDic[key];
            List<GameObject> listReadyGameObject = new List<GameObject>();

            GameObject go = null;

            if (!forceInstantiate)
            {
                for (int i = goList.Count - 1; i >= 0 && listReadyGameObject.Count < amount; i--)
                {
                    go = goList[i];
                    if (go == null)
                    {
                        goList.Remove(go);
                        continue;
                    }
                    if (go.activeSelf == false)
                    {
                        // Found free GameObject in object pool.
                        Transform goTransform = go.transform;
                        goTransform.position = position;
                        goTransform.rotation = rotation;
                        go.SetActive(true);
                        listReadyGameObject.Add(go);
                    }
                }
            }

            // extend list capacity
            goList.Capacity = Mathf.Max(goList.Capacity, goList.Count + (amount - listReadyGameObject.Count));

            while (listReadyGameObject.Count < amount)
            {
                // Instantiate because there is no free GameObject in object pool.
                go = (GameObject)Instantiate(prefab);
                Transform goTransform = go.transform;
                goTransform.position = position;
                goTransform.rotation = rotation;
                go.SetActive(true);
                go.transform.SetParent(transform);
                goList.Add(go);

                listReadyGameObject.Add(go);
            }

            return listReadyGameObject;
        }

        public List<T> GetGameObjects<T>(T prefabComponent, int amount, bool forceInstantiate = false) where T : MonoBehaviour
        {
            if (prefabComponent == null) return null;
            List<GameObject> gos = GetGameObjects(prefabComponent.gameObject, amount, forceInstantiate);
            List<T> components = new List<T>();
            for (int i = 0; i < gos.Count; i++)
            {
                components.Add(gos[i].GetComponent<T>());
            }
            return components;
        }
        public List<ParticleSystem> GetParticles(ParticleSystem particle, int amount, bool forceInstantiate = false)
        {
            if (particle == null) return null;
            List<GameObject> gos = GetGameObjects(particle.gameObject, amount, forceInstantiate);
            List<ParticleSystem> components = new List<ParticleSystem>();
            for (int i = 0; i < gos.Count; i++)
            {
                components.Add(gos[i].GetComponent<ParticleSystem>());
            }
            return components;
        }
        public List<LineRenderer> GetLines(LineRenderer line, int amount, bool forceInstantiate = false)
        {
            if (line == null) return null;
            List<GameObject> gos = GetGameObjects(line.gameObject, amount, forceInstantiate);
            List<LineRenderer> components = new List<LineRenderer>();
            for (int i = 0; i < gos.Count; i++)
            {
                components.Add(gos[i].GetComponent<LineRenderer>());
            }
            return components;
        }

        public List<T> GetGameObjects<T>(T prefabComponent, int amount, Vector3 position, Quaternion rotation, bool forceInstantiate = false) where T : MonoBehaviour
        {
            if (prefabComponent == null) return null;
            List<GameObject> gos = GetGameObjects(prefabComponent.gameObject, amount, position, rotation, forceInstantiate);
            List<T> components = new List<T>();
            for (int i = 0; i < gos.Count; i++)
            {
                components.Add(gos[i].GetComponent<T>());
            }
            return components;
        }
        public List<ParticleSystem> GetParticles(ParticleSystem particle, int amount, Vector3 position, Quaternion rotation, bool forceInstantiate = false)
        {
            if (particle == null) return null;
            List<GameObject> gos = GetGameObjects(particle.gameObject, amount, position, rotation, forceInstantiate);
            List<ParticleSystem> components = new List<ParticleSystem>();
            for (int i = 0; i < gos.Count; i++)
            {
                components.Add(gos[i].GetComponent<ParticleSystem>());
            }
            return components;
        }
        public List<LineRenderer> GetLines(LineRenderer line, int amount, Vector3 position, Quaternion rotation, bool forceInstantiate = false)
        {
            if (line == null) return null;
            List<GameObject> gos = GetGameObjects(line.gameObject, amount, position, rotation, forceInstantiate);
            List<LineRenderer> components = new List<LineRenderer>();
            for (int i = 0; i < gos.Count; i++)
            {
                components.Add(gos[i].GetComponent<LineRenderer>());
            }
            return components;
        }
        #endregion

        #region Get Iteratedly
        public Coroutine GetGameObjectsIterate(GameObject prefab, int amount, int amountEachFrame, System.Action<GameObject> onGetObject, System.Action onFinished = null)
        {
            if (prefab == null || onGetObject == null)
            {
                onFinished?.Invoke();
                return null;
            }

            int key = prefab.GetInstanceID();
            int countReadyObjects = 0;

            if (_pooledKeyList.Contains(key) == false && _pooledGoDic.ContainsKey(key) == false)
            {
                _pooledKeyList.Add(key);
                _pooledGoDic.Add(key, new List<GameObject>());
            }

            List<GameObject> goList = _pooledGoDic[key];

            GameObject go = null;

            for (int i = goList.Count - 1; i >= 0 && countReadyObjects < amount; i--)
            {
                go = goList[i];
                if (go == null)
                {
                    goList.Remove(go);
                    continue;
                }
                if (go.activeSelf == false)
                {
                    // Found free GameObject in object pool.
                    go.SetActive(true);
                    onGetObject?.Invoke(go);
                    countReadyObjects += 1;
                }
            }

            if (countReadyObjects < amount)
            {
                return StartCoroutine(IESpawnGameObjectIterate(goList, prefab, amount - countReadyObjects, amountEachFrame, onGetObject, onFinished));
            }
            else
            {
                onFinished?.Invoke();
                return null;
            }

            IEnumerator IESpawnGameObjectIterate(List<GameObject> goList, GameObject prefab, int amount, int amountEachFrame, System.Action<GameObject> onGetObject, System.Action onFinished)
            {
                // extend list capacity
                goList.Capacity = Mathf.Max(goList.Capacity, goList.Count + amount);

                GameObject go = null;
                for (int i = 0; i < amount; i++)
                {
                    if (i % amountEachFrame == 0) yield return null;
                    go = (GameObject)Instantiate(prefab, transform);
                    goList.Add(go);
                    go.SetActive(true);
                    onGetObject?.Invoke(go);
                }
                onFinished?.Invoke();
            }
        }

        public Coroutine GetGameObjectsIterate(GameObject prefab, int amount, Vector3 position, Quaternion rotation, int amountEachFrame, System.Action<GameObject> onGetObject, System.Action onFinished = null)
        {
            if (prefab == null || onGetObject == null)
            {
                onFinished?.Invoke();
                return null;
            }

            int key = prefab.GetInstanceID();
            int countReadyObjects = 0;

            if (_pooledKeyList.Contains(key) == false && _pooledGoDic.ContainsKey(key) == false)
            {
                _pooledKeyList.Add(key);
                _pooledGoDic.Add(key, new List<GameObject>());
            }

            List<GameObject> goList = _pooledGoDic[key];

            GameObject go = null;

            for (int i = goList.Count - 1; i >= 0 && countReadyObjects < amount; i--)
            {
                go = goList[i];
                if (go == null)
                {
                    goList.Remove(go);
                    continue;
                }
                if (go.activeSelf == false)
                {
                    // Found free GameObject in object pool.
                    Transform goTransform = go.transform;
                    goTransform.position = position;
                    goTransform.rotation = rotation;
                    go.SetActive(true);
                    onGetObject?.Invoke(go);
                    countReadyObjects += 1;
                }
            }

            if (countReadyObjects < amount)
            {
                return StartCoroutine(IESpawnGameObjectIterate(goList, prefab, amount - countReadyObjects, position, rotation, amountEachFrame, onGetObject, onFinished));
            }
            else
            {
                onFinished?.Invoke();
                return null;
            }

            IEnumerator IESpawnGameObjectIterate(List<GameObject> goList, GameObject prefab, int amount, Vector3 position, Quaternion rotation, int amountEachFrame, System.Action<GameObject> onGetObject, System.Action onFinished)
            {
                // extend list capacity
                goList.Capacity = Mathf.Max(goList.Capacity, goList.Count + amount);

                GameObject go = null;
                for (int i = 0; i < amount; i++)
                {
                    if (i % amountEachFrame == 0) yield return null;
                    go = (GameObject)Instantiate(prefab, transform);
                    Transform goTransform = go.transform;
                    goTransform.position = position;
                    goTransform.rotation = rotation;
                    goList.Add(go);
                    go.SetActive(true);
                    onGetObject?.Invoke(go);
                }
                onFinished?.Invoke();
            }
        }

        public Coroutine GetGameObjectsIterate<T>(T prefabComponent, int amount, int amountEachFrame, System.Action<T> onGetComponent, System.Action onFinished = null) where T : MonoBehaviour
        {
            if (prefabComponent == null) return null;
            return GetGameObjectsIterate(prefabComponent.gameObject, amount, amountEachFrame, OnGetGameObject, onFinished);
            void OnGetGameObject(GameObject gameObject)
            {
                onGetComponent?.Invoke(gameObject.GetComponent<T>());
            }
        }

        public Coroutine GetParticles(ParticleSystem particle, int amount, int amountEachFrame, System.Action<ParticleSystem> onGetParticle, System.Action onFinished = null)
        {
            if (particle == null) return null;
            return GetGameObjectsIterate(particle.gameObject, amount, amountEachFrame, OnGetGameObject, onFinished);
            void OnGetGameObject(GameObject gameObject)
            {
                onGetParticle?.Invoke(gameObject.GetComponent<ParticleSystem>());
            }
        }

        public Coroutine GetLines(LineRenderer line, int amount, int amountEachFrame, System.Action<LineRenderer> onGetLine, System.Action onFinished = null)
        {
            if (line == null) return null;
            return GetGameObjectsIterate(line.gameObject, amount, amountEachFrame, OnGetGameObject, onFinished);
            void OnGetGameObject(GameObject gameObject)
            {
                onGetLine?.Invoke(gameObject.GetComponent<LineRenderer>());
            }
        }


        public Coroutine GetGameObjectsIterate<T>(T prefabComponent, int amount, Vector3 position, Quaternion rotation, int amountEachFrame, System.Action<T> onGetComponent, System.Action onFinished = null) where T : MonoBehaviour
        {
            if (prefabComponent == null) return null;
            return GetGameObjectsIterate(prefabComponent.gameObject, amount, position, rotation, amountEachFrame, OnGetGameObject, onFinished);
            void OnGetGameObject(GameObject gameObject)
            {
                onGetComponent?.Invoke(gameObject.GetComponent<T>());
            }
        }

        public Coroutine GetParticles(ParticleSystem particle, int amount, Vector3 position, Quaternion rotation, int amountEachFrame, System.Action<ParticleSystem> onGetParticle, System.Action onFinished = null)
        {
            if (particle == null) return null;
            return GetGameObjectsIterate(particle.gameObject, amount, position, rotation, amountEachFrame, OnGetGameObject, onFinished);
            void OnGetGameObject(GameObject gameObject)
            {
                onGetParticle?.Invoke(gameObject.GetComponent<ParticleSystem>());
            }
        }

        public Coroutine GetLines(LineRenderer line, int amount, Vector3 position, Quaternion rotation, int amountEachFrame, System.Action<LineRenderer> onGetLine, System.Action onFinished = null)
        {
            if (line == null) return null;
            return GetGameObjectsIterate(line.gameObject, amount, position, rotation, amountEachFrame, OnGetGameObject, onFinished);
            void OnGetGameObject(GameObject gameObject)
            {
                onGetLine?.Invoke(gameObject.GetComponent<LineRenderer>());
            }
        }
        #endregion

        /// <summary>
        /// Releases game object back to pool.
        /// </summary>
        public void ReleaseGameObject(GameObject go)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                DestroyImmediate(go);
                return;
            }
#endif
            go.transform.SetParent(transform);
            go.SetActive(false);
        }

        ///// <summary>
        ///// Delete game object and remove from pool.
        ///// </summary>
        //public void DestroyGameObject(GameObject go)
        //{
        //    Destroy(go);
        //    return;
        //}

        /// <summary>
        /// Get active bullets count.
        /// </summary>
        public int GetActivePooledObjectCount()
        {
            int cnt = 0;
            for (int i = 0; i < _pooledKeyList.Count; i++)
            {
                int key = _pooledKeyList[i];
                var goList = _pooledGoDic[key];
                for (int j = 0; j < goList.Count; j++)
                {
                    var go = goList[j];
                    if (go != null && go.activeInHierarchy)
                    {
                        cnt++;
                    }
                }
            }
            return cnt;
        }

        public List<GameObject> GetActiveObjects(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            int key = prefab.GetInstanceID();

            if (_pooledKeyList.Contains(key) == false && _pooledGoDic.ContainsKey(key) == false)
            {
                return null;
            }

            List<GameObject> listActiveObjecs = new List<GameObject>();

            List<GameObject> goList = _pooledGoDic[key];

            GameObject go = null;

            for (int i = goList.Count - 1; i >= 0; i--)
            {
                go = goList[i];
                if (go == null)
                {
                    goList.Remove(go);
                    continue;
                }
                if (go.activeSelf == true)
                {
                    listActiveObjecs.Add(go);
                }
            }

            return listActiveObjecs;
        }

        public List<GameObject> GetListSpawnedObjects(GameObject prefab)
        {
            int key = prefab.GetInstanceID();
            if (_pooledKeyList.Contains(key) && _pooledGoDic.TryGetValue(key, out List<GameObject> listObjects))
            {
                return listObjects;
            }
            else
            {
                return null;
            }
        }

        public int CountSpawnedObjects(GameObject prefab)
        {
            int key = prefab.GetInstanceID();
            if (_pooledKeyList.Contains(key) && _pooledGoDic.TryGetValue(key, out List<GameObject> listObjects))
            {
                if (listObjects != null)
                {
                    return listObjects.Count;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}