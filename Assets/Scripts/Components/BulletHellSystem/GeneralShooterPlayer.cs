using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DaiVQScript.BulletHellSystem
{
    public class GeneralShooterPlayer : MonoBehaviour 
    {
        [Header("Bullet config")]
        public float damage = 5f;
        public float critRatio = 0f;
        public float pushForce = 1f;
        public float pushDuration = 0.1f;
        public float damageMulEachHit = 1f;
        public int maxCountHitAllows = 1;

        [Header("Vfx")]
        public ParticleSystem vfxMuzzle = null;
        public Utilities.SFxPlayerRepeatSound sfxShoot = null;
        public ParticleSystem prefabVfxBulletHit = null;

        public class PlayerShootThread : GeneralShootThreadAbstract
        {
            [Header("Bullet config")]
            public float damage = 5f;
            public float critRatio = 0f;
            public float pushForce = 1f;
            public float pushDuration = 0.1f;

            [Header("Pierce")]
            public float damageMulEachHit = 1f;
            public int maxCountHitAllows = -1;
            private const int NUM_EXT_DATA_INT = 1;
            private const int NUM_EXT_DATA_FLOAT = 1;
            private const int INDEX_DATA_FLOAT_DAMAGE = 0;
            private const int INDEX_DATA_INT_COUNT_HIT = 0;

            [Header("Vfx")]
            public ParticleSystem prefabVfxBulletHit = null;

            private System.Action<PlayerShootThread> _onFinishedShooting = null;
            private System.Action<PlayerShootThread> _onAllBulletDisappeared = null;

            //[Header("Statistic")]
            public float CountTotalDamage { get; private set; } = 0f;
            public int CountTotalHits { get; private set; } = 0;
            public int CountTotalKills { get; private set; } = 0;

            public void StartShoot(System.Action<PlayerShootThread> onFinishedShooting, System.Action<PlayerShootThread> onAllBulletDisappeared)
            {
                _onFinishedShooting = onFinishedShooting;
                _onAllBulletDisappeared = onAllBulletDisappeared;

                CountTotalDamage = 0;
                CountTotalHits = 0;
                CountTotalKills = 0;

                TriggerShoot();
            }

            protected override void OnPrepareShootBullet(BulletHellObjectBase bullet)
            {
                base.OnPrepareShootBullet(bullet);
                bullet.extDataInts = new int[NUM_EXT_DATA_INT];
                bullet.extDataInts[INDEX_DATA_INT_COUNT_HIT] = 0;
                bullet.extDataFloats = new float[NUM_EXT_DATA_FLOAT];
                bullet.extDataFloats[INDEX_DATA_FLOAT_DAMAGE] = damage;
            }

            protected override bool HandleOnBulletCollided(BulletHellObjectBase bullet, Collider2D target)
            {
                if (target.TryGetComponent<ITakeDamage>(out ITakeDamage targetTakeDamage))
                {
                    Vector3 hitPosition = target.transform.position;
                    Vector2 direction = bullet.transform.up;
                    DaiVQObjectPool.Instance.PlayParticleOnce(prefabVfxBulletHit, hitPosition, Quaternion.LookRotation(Vector3.forward, direction));
                    float realDamage = targetTakeDamage.TakedDamage(bullet.extDataFloats[INDEX_DATA_FLOAT_DAMAGE], hitPosition, critRatio, OnTargetKilled);
                    if (pushDuration > 0.01f) targetTakeDamage.ApplyForce(direction * pushForce, pushDuration);
                    CountTotalDamage += realDamage;
                    CountTotalHits++;

                    bullet.extDataFloats[INDEX_DATA_FLOAT_DAMAGE] *= damageMulEachHit;
                    bullet.extDataInts[INDEX_DATA_INT_COUNT_HIT] += 1;

                    if (bullet.extDataFloats[INDEX_DATA_FLOAT_DAMAGE] < 1f || (maxCountHitAllows >= 0 && bullet.extDataInts[INDEX_DATA_INT_COUNT_HIT] >= maxCountHitAllows))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                void OnTargetKilled(Transform target)
                {
                    CountTotalKills++;
                }
            }

            protected override void OnFinishedShooting()
            {
                base.OnFinishedShooting();
                _onFinishedShooting?.Invoke(this);
                _onFinishedShooting = null;
            }

            protected override void OnAllBulletDisappeared()
            {
                base.OnAllBulletDisappeared();
                _onAllBulletDisappeared?.Invoke(this);
                _onAllBulletDisappeared = null;
            }
        }

        [SerializeField] private bool _isPrePoolBulletOnStart = false;
        public GeneralShootConfig shootConfig = default;
        private List<ShootThreadWrapper> _listShootThreads = new List<ShootThreadWrapper>();

        public class ShootThreadWrapper
        {
            public PlayerShootThread shootThread;
            public System.Action<PlayerShootThread> onAllBulletDisappeared;
        }

        [Sirenix.OdinInspector.Button("Setup")]
        public void SetUp()
        {
            if (_isPrePoolBulletOnStart && shootConfig.prefabBullet)
            {
                int maxNumBullet = ((shootConfig.bulletLimitAmount >= 0) ? shootConfig.bulletLimitAmount : (shootConfig.numWaves * shootConfig.numWays * shootConfig.numBulletsInPack));
                BulletHellSharedPool.Instance.AddPool(shootConfig.prefabBullet, maxNumBullet);
                BulletHellSharedPool.Instance.PrePoolMaxIterate(shootConfig.prefabBullet, maxNumBullet);
            }
        }

        public void StartShoot(System.Action<PlayerShootThread> onFinishedShooting = null, System.Action<PlayerShootThread> onAllBulletDisappeared = null)
        {
            PlayerShootThread playerShootThread = new PlayerShootThread();
            playerShootThread.SetUp(this, (GeneralShootConfig)shootConfig.Clone());

            ShootThreadWrapper shootThreadWrapper = new ShootThreadWrapper()
            {
                shootThread = playerShootThread,
                onAllBulletDisappeared = onAllBulletDisappeared,
            };
            shootThreadWrapper.shootThread.damage = damage;
            shootThreadWrapper.shootThread.critRatio = critRatio;
            shootThreadWrapper.shootThread.pushForce = pushForce;
            shootThreadWrapper.shootThread.pushDuration = pushDuration;
            shootThreadWrapper.shootThread.damageMulEachHit = 1f;
            shootThreadWrapper.shootThread.maxCountHitAllows = maxCountHitAllows;
            shootThreadWrapper.shootThread.prefabVfxBulletHit = prefabVfxBulletHit;
            _listShootThreads.Add(shootThreadWrapper);
            shootThreadWrapper.shootThread.StartShoot(onFinishedShooting, OnThreadHasAllBulletDisappread);

            if (vfxMuzzle != null) vfxMuzzle.Play();
            sfxShoot.Play();
        }

        private void OnThreadHasAllBulletDisappread(PlayerShootThread shootThread)
        {
            for (int i = _listShootThreads.Count - 1; i >= 0; i--)
            {
                if (_listShootThreads[i].shootThread == shootThread)
                {
                    _listShootThreads[i].onAllBulletDisappeared?.Invoke(shootThread);
                    _listShootThreads.RemoveAt(i);
                }
            }
        }

        private void Update()
        {
            for (int i = _listShootThreads.Count - 1; i >= 0; i--)
            {
                _listShootThreads[i].shootThread.Update();
            }
        }

        private void LateUpdate()
        {
            for (int i = _listShootThreads.Count - 1; i >= 0; i--)
            {
                _listShootThreads[i].shootThread.LateUpdate();
            }
        }

        public void CancelAllShoot()
        {
            for (int i = _listShootThreads.Count - 1; i >= 0; i--)
            {
                _listShootThreads[i].shootThread.CancelShoot();
            }
            _listShootThreads.Clear();
        }

        private void OnDestroy()
        {
            CancelAllShoot();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            float waveMainAngle;
            if (shootConfig.shootTarget)
            {
                waveMainAngle = Vector2.SignedAngle(Vector2.right, shootConfig.shootTarget.position - transform.position);
            }
            else if (shootConfig.isUseLocalShootDirection)
            {
                waveMainAngle = Vector2.SignedAngle(Vector2.right, transform.up);
            }
            else
            {
                waveMainAngle = Vector2.SignedAngle(Vector2.right, shootConfig.shootDirection);
            }
            float wayShootAngle;
            Vector3 wayShootDirectionNorm = Vector3.zero;
            for (int wayIndex = 0; wayIndex < shootConfig.numWays; wayIndex++)
            {
                wayShootAngle = waveMainAngle + shootConfig.angleOffset + shootConfig.angleRange * (-0.5f + (wayIndex + 0.5f) / shootConfig.numWays);
                math.sincos(wayShootAngle * Mathf.Deg2Rad, out wayShootDirectionNorm.y, out wayShootDirectionNorm.x);
                Gizmos.DrawLine(transform.position, transform.position + wayShootDirectionNorm * 5f);
            }
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button, Sirenix.OdinInspector.ButtonGroup]
        public void TestShoot()
        {
            StartShoot();
        }

        [Sirenix.OdinInspector.Button, Sirenix.OdinInspector.ButtonGroup]
        public void CancelShoot()
        {
            CancelAllShoot();
        }
#endif
    }
}