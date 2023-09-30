using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript.BulletHellSystem
{
    public class BulletHellSharedPool : DaiVQSingleton<BulletHellSharedPool>
    {
        private Dictionary<BulletHellObjectBase, PrefabStackPool<BulletHellObjectBase>> _pooledBulletDict = new Dictionary<BulletHellObjectBase, PrefabStackPool<BulletHellObjectBase>>();

        public struct PrePoolJob
        {
            public int maxNumber;
            public Coroutine coroutine;
            public PrefabStackPool<BulletHellObjectBase> stackPool;
        }
        private Dictionary<BulletHellObjectBase, PrePoolJob> _corPrePoolMaxDict = new Dictionary<BulletHellObjectBase, PrePoolJob>();

        public PrefabStackPool<BulletHellObjectBase> AddPool(BulletHellObjectBase bullet, int capacity)
        {
            if (_pooledBulletDict.TryGetValue(bullet, out PrefabStackPool<BulletHellObjectBase> pool))
            {
                return pool;
            }
            else
            {
                pool = new PrefabStackPool<BulletHellObjectBase>(bullet, transform, capacity);
                _pooledBulletDict.Add(bullet, pool);
                return pool;
            }
        }

        public void PrePoolMaxIterate(BulletHellObjectBase bullet, int maxNum)
        {
            if (_corPrePoolMaxDict.TryGetValue(bullet, out PrePoolJob prePoolJob))
            {
                if (prePoolJob.maxNumber < maxNum && prePoolJob.stackPool.CountMaxSpawned < maxNum)
                {
                    if (prePoolJob.coroutine != null) StopCoroutine(prePoolJob.coroutine);
                    prePoolJob.coroutine = StartCoroutine(prePoolJob.stackPool.IEPrePoolMaxIterate(maxNum));
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
                    prePoolJob.coroutine = StartCoroutine(prePoolJob.stackPool.IEPrePoolMaxIterate(maxNum));
                }

                _corPrePoolMaxDict.Add(bullet, prePoolJob);
            }
        }

        public PrefabStackPool<BulletHellObjectBase> GetPool(BulletHellObjectBase bullet)
        {
            if (_pooledBulletDict.TryGetValue(bullet, out PrefabStackPool<BulletHellObjectBase> pool))
            {
                return pool;
            }
            else
            {
                pool = new PrefabStackPool<BulletHellObjectBase>(bullet, transform, 5);
                _pooledBulletDict.Add(bullet, pool);
                return pool;
            }
        }
    }
}