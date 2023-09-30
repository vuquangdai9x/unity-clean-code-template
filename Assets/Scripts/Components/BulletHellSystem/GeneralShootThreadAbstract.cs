using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DaiVQScript.BulletHellSystem
{
    public abstract class GeneralShootThreadAbstract
    {
        private GeneralBulletsController _bulletsController;
        private MonoBehaviour _context;
        private GeneralShootConfig _shootConfig;
        private Coroutine _corShooting;
        private PrefabStackPool<BulletHellObjectBase> _poolBullet;
        private bool _isShooting;

        public void SetUp(MonoBehaviour context, GeneralShootConfig shootConfig)
        {
            _context = context;
            _shootConfig = shootConfig;

            int maxNumBullet = ((_shootConfig.bulletLimitAmount >= 0) ? _shootConfig.bulletLimitAmount : (_shootConfig.numWaves * _shootConfig.numWays * _shootConfig.numBulletsInPack));
            _poolBullet = BulletHellSharedPool.Instance.AddPool(_shootConfig.prefabBullet, maxNumBullet);
            BulletHellSharedPool.Instance.PrePoolMaxIterate(_shootConfig.prefabBullet, maxNumBullet);

            _bulletsController = new GeneralBulletsController(maxNumBullet, HandleOnBulletCollided, OnBulletRemoved);

            _bulletsController.moveAroundAnchorMode = _shootConfig.moveAroundAnchorMode;
            _bulletsController.autoReleaseTime = _shootConfig.autoReleaseTime;
            _bulletsController.localDirectionalForce = _shootConfig.localDirectionalForce;
        }

        /// <summary>
        /// Must be called to shoot
        /// </summary>
        protected void TriggerShoot()
        {
            _corShooting = _context.StartCoroutine(IEShoot());
        }

        public void CancelShoot()
        {
            if (_corShooting != null) _context.StopCoroutine(_corShooting);
            _isShooting = false;
            OnFinishedShooting();
            if (_bulletsController.IsAlive)
            {
                _bulletsController.RemoveAllBullets();
                if (_bulletsController.IsAlive) _bulletsController.Destroy();
            }
        }

        private IEnumerator IEShoot()
        {
            float GetNormValueOfRange(int index, int totalNum, float randomness) => (index + 0.5f + 0.5f * randomness * UnityEngine.Random.Range(-1f, 1f)) / totalNum;

            WaitForSeconds waitBetweenWaves = new WaitForSeconds(_shootConfig.delayBetweenWaves);
            WaitForSeconds waitBetweenWays = new WaitForSeconds(_shootConfig.delayBetweenWays);

            float waveMainAngle;
            float waveSpeed;
            float waveAccelerate;
            float waveTurnAngle;
            //float timeShootWave;

            float wayShootAngle;
            float flip;

            Vector2 wayShootDirectionNorm;
            Vector2 wayShootDirectionPependicularVector = Vector2.zero;

            Transform transform = _context.transform;
            Vector2 bulletShootPosition;
            float bulletShootAngle;
            Vector2 bulletShootDirectionNorm;
            float bulletSpeed;
            float bulletAccelerate;
            float bulletTurnAngle;

            int countBulletShootOneFrame = 0;

            BulletHellObjectBase bullet;

            GeneralBulletShootInfo bulletShootInfo = new GeneralBulletShootInfo
            {
                //startPosition = float3.zero,
                //velocity = float2.zero,
                //accelerate = 0f,
                //turnAngleRadian = 0f,

                initScale = _shootConfig.initScale,
                scaleSpeed = _shootConfig.scaleSpeed,

                moveAroundRadius = _shootConfig.moveAroundRadius,
                moveAroundRadiusSpeed = _shootConfig.moveAroundRadiusSpeed,
                moveAroundFrequency = _shootConfig.moveAroundFreqency,
                moveAroundArcRadian = _shootConfig.moveAroundArcRadian,
                moveAroundArcRadianSpeed = _shootConfig.moveAroundArcRadianSpeed,
            };

            _isShooting = true;

            for (int waveIndex = 0; waveIndex < _shootConfig.numWaves; waveIndex++)
            {
                if (_shootConfig.shootTarget)
                {
                    waveMainAngle = Vector2.SignedAngle(Vector2.right, _shootConfig.shootTarget.position - transform.position);
                }
                else if (_shootConfig.isUseLocalShootDirection)
                {
                    waveMainAngle = Vector2.SignedAngle(Vector2.right, transform.up);
                }
                else
                {
                    waveMainAngle = Vector2.SignedAngle(Vector2.right, _shootConfig.shootDirection);
                }

                //timeShootWave = Time.time;
                waveSpeed = Mathf.LerpUnclamped(_shootConfig.speedRange.x, _shootConfig.speedRange.y, GetNormValueOfRange(waveIndex, _shootConfig.numWaves, _shootConfig.speedRandomness));
                waveAccelerate = Mathf.LerpUnclamped(_shootConfig.accelerateRange.x, _shootConfig.accelerateRange.y, GetNormValueOfRange(waveIndex, _shootConfig.numWaves, _shootConfig.accelerateRandomness));
                waveTurnAngle = Mathf.LerpUnclamped(_shootConfig.turnAngleRange.x, _shootConfig.turnAngleRange.y, GetNormValueOfRange(waveIndex, _shootConfig.numWaves, _shootConfig.turnAngleRandomness));

                int[] shuffledWayIndexes = _shootConfig.isShuffleWayIndex ? DaiVQScript.Utilities.RandomUtility.GetShuffledIndexArray(_shootConfig.numWays) : null;
                int wayIndex;
                for (int wayI = 0; wayI < _shootConfig.numWays; wayI++)
                {
                    wayIndex = _shootConfig.isShuffleWayIndex ? shuffledWayIndexes[wayI] : wayI;
                    wayShootAngle = waveMainAngle + _shootConfig.angleOffset + _shootConfig.angleRange * (-0.5f + GetNormValueOfRange(wayIndex, _shootConfig.numWays, _shootConfig.angleRandomness));

                    flip = ((_shootConfig.isFlipTurnSpeedByOddWay && (wayIndex % 2 == 1)) ? -1f : 1f);

                    if (_shootConfig.isFlipTurnSpeedByLeftRight)
                    {
                        if (_shootConfig.numWays % 2 == 1 && wayIndex == _shootConfig.numWays / 2)
                        {
                            flip = 0f;
                        }
                        else
                        {
                            if (wayIndex < _shootConfig.numWays / 2)
                            {
                                flip *= -1f;
                            }
                        }
                    }

                    math.sincos(wayShootAngle * Mathf.Deg2Rad, out wayShootDirectionNorm.y, out wayShootDirectionNorm.x);
                    wayShootDirectionPependicularVector.x = wayShootDirectionNorm.y;
                    wayShootDirectionPependicularVector.y = -wayShootDirectionNorm.x;

                    int indexIncludeFlip;
                    int[] shuffledIndexes = _shootConfig.isShuffleBulletIndex ? DaiVQScript.Utilities.RandomUtility.GetShuffledIndexArray(_shootConfig.numBulletsInPack) : null;
                    int bulletIndex;
                    for (int i = 0; i < _shootConfig.numBulletsInPack; i++)
                    {
                        bulletIndex = _shootConfig.isShuffleBulletIndex ? shuffledIndexes[i] : i;

                        indexIncludeFlip = (flip >= 0f) ? bulletIndex : (_shootConfig.numBulletsInPack - bulletIndex - 1);

                        bulletShootPosition = (Vector2)transform.position + wayShootDirectionPependicularVector * _shootConfig.packSpacingVariance * (-0.5f + GetNormValueOfRange(indexIncludeFlip, _shootConfig.numBulletsInPack, _shootConfig.packSpacingRandomness));

                        bulletShootAngle = (wayShootAngle + _shootConfig.packSpreadAngleVariance * (-0.5f + GetNormValueOfRange(indexIncludeFlip, _shootConfig.numBulletsInPack, _shootConfig.packSpreadAngleRandomness)));
                        math.sincos(Mathf.Deg2Rad * bulletShootAngle, out bulletShootDirectionNorm.y, out bulletShootDirectionNorm.x);

                        bulletSpeed = waveSpeed + _shootConfig.packSpeedVariance * (-0.5f + GetNormValueOfRange(i, _shootConfig.numBulletsInPack, _shootConfig.packSpeedRandomness));
                        bulletAccelerate = waveAccelerate + _shootConfig.packAccelerateVariance * (-0.5f + GetNormValueOfRange(i, _shootConfig.numBulletsInPack, _shootConfig.packAccelerateRandomness));
                        bulletTurnAngle = waveTurnAngle * flip + _shootConfig.packTurnAngleSpeedVariance * (-0.5f + GetNormValueOfRange(indexIncludeFlip, _shootConfig.numBulletsInPack, _shootConfig.packTurnAngleSpeedRandomness));

                        bulletShootInfo.startPosition = bulletShootPosition;
                        bulletShootInfo.velocity = bulletShootDirectionNorm * bulletSpeed;
                        bulletShootInfo.accelerate = bulletAccelerate;
                        bulletShootInfo.turnAngleRadian = bulletTurnAngle * Mathf.Deg2Rad;

                        bullet = _poolBullet.UseGameObject();
                        OnPrepareShootBullet(bullet);
                        _bulletsController.ShootBullet(bullet, bulletShootInfo);

                        countBulletShootOneFrame += 1;
                        if (_shootConfig.maxNumBulletShootPerFrame > 0 && countBulletShootOneFrame % _shootConfig.maxNumBulletShootPerFrame == 0) yield return null;
                    }

                    if (_shootConfig.delayBetweenWays >= 0f)
                    {
                        yield return waitBetweenWays;
                        countBulletShootOneFrame = 0;
                    }
                }
                yield return waitBetweenWaves;
                countBulletShootOneFrame = 0;
            }
            _isShooting = false;

            OnFinishedShooting();

            if (_bulletsController.IsAlive && _bulletsController.CountBulletActive == 0)
            {
                _bulletsController.Destroy();
                OnAllBulletDisappeared();
            }
        }
        protected virtual void OnPrepareShootBullet(BulletHellObjectBase bullet) { }

        public void Update() => _bulletsController?.Update();
        public void LateUpdate() => _bulletsController?.LateUpdate();

        protected virtual void OnBulletRemoved(BulletHellObjectBase bullet)
        {
            _poolBullet.ReturnGameObject(bullet);
            if (!_isShooting && _bulletsController.IsAlive && _bulletsController.CountBulletActive == 0)
            {
                _bulletsController.Destroy();
                OnAllBulletDisappeared();
            }
        }
        protected virtual void OnFinishedShooting() { }
        protected virtual void OnAllBulletDisappeared() { }
        protected abstract bool HandleOnBulletCollided(BulletHellObjectBase bullet, Collider2D target);
    }
}