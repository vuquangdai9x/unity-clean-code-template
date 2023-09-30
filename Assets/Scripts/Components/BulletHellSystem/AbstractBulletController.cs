using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace DaiVQScript.BulletHellSystem
{
    public delegate bool HandleOnBulletCollided(BulletHellObjectBase bullet, Collider2D target);

    public abstract class AbstractBulletController<TJobStruct>
        where TJobStruct : struct, IJobParallelForTransform
    {
        [Header("System")]
        private JobHandle _jobUpdateHandler;
        protected TJobStruct _jobData;

        [Header("Ref")]
        private TransformAccessArray bulletTransforms;
        private NativeArray<bool> markCollided;
        private BulletHellObjectBase[] bullets;

        private HandleOnBulletCollided _onBulletCollidedHandler;
        private System.Action<BulletHellObjectBase> _onBulletRemoved;
        public bool IsAlive { get; private set; }

        public int CountBulletActive => bulletTransforms.length;

        public void SetUp(int maxNumBullets, HandleOnBulletCollided onBulletCollidedHandler, System.Action<BulletHellObjectBase> onBulletRemoved)
        {
            _onBulletCollidedHandler = onBulletCollidedHandler;
            _onBulletRemoved = onBulletRemoved;

            markCollided = new NativeArray<bool>(maxNumBullets, Allocator.Persistent);
            bulletTransforms = new TransformAccessArray(maxNumBullets);
            bullets = new BulletHellObjectBase[maxNumBullets];

            SetUpJobData(maxNumBullets);

            IsAlive = true;
        }

        protected abstract void SetUpJobData(int maxNumBullets);

        public void Destroy()
        {
            IsAlive = false;
            markCollided.Dispose();
            bulletTransforms.Dispose();
            OnDestruct();
        }

        protected abstract void OnDestruct();

        public void RemoveAllBullets()
        {
            if (!_jobUpdateHandler.IsCompleted)
            {
                _jobUpdateHandler.Complete();
            }

            for (int i = bulletTransforms.length - 1; i >= 0; i--)
            {
                bulletTransforms.RemoveAtSwapBack(i);
                _onBulletRemoved?.Invoke(bullets[i]);
            }
        }

        protected int GetNewBulletIndex() => (bulletTransforms.length < bulletTransforms.capacity ? bulletTransforms.length : -1);
        protected void SetUpNewShootBullet(BulletHellObjectBase bullet)
        {
            int newIndex = bulletTransforms.length;

            bullet.index = newIndex;
            bullet.SetUp(OnBulletCollided);

            bulletTransforms.Add(bullet.transform);
            bullets[newIndex] = bullet;
            markCollided[newIndex] = false;
        }

        private void OnBulletCollided(BulletHellObjectBase bullet, Collider2D target)
        {
            markCollided[bullet.index] = _onBulletCollidedHandler(bullet, target);
        }

        public void Update()
        {
            if (bulletTransforms.length > 0)
            {
                ChangeJobDataOnUpdate();
                _jobUpdateHandler = _jobData.Schedule(bulletTransforms);
            }
        }

        protected abstract void ChangeJobDataOnUpdate();

        public void LateUpdate()
        {
            _jobUpdateHandler.Complete();
            BulletHellObjectBase bulletToRemove;
            for (int i = bulletTransforms.length - 1; i >= 0; i--)
            {
                if (CheckIsRemoveBullet(i) || markCollided[i])
                {
                    bulletToRemove = bullets[i];

                    if (i == bulletTransforms.length - 1)
                    {
                        bulletTransforms.RemoveAtSwapBack(i);
                    }
                    else
                    {
                        int lastIndex = bulletTransforms.length - 1;
                        bulletTransforms.RemoveAtSwapBack(i);

                        bullets[i] = bullets[lastIndex];
                        bullets[i].index = i;

                        markCollided[i] = markCollided[lastIndex];

                        RemoveBulletDataBySwapBack(i, lastIndex);
                    }

                    _onBulletRemoved?.Invoke(bulletToRemove);
                }
            }
        }

        protected abstract bool CheckIsRemoveBullet(int index);
        protected abstract void RemoveBulletDataBySwapBack(int index, int lastIndex);
    }
}