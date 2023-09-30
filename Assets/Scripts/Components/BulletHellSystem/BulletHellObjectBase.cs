using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Unity.Collections;

namespace DaiVQScript.BulletHellSystem
{
    public class BulletHellObjectBase : MonoBehaviour
    {
        public enum MoveAroundAnchorMode { CONST = 0, CLOCK_WISE = 1, COUNTER_CLOCK_WISE = 2, SINE = 3, ZIGZAG = 4 }

        //[Header("State")]
        //public float2 velocity = float2.zero;
        //private float2 anchor = float2.zero;

        //[Header("Basic")]
        //public float accelerate = 0f;
        //public float turnAngleRadian = 0f;

        //[Header("Scale")]
        //public float3 initScale = float3.zero;
        //public float3 scaleSpeed = float3.zero;

        //[Header("Global Force")]
        //public float2 globalDirectionalForce = float2.zero;

        //[Header("Rotate Around Anchor")]
        //public MoveAroundAnchorMode moveAroundAnchorMode = MoveAroundAnchorMode.CONST;
        //public float2 moveAroundRadius = float2.zero;
        //public float2 moveAroundRadiusSpeed = float2.zero;
        //public float moveAroundFreqency = 1f;
        //public float moveAroundArcRadian = 0f;
        //public float moveAroundArcRadianSpeed = 0f;

        ////private Transform _transform;
        ////private float3 _tmpPosition;
        ////private quaternion _tmpRotation;
        ////private float3 _tmpScale;


        ////private void Awake()
        ////{
        ////    _transform = transform;
        ////    _tmpPosition = _transform.position;
        ////}

        //private void Update()
        //{
        //    UpdateBullet(Time.time, Time.deltaTime);
        //}

        //private void UpdateBullet(float timePass, float deltaTime)
        //{
        //    float posZ = transform.position.z;

        //    // turn
        //    float sin, cos;
        //    math.sincos(turnAngleRadian * deltaTime, out sin, out cos);
        //    float2x2 turnAngleRotateMat = new float2x2(cos, -sin, sin, cos);
        //    velocity = math.mul(turnAngleRotateMat, velocity);

        //    // accelerate
        //    float speed = math.length(velocity);
        //    velocity = velocity / speed * (speed + accelerate * deltaTime);

        //    // global force
        //    velocity += globalDirectionalForce * deltaTime;

        //    // anchor
        //    anchor += velocity * deltaTime;

        //    // position
        //    float2 position = anchor;
        //    float timeProgressMoveAround = math.frac(timePass * moveAroundFreqency);
        //    float angle = moveAroundAnchorMode switch
        //    {
        //        MoveAroundAnchorMode.CONST => 0f,
        //        MoveAroundAnchorMode.CLOCK_WISE => (1f - timeProgressMoveAround) * 2f * math.PI + 0.5f * math.PI,
        //        MoveAroundAnchorMode.COUNTER_CLOCK_WISE => timeProgressMoveAround * 2f * math.PI + 0.5f * math.PI,
        //        MoveAroundAnchorMode.SINE => math.sin(timeProgressMoveAround * math.PI * 2f) * (moveAroundArcRadian + moveAroundArcRadianSpeed * timePass),
        //        MoveAroundAnchorMode.ZIGZAG => ((timeProgressMoveAround < 0.5f) ? (timeProgressMoveAround * 4f - 1f) : (-(timeProgressMoveAround - 0.5f) * 4f + 1f)) * (moveAroundArcRadian + moveAroundArcRadianSpeed * timePass),
        //        _ => 0
        //    };
        //    math.sincos(angle, out sin, out cos);
        //    position += new float2(cos, sin) * (moveAroundRadius + moveAroundRadiusSpeed * timePass);

        //    // scale
        //    float3 scale = initScale + scaleSpeed * timePass;

        //    // update to transform
        //    transform.position = new float3(position, posZ);
        //    transform.localScale = scale;
        //    transform.rotation = quaternion.LookRotation(new float3(0, 0, 1), new float3(velocity, 0));
        //}

        public struct FlyWithAnchorJob : IJobParallelForTransform
        {
            [Header("State")]
            public NativeArray<float2> velocity;
            public NativeArray<float2> anchor;
            [ReadOnly] public NativeArray<float> timeStart;
            [ReadOnly] public float time;
            [ReadOnly] public float deltaTime;

            [Header("Basic")]
            [ReadOnly] public NativeArray<float> accelerate;
            [ReadOnly] public NativeArray<float> turnAngleRadian;

            [Header("Scale")]
            [ReadOnly] public NativeArray<float3> initScale;
            [ReadOnly] public NativeArray<float3> scaleSpeed;

            [Header("Global Force")]
            [ReadOnly] public float2 directionalForce;

            [Header("Rotate Around Anchor")]
            [ReadOnly] public MoveAroundAnchorMode moveAroundAnchorMode;
            [ReadOnly] public NativeArray<float2> moveAroundRadius;
            [ReadOnly] public NativeArray<float2> moveAroundRadiusSpeed;
            [ReadOnly] public NativeArray<float> moveAroundFrequency;
            [ReadOnly] public NativeArray<float> moveAroundArcRadian;
            [ReadOnly] public NativeArray<float> moveAroundArcRadianSpeed;

