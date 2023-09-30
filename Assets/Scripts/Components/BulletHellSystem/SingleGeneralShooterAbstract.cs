using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace DaiVQScript.BulletHellSystem
{
    public abstract class SingleGeneralShooterAbstract : MonoBehaviour
    {
        public Transform shootTarget = null;
        public Vector2 shootDirection = Vector2.up;

        [SerializeField] private BulletHellObjectBase prefabBullet = null;
        [SerializeField] private int amountPrePoolingBullet = -1;
        private PrefabStackPool<BulletHellObjectBase> _poolBullet;
        public bool isUpdateBulletLimitOnStartShoot = false;
        public int bulletLimitAmount = -1;

        [Header("Shoot Wave")]
        public int numWaves = 1;
        public float delayBetweenWaves = 0f;

        public Vector2 speedRange = new Vector2(5f, 5f);
        public float speedRandomness = 0f;

        public Vector2 accelerateRange = new Vector2(0f, 0f);
        public float accelerateRandomness = 0f;

        public Vector2 turnAngleRange = new Vector2(0f, 0f);
        public float turnAngleRandomness = 0f;

        [Header("Way")]
        public int numWays = 1;
        public float delayBetweenWays = 0f;

        public float angleOffset = 0f;
        public float angleRange = 360f;
        public float angleRandomness = 0f;

        public bool isFlipTurnSpeedByOddWay = false;
        public bool isFlipTurnSpeedByLeftRight = false;

        [Header("Bullet Pack")]
        public int numBulletsInPack = 1;

        public float packSpacingVariance = 0f;
        public float packSpacingRandomness = 0f;

        public float packSpreadAngleVariance = 0f;
        public float packSpreadAngleRandomness = 0f;

        public float packSpeedVariance = 0f;
        public float packSpeedRandomness = 0f;

        public float packAccelerateVariance = 0f;
        public float packAccelerateRandomness = 0f;

        public float packTurnAngleSpeedVariance = 0f;
        public float packTurnAngleSpeedRandomness = 0f;

        [Header("Time")]
        public float autoReleaseTime = 5f;

        [Header("Scale")]
        public Vector3 initScale = Vector3.one;
        public Vector3 scaleSpeed = Vector3.zero;

        [Header("Rotate Around Anchor")]
        public BulletHellObjectBase.MoveAroundAnchorMode moveAroundAnchorMode = BulletHellObjectBase.MoveAroundAnchorMode.CONST;
        public Vector2 moveAroundRadius = Vector2.zero;
        public Vector2 moveAroundRadiusSpeed = Vector2.zero;
        public float moveAroundFreqency = 1f;
        public float moveAroundArc = 0f;
        public float moveAroundArcSpeed = 0f;

        [Header("Global Force")]
        private Vector2 globalDirectionalForce = Vector2.zero;

        private GeneralBulletsController _shootThread;
        private Coroutine _corShooting;
        private System.Action _onFinishedShooting = null;
        private System.Action _onAllBulletDisappeared = null;
        private bool _isShooting;

        //public void SetUp()
        //{
        //    int maxNumBullet = (bulletLimitAmount >= 0) ? bulletLimitAmount : (numWaves * numWays * numBulletsInPack);
        //    _shootThread = new GeneralBulletsController(maxNumBullet, HandleOnBulletCollided, OnBulletRemoved);
        //    _poolBullet = BulletHellSharedPool.Instance.AddPool(prefabBullet, maxNumBullet);
        //    if (amountPrePoolingBullet > 0) BulletHellSharedPool.Instance.PrePoolMaxIterate(prefabBullet, amountPrePoolingBullet);
        //}

        //public void SetGlobalForce(Vector2 value)
        //{
        //    _shootThread.localDirectionalForce = globalDirectionalForce = value;
        //}

        //public void StartShoot(System.Action onFinishedShooting = null, System.Action onAllBulletDisappeared = null)
        //{
        //    if (_corShooting != null) StopCoroutine(_corShooting);
        //    _onFinishedShooting?.Invoke();

        //    if (_shootThread.CountBulletActive > 0)
        //    {
        //        _shootThread.RemoveAllBullets();
        //    }
        //    _onAllBulletDisappeared?.Invoke();

        //    //if (isUpdateBulletLimitOnStartShoot)
        //    //{
        //    //    int maxNumBullet = (bulletLimitAmount >= 0) ? bulletLimitAmount : (numWaves * numWays * numBulletsInPack);
        //    //    _shootThread.ChangeLimitNumBullet(maxNumBullet);
        //    //}

        //    _onFinishedShooting = onFinishedShooting;
        //    _onAllBulletDisappeared = onAllBulletDisappeared;

        //    OnStartShoot();

        //    _shootThread.moveAroundAnchorMode = moveAroundAnchorMode;
        //    _shootThread.autoReleaseTime = autoReleaseTime;
        //    _shootThread.localDirectionalForce = globalDirectionalForce;
        //    _corShooting = StartCoroutine(IEShoot());
        //}

        //public void CancelShoot()
        //{
        //    if (_corShooting != null) StopCoroutine(_corShooting);
        //    _onFinishedShooting?.Invoke();

        //    if (_shootThread.CountBulletActive > 0)
        //    {
        //        _shootThread.RemoveAllBullets();
        //    }
        //    _onAllBulletDisappeared?.Invoke();

        //    OnCancelShoot();
        //}

        //protected virtual void OnStartShoot() { }
        //protected virtual void OnCancelShoot() { }

        //public IEnumerator IEShoot()
        //{
        //    float GetNormValueOfRange(int index, int totalNum, float randomness) => (index + 0.5f + 0.5f * randomness * UnityEngine.Random.Range(-1f, 1f)) / totalNum;

        //    WaitForSeconds waitBetweenWaves = new WaitForSeconds(delayBetweenWaves);
        //    WaitForSeconds waitBetweenWays = new WaitForSeconds(delayBetweenWays);

        //    float waveMainAngle;
        //    float waveSpeed;
        //    float waveAccelerate;
        //    float waveTurnAngle;

        //    float wayShootAngle;
        //    float wayTurnAngle;

        //    Vector2 wayShootDirectionNorm;
        //    Vector2 wayShootDirectionPependicularVector = Vector2.zero;

        //    Transform transform = this.transform;
        //    Vector2 bulletShootPosition;
        //    float bulletShootAngle;
        //    Vector2 bulletShootDirectionNorm;
        //    float bulletSpeed;
        //    float bulletAccelerate;
        //    float bulletTurnAngle;

        //    GeneralBulletShootInfo bulletShootInfo = new GeneralBulletShootInfo
        //    {
        //        //startPosition = float3.zero,
        //        //velocity = float2.zero,
        //        //accelerate = 0f,
        //        //turnAngleRadian = 0f,

        //        initScale = initScale,
        //        scaleSpeed = scaleSpeed,

        //        moveAroundRadius = moveAroundRadius,
        //        moveAroundRadiusSpeed = moveAroundRadiusSpeed,
        //        moveAroundFrequency = moveAroundFreqency,
        //        moveAroundArcRadian = moveAroundArc * Mathf.Deg2Rad,
        //        moveAroundArcRadianSpeed = moveAroundArcSpeed * Mathf.Deg2Rad,
        //    };

        //    _isShooting = true;
        //    for (int waveIndex = 0; waveIndex < numWaves; waveIndex++)
        //    {
        //        if (shootTarget)
        //        {
        //            waveMainAngle = Vector2.SignedAngle(Vector2.right, shootTarget.position - transform.position);
        //        }
        //        else
        //        {
        //            waveMainAngle = Vector2.SignedAngle(Vector2.right, shootDirection);
        //        }

        //        waveSpeed = Mathf.LerpUnclamped(speedRange.x, speedRange.y, GetNormValueOfRange(waveIndex, numWaves, speedRandomness));
        //        waveAccelerate = Mathf.LerpUnclamped(accelerateRange.x, accelerateRange.y, GetNormValueOfRange(waveIndex, numWaves, accelerateRandomness));
        //        waveTurnAngle = Mathf.LerpUnclamped(turnAngleRange.x, turnAngleRange.y, GetNormValueOfRange(waveIndex, numWaves, turnAngleRandomness));

        //        for (int wayIndex = 0; wayIndex < numWays; wayIndex++)
        //        {
        //            wayShootAngle = waveMainAngle + angleOffset + angleRange * (-0.5f + GetNormValueOfRange(wayIndex, numWays, angleRandomness));

        //            wayTurnAngle = waveTurnAngle
        //                * ((isFlipTurnSpeedByOddWay && (wayIndex % 2 == 1)) ? -1f : 1f)
        //                * ((isFlipTurnSpeedByLeftRight && (wayIndex < numWays / 2)) ? -1f : 1f);

        //            math.sincos(wayShootAngle * Mathf.Deg2Rad, out wayShootDirectionNorm.y, out wayShootDirectionNorm.x);
        //            wayShootDirectionPependicularVector.x = wayShootDirectionNorm.y;
        //            wayShootDirectionPependicularVector.y = -wayShootDirectionNorm.x;

        //            for (int bulletIndex = 0; bulletIndex < numBulletsInPack; bulletIndex++)
        //            {
        //                bulletShootPosition = (Vector2)transform.position + wayShootDirectionPependicularVector * packSpacingVariance * (-0.5f + GetNormValueOfRange(bulletIndex, numBulletsInPack, packSpacingRandomness));

        //                bulletShootAngle = (wayShootAngle + packSpreadAngleVariance * (-0.5f + GetNormValueOfRange(bulletIndex, numBulletsInPack, packSpreadAngleRandomness)));
        //                math.sincos(Mathf.Deg2Rad * bulletShootAngle, out bulletShootDirectionNorm.y, out bulletShootDirectionNorm.x);

        //                bulletSpeed = waveSpeed + packSpeedVariance * (-0.5f + GetNormValueOfRange(bulletIndex, numBulletsInPack, packSpeedRandomness));
        //                bulletAccelerate = waveAccelerate + packAccelerateVariance * (-0.5f + GetNormValueOfRange(bulletIndex, numBulletsInPack, packAccelerateRandomness));
        //                bulletTurnAngle = wayTurnAngle + packTurnAngleSpeedVariance * (-0.5f + GetNormValueOfRange(bulletIndex, numBulletsInPack, packTurnAngleSpeedRandomness));

        //                bulletShootInfo.startPosition = bulletShootPosition;
        //                bulletShootInfo.velocity = bulletShootDirectionNorm * bulletSpeed;
        //                bulletShootInfo.accelerate = bulletAccelerate;
        //                bulletShootInfo.turnAngleRadian = bulletTurnAngle * Mathf.Deg2Rad;

        //                _shootThread.ShootBullet(_poolBullet.UseGameObject(), bulletShootInfo);
        //            }

        //            yield return waitBetweenWays;
        //        }
        //        yield return waitBetweenWaves;
        //    }
        //    _isShooting = false;

        //    _onFinishedShooting?.Invoke();
        //    _onFinishedShooting = null;

        //    if (_shootThread.CountBulletActive == 0)
        //    {
        //        OnFinishedShoot();
        //        _onAllBulletDisappeared?.Invoke();
        //        _onAllBulletDisappeared = null;
        //    }
        //}

        //private void Update()
        //{
        //    _shootThread?.Update();
        //}

        //private void LateUpdate()
        //{
        //    _shootThread?.LateUpdate();
        //}

        //protected virtual void OnBulletRemoved(BulletHellObjectBase bullet)
        //{
        //    bullet.gameObject.SetActive(false);
        //    _poolBullet.ReturnGameObject(bullet);
        //    if (!_isShooting && _shootThread.CountBulletActive == 0)
        //    {
        //        OnFinishedShoot();
        //        _onAllBulletDisappeared?.Invoke();
        //        _onAllBulletDisappeared = null;
        //    }
        //}

        //protected virtual void OnFinishedShoot() { }

        //protected abstract bool HandleOnBulletCollided(BulletHellObjectBase bullet, Collider2D target);

        //private void OnDrawGizmosSelected()
        //{
        //    Gizmos.color = Color.yellow;
        //    float waveMainAngle = Vector2.SignedAngle(Vector2.right, shootDirection);
        //    float wayShootAngle;
        //    Vector3 wayShootDirectionNorm = Vector3.zero;
        //    for (int wayIndex = 0; wayIndex < numWays; wayIndex++)
        //    {
        //        wayShootAngle = waveMainAngle + angleOffset + angleRange * (-0.5f + (wayIndex + 0.5f) / numWays);
        //        math.sincos(wayShootAngle * Mathf.Deg2Rad, out wayShootDirectionNorm.y, out wayShootDirectionNorm.x);
        //        Gizmos.DrawLine(transform.position, transform.position + wayShootDirectionNorm * 5f);
        //    }
        //}
    }
}