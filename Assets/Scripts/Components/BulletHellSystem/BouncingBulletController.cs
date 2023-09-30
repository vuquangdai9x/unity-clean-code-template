using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace DaiVQScript.BulletHellSystem
{

    [System.Serializable]
    public class BouncingBulletController : AbstractBulletController<BulletHellObjectBase.BouncingFlyJob>
    {
        public class ShootInfo
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

            [Header("Bounce")]
            public int numBounceVerticleRemain;
            public int numBounceHorizontalRemain;
            public int numBounceBothRemain;
        }

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

        [Header("Bounce")]
        private NativeArray<int> numBounceVerticleRemains;
        private NativeArray<int> numBounceHorizontalRemains;
        private NativeArray<int> numBounceBothRemains;

        [Header("Boundary")]
        public Rect boundRect;
        public float speedMulOnCollidedWall;

        public void SetBoundary(Rect worldRect, float speedMulOnCollidedWall)
        {
            boundRect = worldRect;
            this.speedMulOnCollidedWall = speedMulOnCollidedWall;
            _jobData.xMin = worldRect.xMin;
            _jobData.xMax = worldRect.xMax;
            _jobData.yMin = worldRect.yMin;
            _jobData.yMax = worldRect.yMax;
            _jobData.speedMulOnCollidedWall = speedMulOnCollidedWall;
        }

        protected override void SetUpJobData(int maxNumBullets)
        {
            velocity = new NativeArray<float2>(maxNumBullets, Allocator.Persistent);
            anchor = new NativeArray<float2>(maxNumBullets, Allocator.Persistent);
            timeStart = new NativeArray<float>(maxNumBullets, Allocator.Persistent);

            accelerate = new NativeArray<float>(maxNumBullets, Allocator.Persistent);
            turnAngleRadian = new NativeArray<float>(maxNumBullets, Allocator.Persistent);

            initScale = new NativeArray<float3>(maxNumBullets, Allocator.Persistent);
            scaleSpeed = new NativeArray<float3>(maxNumBullets, Allocator.Persistent);

            numBounceVerticleRemains = new NativeArray<int>(maxNumBullets, Allocator.Persistent);
            numBounceHorizontalRemains = new NativeArray<int>(maxNumBullets, Allocator.Persistent);
            numBounceBothRemains = new NativeArray<int>(maxNumBullets, Allocator.Persistent);

            _jobData = new BulletHellObjectBase.BouncingFlyJob();

            _jobData.velocity = velocity;
            _jobData.anchor = anchor;
            _jobData.timeStart = timeStart;

            _jobData.accelerate = accelerate;
            _jobData.turnAngleRadian = turnAngleRadian;

            _jobData.initScale = initScale;
            _jobData.scaleSpeed = scaleSpeed;

            _jobData.numBounceBothRemains = numBounceBothRemains;
            _jobData.numBounceVerticleRemains = numBounceVerticleRemains;
            _jobData.numBounceHorizontalRemains = numBounceHorizontalRemains;

            SetBoundary(boundRect, speedMulOnCollidedWall);
        }

        protected override void OnDestruct()
        {
            velocity.Dispose();
            anchor.Dispose();
            timeStart.Dispose();

            accelerate.Dispose();
            turnAngleRadian.Dispose();

            initScale.Dispose();
            scaleSpeed.Dispose();

            numBounceBothRemains.Dispose();
            numBounceVerticleRemains.Dispose();
            numBounceHorizontalRemains.Dispose();
        }

        public void ShootBullet(BulletHellObjectBase bullet, ShootInfo shootInfo)
        {
            int newIndex = GetNewBulletIndex();
            if (newIndex < 0) return;

            SetUpNewShootBullet(bullet);
            
            bullet.transform.position = new Vector3(shootInfo.startPosition.x, shootInfo.startPosition.y, 0);
            bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, new Vector3(shootInfo.velocity.x, shootInfo.velocity.y, 0f));
            bullet.transform.localScale = shootInfo.initScale;

            anchor[newIndex] = shootInfo.startPosition;
            velocity[newIndex] = shootInfo.velocity;
            timeStart[newIndex] = Time.time;

            accelerate[newIndex] = shootInfo.accelerate;
            turnAngleRadian[newIndex] = shootInfo.turnAngleRadian;

            initScale[newIndex] = shootInfo.initScale;
            scaleSpeed[newIndex] = shootInfo.scaleSpeed;

            scaleSpeed[newIndex] = shootInfo.scaleSpeed;

            numBounceBothRemains[newIndex] = shootInfo.numBounceBothRemain;
            numBounceVerticleRemains[newIndex] = shootInfo.numBounceVerticleRemain;
            numBounceHorizontalRemains[newIndex] = shootInfo.numBounceHorizontalRemain;
        }

        protected override void ChangeJobDataOnUpdate()
        {
            _jobData.deltaTime = Time.deltaTime;
            _jobData.time = Time.time;
            _jobData.directionalForce = new float2(GameplayGlobalData.Instance.GlobalDirectionalForce) + localDirectionalForce;
        }

        protected override bool CheckIsRemoveBullet(int index) 
            => numBounceBothRemains[index] == 0 || numBounceVerticleRemains[index] == 0 || numBounceHorizontalRemains[index] == 0;

        protected override void RemoveBulletDataBySwapBack(int index, int lastIndex)
        {
            anchor[index] = anchor[lastIndex];
            velocity[index] = velocity[lastIndex];
            timeStart[index] = timeStart[lastIndex];

            accelerate[index] = accelerate[lastIndex];
            turnAngleRadian[index] = turnAngleRadian[lastIndex];

            initScale[index] = initScale[lastIndex];
            scaleSpeed[index] = scaleSpeed[lastIndex];

            numBounceBothRemains[index] = numBounceBothRemains[lastIndex];
            numBounceVerticleRemains[index] = numBounceVerticleRemains[lastIndex];
            numBounceHorizontalRemains[index] = numBounceHorizontalRemains[lastIndex];
        }
    }
}