            public void Execute(int index, TransformAccess transform)
            {
                float posZ = transform.position.z;
                float timePass = time - timeStart[index];

                // turn
                float sin, cos;
                math.sincos(turnAngleRadian[index] * deltaTime, out sin, out cos);
                float2x2 turnAngleRotateMat = new float2x2(cos, -sin, sin, cos);
                float2 v = math.mul(turnAngleRotateMat, velocity[index]);

                // accelerate
                float speed = math.length(v);
                v = v / speed * (speed + accelerate[index] * deltaTime);

                // global force
                v += directionalForce * deltaTime;

                // anchor
                anchor[index] += v * deltaTime;

                // position
                float2 position = anchor[index];
                float timeProgressMoveAround = math.frac(timePass * moveAroundFrequency[index]);
                float timeProgressMoveAroundZigzag = math.frac(timePass * moveAroundFrequency[index] + 0.25f);
                float angle = moveAroundAnchorMode switch
                {
                    MoveAroundAnchorMode.CONST => 0.5f * math.PI,
                    MoveAroundAnchorMode.CLOCK_WISE => (1f - timeProgressMoveAround) * 2f * math.PI + 0.5f * math.PI,
                    MoveAroundAnchorMode.COUNTER_CLOCK_WISE => timeProgressMoveAround * 2f * math.PI + 0.5f * math.PI,
                    MoveAroundAnchorMode.SINE => 0.5f * math.PI + math.sin(timeProgressMoveAround * math.PI * 2f) * (moveAroundArcRadian[index] + moveAroundArcRadianSpeed[index] * timePass),
                    MoveAroundAnchorMode.ZIGZAG => 0.5f * math.PI + ((timeProgressMoveAroundZigzag < 0.5f) ? (timeProgressMoveAroundZigzag * 4f - 1f) : (-(timeProgressMoveAroundZigzag - 0.5f) * 4f + 1f)) * (moveAroundArcRadian[index] + moveAroundArcRadianSpeed[index] * timePass),
                    _ => 0.5f * math.PI
                };
                math.sincos(angle, out sin, out cos);
                position += new float2(cos, sin) * (moveAroundRadius[index] + moveAroundRadiusSpeed[index] * timePass);

                // scale
                float3 scale = initScale[index] + scaleSpeed[index] * timePass;

                velocity[index] = v;

                // update to transform
                transform.position = new float3(position, posZ);
                transform.localScale = scale;
                transform.rotation = quaternion.LookRotation(new float3(0, 0, 1), new float3(v, 0));
            }
        }

        public struct BouncingFlyJob : IJobParallelForTransform
        {
            [Header("State")]
            public NativeArray<float2> velocity;
            public NativeArray<float2> anchor;
            [ReadOnly] public NativeArray<float> timeStart;
            [ReadOnly] public float time;
            [ReadOnly] public float deltaTime;

            [Header("Basic")]
            [ReadOnly] public NativeArray<float> accelerate;
            [ReadOnly] public NativeArray<float> turnAngleRadian;

            [Header("Scale")]
            [ReadOnly] public NativeArray<float3> initScale;
            [ReadOnly] public NativeArray<float3> scaleSpeed;

            [Header("Global Force")]
            [ReadOnly] public float2 directionalForce;

            [Header("Bound")]
            [ReadOnly] public float xMin;
            [ReadOnly] public float xMax;
            [ReadOnly] public float yMin;
            [ReadOnly] public float yMax;
            [ReadOnly] public float speedMulOnCollidedWall;
            public NativeArray<int> numBounceVerticleRemains;
            public NativeArray<int> numBounceHorizontalRemains;
            public NativeArray<int> numBounceBothRemains;

            public void Execute(int index, TransformAccess transform)
            {
                float posZ = transform.position.z;
                float timePass = time - timeStart[index];

                // turn
                float sin, cos;
                math.sincos(turnAngleRadian[index] * deltaTime, out sin, out cos);
                float2x2 turnAngleRotateMat = new float2x2(cos, -sin, sin, cos);
                float2 v = math.mul(turnAngleRotateMat, velocity[index]);

                // accelerate
                float speed = math.length(v);
                v = v / speed * (speed + accelerate[index] * deltaTime);

                // global force
                v += directionalForce * deltaTime;

                // anchor
                anchor[index] += v * deltaTime;

                // position
                float2 position = anchor[index];

                bool isBounce = false;
                if (position.x < xMin)
                {
                    position.x = xMin + (xMin - position.x);
                    v.x = -v.x;
                    numBounceHorizontalRemains[index] -= 1;
                    isBounce = true;
                }
                else if (position.x > xMax)
                {
                    position.x = xMax - (position.x - xMax);
                    v.x = -v.x;
                    numBounceHorizontalRemains[index] -= 1;
                    isBounce = true;
                }

                if (position.y < yMin)
                {
                    position.y = yMin + (yMin - position.y);
                    v.y = -v.y;
                    numBounceVerticleRemains[index] -= 1;
                    isBounce = true;
                }
                else if (position.y > yMax)
                {
                    position.y = yMax - (position.y - yMax);
                    v.y = -v.y;
                    numBounceVerticleRemains[index] -= 1;
                    isBounce = true;
                }

                if (isBounce)
                {
                    numBounceBothRemains[index] -= 1;
                }

                // scale
                float3 scale = initScale[index] + scaleSpeed[index] * timePass;

                velocity[index] = v;
                anchor[index] = position;

                // update to transform
                transform.position = new float3(position, posZ);
                transform.localScale = scale;
                transform.rotation = quaternion.LookRotation(new float3(0, 0, 1), new float3(v, 0));
            }
        }

        public int index = -1;
        public int[] extDataInts = null;
        public float[] extDataFloats = null;
        private System.Action<BulletHellObjectBase, Collider2D> _onBulletCollided = null;
        public void SetUp(System.Action<BulletHellObjectBase, Collider2D> onBulletCollided)
        {
            _onBulletCollided = onBulletCollided;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            _onBulletCollided?.Invoke(this, collision);
        }
    }
}