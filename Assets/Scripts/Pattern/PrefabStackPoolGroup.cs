using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript
{
    public class PrefabStackPoolGroup<TBase> where TBase : Component
    {
        public Transform container;
        private Dictionary<TBase, PrefabStackPool<TBase>> _pooledBulletDict = new Dictionary<TBase, PrefabStackPool<TBase>>();

        private struct PrePoolJob
        {
            public int maxNumber;
            public Coroutine coroutine;
            public PrefabStackPool<TBase> stackPool;
        }
        private Dictionary<TBase, PrePoolJob> _corPrePoolMaxDict = new Dictionary<TBase, PrePoolJob>();

        public PrefabStackPool<TBase> AddPool(TBase prefab, int capacity)
        {
            if (_pooledBulletDict.TryGetValue(prefab, out PrefabStackPool<TBase> pool))
            {
                return pool;
            }
            else
            {
                pool = new PrefabStackPool<TBase>(prefab, container, capacity);
                _pooledBulletDict.Add(prefab, pool);
                return pool;
            }
        }
        public PrefabStackPool<TBase> GetPool(TBase prefab)
        {
            if (_pooledBulletDict.TryGetValue(prefab, out PrefabStackPool<TBase> pool))
            {
                return pool;
            }
            else
            {
                return null;
            }
        }
        public TBase GetObject(TBase prefab) => GetPool(prefab)?.UseGameObject();
        public void ReturnObject(TBase prefab, TBase obj) => GetPool(prefab)?.ReturnGameObject(obj);

        public void PrePoolMaxIterate(MonoBehaviour coroutineContainer, TBase bullet, int maxNum)
        {
            if (_corPrePoolMaxDict.TryGetValue(bullet, out PrePoolJob prePoolJob))
            {
                if (prePoolJob.maxNumber < maxNum && prePoolJob.stackPool.CountMaxSpawned < maxNum)
                {
                    if (prePoolJob.coroutine != null) coroutineContainer.StopCoroutine(prePoolJob.coroutine);
                    prePoolJob.coroutine = coroutineContainer.StartCoroutine(prePoolJob.stackPool.IEPrePoolMaxIterate(maxNum));
                }
            }
            else
            {
                prePoolJob = new PrePoolJob()
                {
                    maxNumber = maxNum,
                    stackPool = GetPool(bullet)
                };

                if (prePoolJob.stackPool.CountMaxSpawned < maxNum)
                {
                    prePoolJob.coroutine = coroutineContainer.StartCoroutine(prePoolJob.stackPool.IEPrePoolMaxIterate(maxNum));
                }

                _corPrePoolMaxDict.Add(bullet, prePoolJob);
            }
        }
    }
}