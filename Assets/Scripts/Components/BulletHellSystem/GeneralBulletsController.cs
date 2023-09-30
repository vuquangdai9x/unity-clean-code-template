using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace DaiVQScript.BulletHellSystem
{
    public class GeneralBulletShootInfo
    {
        [Header("State")]
        public float2 startPosition;
        public float2 velocity;

        [Header("Basic")]
        public float accelerate;
        public float turnAngleRadian;

        [Header("Scale")]
        public float3 initScale;
        public float3 scaleSpeed;

        [Header("Rotate Around Anchor")]
        public float2 moveAroundRadius;
        public float2 moveAroundRadiusSpeed;
        public float moveAroundFrequency;
        public float moveAroundArcRadian;
        public float moveAroundArcRadianSpeed;
    }

    [System.Serializable]
    public class GeneralBulletsController
    {
        [Header("System")]
        private JobHandle _jobUpdateHandler;
        private BulletHellObjectBase.FlyWithAnchorJob _updatePositionJob;

        [Header("Ref")]
        private TransformAccessArray bulletTransforms;
        private NativeArray<bool> markCollided;
        private BulletHellObjectBase[] bullets;

        [Header("State")]
        private NativeArray<float2> velocity;
        private NativeArray<float2> anchor;
        private NativeArray<float> timeStart;

        [Header("Basic")]
        private NativeArray<float> accelerate;
        private NativeArray<float> turnAngleRadian;

        [Header("Scale")]
        private NativeArray<float3> initScale;
        private NativeArray<float3> scaleSpeed;

        [Header("Force")]
        public float2 localDirectionalForce;

        [Header("Rotate Around Anchor")]
        public BulletHellObjectBase.MoveAroundAnchorMode moveAroundAnchorMode;
        private NativeArray<float2> moveAroundRadius;
        private NativeArray<float2> moveAroundRadiusSpeed;
        private NativeArray<float> moveAroundFrequency;
        private NativeArray<float> moveAroundArcRadian;
        private NativeArray<float> moveAroundArcRadianSpeed;

        public float autoReleaseTime;
        private HandleOnBulletCollided _onBulletCollidedHandler;
        private System.Action<BulletHellObjectBase> _onBulletRemoved;
        public bool IsAlive { get; private set; }

        //public delegate bool HandleOnBulletCollided(BulletHellObjectBase bullet, Collider2D target);

        public int CountBulletActive => bulletTransforms.length;

        public GeneralBulletsController(int maxNumBullets, HandleOnBulletCollided onBulletCollidedHandler, System.Action<BulletHellObjectBase> onBulletRemoved)
        {
            _onBulletCollidedHandler = onBulletCollidedHandler;
            _onBulletRemoved = onBulletRemoved;

            markCollided = new NativeArray<bool>(maxNumBullets, Allocator.Persistent);

            velocity = new NativeArray<float2>(maxNumBullets, Allocator.Persistent);
            anchor = new NativeArray<float2>(maxNumBullets, Allocator.Persistent);
            timeStart = new NativeArray<float>(maxNumBullets, Allocator.Persistent);

            accelerate = new NativeArray<float>(maxNumBullets, Allocator.Persistent);
            turnAngleRadian = new NativeArray<float>(maxNumBullets, Allocator.Persistent);

            initScale = new NativeArray<float3>(maxNumBullets, Allocator.Persistent);
            scaleSpeed = new NativeArray<float3>(maxNumBullets, Allocator.Persistent);

            moveAroundRadius = new NativeArray<float2>(maxNumBullets, Allocator.Persistent);
            moveAroundRadiusSpeed = new NativeArray<float2>(maxNumBullets, Allocator.Persistent);
            moveAroundFrequency = new NativeArray<float>(maxNumBullets, Allocator.Persistent);
            moveAroundArcRadian = new NativeArray<float>(maxNumBullets, Allocator.Persistent);
            moveAroundArcRadianSpeed = new NativeArray<float>(maxNumBullets, Allocator.Persistent);

            bulletTransforms = new TransformAccessArray(maxNumBullets);

            bullets = new BulletHellObjectBase[maxNumBullets];

            _updatePositionJob = new BulletHellObjectBase.FlyWithAnchorJob();

            _updatePositionJob.velocity = velocity;
            _updatePositionJob.anchor = anchor;
            _updatePositionJob.timeStart = timeStart;

            _updatePositionJob.accelerate = accelerate;
            _updatePositionJob.turnAngleRadian = turnAngleRadian;

            _updatePositionJob.initScale = initScale;
            _updatePositionJob.scaleSpeed = scaleSpeed;

            _updatePositionJob.moveAroundAnchorMode = moveAroundAnchorMode;
            _updatePositionJob.moveAroundRadius = moveAroundRadius;
            _updatePositionJob.moveAroundRadiusSpeed = moveAroundRadiusSpeed;
            _updatePositionJob.moveAroundFrequency = moveAroundFrequency;
            _updatePositionJob.moveAroundArcRadian = moveAroundArcRadian;
            _updatePositionJob.moveAroundArcRadianSpeed = moveAroundArcRadianSpeed;

            IsAlive = true;
        }

        public void Destroy()
        {
            IsAlive = false;

            markCollided.Dispose();

            velocity.Dispose();
            anchor.Dispose();
            timeStart.Dispose();

            accelerate.Dispose();
            turnAngleRadian.Dispose();

            initScale.Dispose();
            scaleSpeed.Dispose();

            moveAroundRadius.Dispose();
            moveAroundRadiusSpeed.Dispose();
            moveAroundFrequency.Dispose();
            moveAroundArcRadian.Dispose();
            moveAroundArcRadianSpeed.Dispose();

            bulletTransforms.Dispose();
        }

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

        public void ShootBullet(BulletHellObjectBase bullet, GeneralBulletShootInfo shootInfo)
        {
            if (bulletTransforms.length == bulletTransforms.capacity) return;
            int newIndex = bulletTransforms.length;

            bullet.transform.position = new Vector3(shootInfo.startPosition.x, shootInfo.startPosition.y, 0);
            bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, new Vector3(shootInfo.velocity.x, shootInfo.velocity.y, 0));
            bullet.transform.localScale = shootInfo.initScale;

            bullet.index = newIndex;
            bullet.SetUp(OnBulletCollided);

            bulletTransforms.Add(bullet.transform);
            bullets[newIndex] = bullet;
            markCollided[newIndex] = false;

            anchor[newIndex] = shootInfo.startPosition;
            velocity[newIndex] = shootInfo.velocity;
            timeStart[newIndex] = Time.time;

            accelerate[newIndex] = shootInfo.accelerate;
            turnAngleRadian[newIndex] = shootInfo.turnAngleRadian;

            initScale[newIndex] = shootInfo.initScale;
            scaleSpeed[newIndex] = shootInfo.scaleSpeed;

            moveAroundRadius[newIndex] = shootInfo.moveAroundRadius;
            moveAroundRadiusSpeed[newIndex] = shootInfo.moveAroundRadiusSpeed;
            moveAroundFrequency[newIndex] = shootInfo.moveAroundFrequency;
            moveAroundArcRadian[newIndex] = shootInfo.moveAroundArcRadian;
            moveAroundArcRadianSpeed[newIndex] = shootInfo.moveAroundArcRadianSpeed;
        }

        private void OnBulletCollided(BulletHellObjectBase bullet, Collider2D target)
        {
            if (IsAlive) markCollided[bullet.index] = _onBulletCollidedHandler(bullet, target);
        }

        public void Update()
        {
            if (bulletTransforms.length > 0)
            {
                _updatePositionJob.deltaTime = Time.deltaTime;
                _updatePositionJob.time = Time.time;
                _updatePositionJob.directionalForce = localDirectionalForce + GameplayGlobalData.Instance.GlobalDirectionalForce;
                _updatePositionJob.moveAroundAnchorMode = moveAroundAnchorMode;
                _jobUpdateHandler = _updatePositionJob.Schedule(bulletTransforms);
            }
        }

        public void LateUpdate()
        {
            _jobUpdateHandler.Complete();
            BulletHellObjectBase bulletToRemove;
            for (int i = bulletTransforms.length - 1; i >= 0; i--)
            {
                if (Time.time > timeStart[i] + autoReleaseTime || markCollided[i])
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

                        anchor[i] = anchor[lastIndex];
                        velocity[i] = velocity[lastIndex];
                        timeStart[i] = timeStart[lastIndex];

                        accelerate[i] = accelerate[lastIndex];
                        turnAngleRadian[i] = turnAngleRadian[lastIndex];

                        initScale[i] = initScale[lastIndex];
                        scaleSpeed[i] = scaleSpeed[lastIndex];

                        moveAroundRadius[i] = moveAroundRadius[lastIndex];
                        moveAroundRadiusSpeed[i] = moveAroundRadiusSpeed[lastIndex];
                        moveAroundFrequency[i] = moveAroundFrequency[lastIndex];
                        moveAroundArcRadian[i] = moveAroundArcRadian[lastIndex];
                        moveAroundArcRadianSpeed[i] = moveAroundArcRadianSpeed[lastIndex];
                    }

                    _onBulletRemoved?.Invoke(bulletToRemove);
                }
            }
        }
    }
}